using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        TransitionHandler.Instance.StartSceneTransition("game");
    }

    public void ShowCredits()
    {
        TransitionHandler.Instance.StartSceneTransition("credits");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
