using UnityEngine;

public class Screenshake : MonoBehaviour
{
    public Camera cam;
    public float shake = 0;
    public float shakeAmount = 0.7f;
    private float decreaseFactor = 1.0f;

    private Vector3 originalPos;

    private void Start() 
    {
        originalPos = cam.transform.localPosition;
    }

    private void Update() 
    {
        if (shake > 0) 
        {
            cam.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;            
        }

        else 
        {
            shake = 0.0f;
            cam.transform.localPosition = originalPos;
        }
    }
}
