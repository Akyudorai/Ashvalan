using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionHandler : MonoBehaviour
{
    public static TransitionHandler Instance;

    public Image fadeImage;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator Fade(float from, float to, float duration, Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // - Block UI input while fading
        fadeImage.raycastTarget = true;

        float t = 0f;
        var c = fadeImage.color;

        // - Guard against zero duration
        if (duration <= 0f)
        {
            c.a = to;
            fadeImage.color = c;
            fadeImage.raycastTarget = to > 0.01f;
            onComplete?.Invoke();
            yield break;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
        fadeImage.raycastTarget = to > 0.01f;
        onComplete?.Invoke();
    }

    public void StartFadeIn(float duration = 0.5f, Action onComplete = null)
    {
        StartCoroutine(FadeIn(duration, onComplete));
    }

    private IEnumerator FadeIn(float duration = 0.5f, Action onComplete = null)
    {
        // - Transition from opaque (1) to transparent (0)
        yield return StartCoroutine(Fade(1f, 0f, duration, onComplete));

        // - Separate due to event being assigned during the load transition
        Game.OnTransitionComplete?.Invoke();
    }

    public void StartFadeOut(float duration = 0.5f, Action onComplete = null)
    {
        StartCoroutine(FadeOut(duration, onComplete));
    }

    private IEnumerator FadeOut(float duration = 0.5f, Action onComplete = null)
    {
        // Transition from transparent (0) to opaque (1)
        yield return StartCoroutine(Fade(0f, 1f, duration, onComplete));
    }

    public void StartSceneTransition(string sceneName, float duration = 2f)
    {
        StartCoroutine(FadeOutThenLoad(sceneName, duration));
    }

    // Fade out, then load scene, then fade in
    private IEnumerator FadeOutThenLoad(string sceneName, float fadeDuration = 2f)
    {
        yield return StartCoroutine(FadeOut(fadeDuration));
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitForSeconds(fadeDuration); // - FadeIn begins before scene transition is complete. This acts as a net to delay the fade in until fully loaded
        yield return StartCoroutine(FadeIn(fadeDuration));
    }

}
