using UnityEngine;
using System.Collections;

public class CinematicCamera : MonoBehaviour
{
    // - Component References
    public Camera cam;
    private Vector3 originalPos;

    [Header("Screenshake Settings")]
    public float shake = 0;
    public float shakeAmount = 0.7f;
    private float decreaseFactor = 1.0f;
   
    [Header("Smash Settings")]
    public float smashZoomAmount = 2.0f;
    public float smashDuration = 0.5f;
    private float smashTimer = 0f;
    private float originalZoom;
    private bool isSmashing = false;
    private Coroutine smashRoutine;

    [Header("Time Dilation Settings")]
    public float timeDilationFactor = 0.5f;
    public float timeDilationDuration = 0.5f;
    private float timeDilationTimer = 0f;
    private bool isTimeDilating = false;

    [Header("Focus Settings")]
    private Vector3 focusPoint;
    //private Vector3 centerPoint;

    // - Monobehaviour
    private void Start() 
    {
        originalPos = cam.transform.position;
        originalZoom = cam.orthographicSize;
    }

    private void Update() 
    {          
        //if (!Game.isCombatActive) return;

        // - Handle Time Dilation
        HandleTimeDilation();

        // - Handle Cinematic Camera
        //HandleCinematicCamera();
    }

    private void HandleCinematicCamera() 
    {   
        Vector3 midpoint = CalculateMidPoint();
        float targetZoom = CalculateFocusDistance();

        // - If nothing else is overriding the camera, smoothly move to center point
        if (!isSmashing) 
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, midpoint, Time.unscaledDeltaTime * 3f);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.unscaledDeltaTime * 3f);
        }
    }

    private Vector3 CalculateMidPoint() 
    {
        // - Default to oringal zoom if either reference is missing
        if (Game.heroRef == null || Game.playerRef == null)
            return originalPos;

        Vector3 centerPoint = Vector3.zero;

        // - Calcualte Center Point between Player and Hero
        if (GetPlayerPosition() != Vector3.zero && GetHeroPosition() != Vector3.zero)
        {               
            centerPoint = (GetPlayerPosition() + GetHeroPosition()) / 2;
            centerPoint.y += 3f;
            centerPoint.z = originalPos.z;            
        }
       
        // - If neither exists, return to original position    
        else 
        {   
            centerPoint = originalPos;
        }

        return centerPoint;
    }

    private float CalculateFocusDistance() 
    {
        // - Default to oringal zoom if either reference is missing
        if (Game.heroRef == null || Game.playerRef == null)
            return originalZoom;

        // - World space extents between Player and Hero
        float width = Mathf.Abs(GetPlayerPosition().x - GetHeroPosition().x);
        float height = Mathf.Abs(GetPlayerPosition().y - GetHeroPosition().y);

        // - Padding in world units
        float padding = 2.0f;

        // - Calculate required orthographic size based on extents and aspect ratio
        float requiredHalfHeight = Mathf.Max(height *0.5f, (width * 0.5f) / Mathf.Max(0.0001f, cam.aspect));
        float desiredSize = Mathf.Max(requiredHalfHeight + padding, 8.0f); // - Minimum size of 5  

        float targetZoom = cam.orthographic ? Mathf.Max(0.01f, desiredSize) : originalZoom;
        return targetZoom;
    }

    private Vector3 GetPlayerPosition() 
    {
        if (Game.playerRef != null) 
        {
            return Game.playerRef.transform.position;
        }

        return Vector3.zero;
    }

    private Vector3 GetHeroPosition() 
    {
        if (Game.heroRef != null) 
        {
            return Game.heroRef.transform.position;
        }

        return Vector3.zero;
    }

    // public void ApplyScreenShake(float intensity, float duration)
    // {
    //     shake = duration;
    //     shakeAmount = intensity;
    // }

    // private void HandleScreenShake() 
    // {   
    //     if (shake > 0) 
    //     {
    //         transform.position = originalPos + Random.insideUnitSphere * shakeAmount;
    //         shake -= Time.deltaTime * decreaseFactor;            
    //     }

    //     else 
    //     {
    //         shake = 0.0f;
    //         transform.position = originalPos;
    //     }
    // }

    public void ApplyTimeDilation(float intensity, float duration)
    {
        isTimeDilating = true;
        timeDilationDuration = duration;
        timeDilationFactor = intensity;
        timeDilationTimer = 0f;
    }

    private void HandleTimeDilation() 
    {
        if (isTimeDilating) 
        {
            Time.timeScale = timeDilationFactor;

            timeDilationTimer += Time.unscaledDeltaTime;
            if (timeDilationTimer >= timeDilationDuration) 
            {
                Time.timeScale = 1.0f;
                isTimeDilating = false;
            }
        }
    }

    public void ApplySmashEffect(GameObject player, GameObject hero, float duration) 
    {
        isSmashing = true;
        smashDuration = duration;
        focusPoint = (player.transform.position + hero.transform.position) / 2;
        smashTimer = 0f;

        if (smashRoutine != null) StopCoroutine(smashRoutine);
        smashRoutine = StartCoroutine(SmashEffectCoroutine(player, hero));
    }

    private IEnumerator SmashEffectCoroutine(GameObject player, GameObject hero) 
    {
        isSmashing = true;
        smashTimer = 0f;

        // - Capture starting values
        Vector3 startPos = cam.transform.position;
        float startZoom = cam.orthographicSize;
        Vector3 focusPoint = (player.transform.position + hero.transform.position) / 2;
        focusPoint.z = originalPos.z;
        focusPoint.y += 1.5f;
        float targetZoom = smashZoomAmount;

        // - Define phase durations
        float inDuration = 0.5f;
        float holdDuration = smashDuration;
        float outDuration = 1f;
    
        // - Smash In
        float t = 0f;
        while (t < 1f)
        {
            // - Update focus point in case player/hero moved
            focusPoint = (player.transform.position + hero.transform.position) / 2;
            focusPoint.z = originalPos.z;
            focusPoint.y += 1.5f;

            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, inDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            cam.transform.position = Vector3.Lerp(startPos, focusPoint, eased);
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, eased);
            yield return null;
        }

        // - Hold Phase
        float holdTimer = 0f;
        cam.transform.position = focusPoint;
        cam.orthographicSize = targetZoom;
        while (holdTimer < holdDuration)
        {            
            // - Update focus point in case player/hero moved
            focusPoint = (player.transform.position + hero.transform.position) / 2;
            focusPoint.z = originalPos.z;
            focusPoint.y += 1.5f;

            // - Update camera position and zoom to follow
            float eased = 1f; // No easing during hold
            cam.transform.position = Vector3.Lerp(startPos, focusPoint, eased);
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, eased);

            
            holdTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // - Smash Out
        t = 0f;        
        while (t < 1f)
        {
            // - Update focus point in case player/hero moved
            focusPoint = (player.transform.position + hero.transform.position) / 2;
            focusPoint.z = originalPos.z;
            focusPoint.y += 1.5f;
            
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, outDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            cam.transform.position = Vector3.Lerp(focusPoint, originalPos, eased);
            cam.orthographicSize = Mathf.Lerp(targetZoom, originalZoom, eased);
            yield return null;
        }

        // - Restore Defaults
        cam.orthographicSize = originalZoom;
        cam.transform.position = originalPos;
        isSmashing = false;
        smashRoutine = null;        
    }
}
