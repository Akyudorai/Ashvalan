using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class CreditsScene : MonoBehaviour
{
    private void Start()
    {
        //StartCoroutine(DelayTransition());
    }
    
    public void ToMenu()
    {
        TransitionHandler.Instance.StartSceneTransition("menu", 2f);
    }

    private IEnumerator DelayTransition()
    {
        yield return new WaitForSeconds(5f);

        TransitionHandler.Instance.StartSceneTransition("menu", 2f);
    }
}
