
using UnityEngine;

public static class Game
{
    public delegate void TransitionDelegate();
    public static TransitionDelegate OnTransitionComplete;

    public static GameObject playerRef, heroRef;

    public static int currentLevel = 0;
    public static bool isCombatActive = false;
    public static bool isPlayerResetReady = false;
    public static bool isDialogueActive = false;
    public static bool isCinematicActive = false;
}