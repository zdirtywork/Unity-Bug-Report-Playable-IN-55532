# Unity-Bug-Report-Playable-IN-55532

## About this issue

After setting the time of the AnimationClipPlayable, the time of the AnimationClipPlayable will not automatically advance in the next frame.

**NOTE**: I set the time of the AnimationClipPlayable in the `PrepareFrame` method of a ScriptPlayable, and the AnimationClipPlayable is a child node of the ScriptPlayable. According to the rules of Playables, at this point, the `PrepareFrame` and `ProcessFrame` methods of the AnimationClipPlayable should not have been executed yet, so the impact caused by `SetTime` should theoretically be consumed in this frame and should not carry over to the next frame.

## How to reproduce

1. Open the scene `Test`.
2. Enter Play mode.
3. Select the GameObject "SKM_Manny" in the Hierarchy, expand the "SyncTimeTest" component in the Inspector.
4. Click the "Sync Once" button in the Inspector.
5. Observe the console output.
Expected result: The results of GetTime in two consecutive PrepareFrame outputs are not equal.
Actual result: The results of GetTime in two consecutive PrepareFrame outputs are equal.