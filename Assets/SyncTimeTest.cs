using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ## About this issue
// After setting the time of the AnimationClipPlayable, the time of the AnimationClipPlayable will not automatically advance in the next frame.
// NOTE: I set the time of the AnimationClipPlayable in the `PrepareFrame` method of a ScriptPlayable,
// and the AnimationClipPlayable is a child node of the ScriptPlayable.
// According to the rules of Playables, at this point,
// the `PrepareFrame` and `ProcessFrame` methods of the AnimationClipPlayable should not have been executed yet,
// so the impact caused by `SetTime` should theoretically be consumed in this frame and should not carry over to the next frame.
// 
// ## How to reproduce
// 1. Open the scene `Test`.
// 2. Enter Play mode.
// 3. Select the GameObject "SKM_Manny" in the Hierarchy, expand the "SyncTimeTest" component in the Inspector.
// 4. Click the "Sync Once" button in the Inspector.
// 5. Observe the console output.
// Expected result: The results of GetTime in two consecutive PrepareFrame outputs are not equal.
// Actual result: The results of GetTime in two consecutive PrepareFrame outputs are equal.

public class SyncControllerBehaviour : PlayableBehaviour
{
    public bool syncOnce;

    private AnimationClipPlayable _input;
    private ulong _syncedFrameId;


    public void Init(AnimationClipPlayable input)
    {
        _input = input;
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);

        if (!syncOnce)
        {
            if (info.frameId == _syncedFrameId + 1)
            {
                // The input has been synced at the last frame
                Debug.Log($"PrepareFrame#{info.frameId}  GetTime()={_input.GetTime():F3}");
            }

            return;
        }

        //syncOnce = false;

        var deltaTime = info.deltaTime * info.effectiveSpeed * _input.GetSpeed();
        var time = _input.GetTime() + deltaTime;
        _input.SetTime(time);
        _syncedFrameId = info.frameId;

        // The input is synced at the current frame
        Debug.Log($"PrepareFrame#{info.frameId}  SetTime({time:F3}), GetTime()={_input.GetTime():F3}");
    }
}

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class SyncTimeTest : MonoBehaviour
{
    public AnimationClip clip;


    private PlayableGraph _graph;
    private SyncControllerBehaviour _syncCtrlBrhaviour;
    private AnimationClipPlayable _clipPlayable;

    public void SyncOnce()
    {
        _syncCtrlBrhaviour.syncOnce = true;
    }

    private void Start()
    {
        _graph = PlayableGraph.Create(GetType().Name);
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _clipPlayable = AnimationClipPlayable.Create(_graph, clip);

        var scriptPlayable = ScriptPlayable<SyncControllerBehaviour>.Create(_graph, 1);
        scriptPlayable.ConnectInput(0, _clipPlayable, 0, 1);

        _syncCtrlBrhaviour = scriptPlayable.GetBehaviour();
        _syncCtrlBrhaviour.Init(_clipPlayable);
        var animOutput = AnimationPlayableOutput.Create(_graph, "Animation", GetComponent<Animator>());
        animOutput.SetSourcePlayable(scriptPlayable);

        _graph.Play();
    }

    private void OnAnimatorMove() { }

    private void LateUpdate()
    {
        if (_syncCtrlBrhaviour.syncOnce)
        {
            // The input has been synced at the current frame
            Debug.Log($"LateUpdate  GetTime()={_clipPlayable.GetTime():F3}");
            _syncCtrlBrhaviour.syncOnce = false;
        }
    }

    private void OnDestroy()
    {
        if (_graph.IsValid()) _graph.Destroy();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SyncTimeTest))]
class SyncTimeTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var syncTimeTest = (SyncTimeTest)target;
        if (GUILayout.Button("Sync Once"))
        {
            syncTimeTest.SyncOnce();
        }
    }
}
#endif
