using System.Collections;
using UnityEngine;

using UnityEngine.UI;

public class ThunderController : MonoBehaviour
{
    public Image thunderImage;
    public AudioSource source;
    public AudioClip thunder2, thunder3;

    // - Timing
    public float minInterval = 3f;
    public float maxInterval = 8f;


    // - Thunder Flash
    public float flashInDuration = 0.05f;
    public float holdDuration = 0.05f;
    public float flashOutDuration = 0.3f;
    public float brightMultiplier = 2f;

    private Color baseColor;
    private Coroutine thunderRoutine;

    private void Awake()
    {
        if (thunderImage != null)
        {
            baseColor = thunderImage.color;
            baseColor.a = 0f;
            thunderImage.color = baseColor;
        }
    }

    private void Start()
    {
        StartThunder();
    }

    public void StartThunder()
    {
        if (thunderImage == null) return;
        if (thunderRoutine != null) StopCoroutine(thunderRoutine);
        thunderRoutine = StartCoroutine(ThunderLoop());
    }

    private IEnumerator ThunderLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
            yield return StartCoroutine(FlashOnce());
        }
    }
    
    private IEnumerator FlashOnce()
    {
        if (thunderImage == null) yield break;

        // - Play Audio Effect
        int random = Random.Range(0, 100);
        var clip = (random < 50) ? thunder2 : thunder3;
        source.PlayOneShot(clip);

        // - Build Color
        Color bright = new Color(
            Mathf.Clamp01(baseColor.r * brightMultiplier),
            Mathf.Clamp01(baseColor.g * brightMultiplier),
            Mathf.Clamp01(baseColor.b * brightMultiplier),
            0f
        );

        // - Flash In
        float t = 0f;
        while (t < flashInDuration)
        {
            t += Time.deltaTime;
            float a = flashInDuration <= 0f ? 1f : Mathf.Lerp(0f, 1f, t / flashInDuration);
            var c = bright;
            c.a = a;
            thunderImage.color = c;
            yield return null;
        }

        // - Full Flash
        var full = bright;
        full.a = 1f;
        thunderImage.color = full;

        // - Hold
        if (holdDuration > 0f) yield return new WaitForSeconds(holdDuration);

        // - Fade Out
        t = 0f;
        Color start = thunderImage.color;
        while (t < flashOutDuration)
        {
            t += Time.deltaTime;
            float a = flashOutDuration <= 0f ? 0f : Mathf.Lerp(1f, 0f, t / flashOutDuration);
            var c = start;
            c.a = a;
            thunderImage.color = c;
            yield return null;
        }

        // - Full Transparent
        var end = thunderImage.color;
        end.a = 0f;
        thunderImage.color = end;
    }

}
