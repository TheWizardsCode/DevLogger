using UnityEngine;
using UnityEditor.Animations;

public class AnimationUtil
{
  // is the given object an internal animation object?
  public static bool isInternalAnimationObject(UnityEngine.Object obj)
  {
    return obj is AnimatorStateMachine || obj is AnimatorStateTransition || obj is AnimatorState;
  }

  public static bool isAnimationObject(UnityEngine.Object obj)
  {
    return obj is AnimationClip || obj is AnimatorController || obj is AnimatorStateMachine || obj is AnimatorStateTransition || obj is AnimatorState;
  }
}