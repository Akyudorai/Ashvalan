using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        TransitionHandler.Instance.StartSceneTransition("game");
    }

    public void ShowCredits()
    {
        
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
