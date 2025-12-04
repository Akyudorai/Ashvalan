using System.Collections.Generic;
using UnityEngine;

public class CreditsParallax : MonoBehaviour
{
    public GameObject[] backgroundLayers;
    
    public float repeatValue = -30f;
    public float gain = 60f;

    private void Update()
    {        
        foreach (GameObject go in backgroundLayers)
        {
            go.transform.position -= Vector3.up * Time.deltaTime;
            if (go.transform.position.y <= repeatValue) go.transform.position += Vector3.up * gain;
        }   
    }
}
