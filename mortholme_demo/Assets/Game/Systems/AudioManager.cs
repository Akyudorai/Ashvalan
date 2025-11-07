using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource source;

    public AudioClip sword, dash, chain, spell, burn, death;

    public AudioClip hDeath, hSwing, hSword, hBlock1, hBlock2, hBlock3, hRoll;

    public void PlayClip(string name)
    {
        switch (name)
        {
            case "Sword":
                source.PlayOneShot(sword);
                break;
            case "Dash":
                source.PlayOneShot(dash);
                break;
            case "Chain":
                source.PlayOneShot(chain);
                break;
            case "Spell":
                source.PlayOneShot(spell);
                break;
            case "Burn":
                source.PlayOneShot(burn);
                break;
            case "Death":
                source.PlayOneShot(death);
                break;
            case "hDeath":
                source.PlayOneShot(hDeath);
                break;
            case "hSwing":
                source.PlayOneShot(hSwing);
                break;
            case "hSword":
                source.PlayOneShot(hSword);
                break;
            case "hBlock":
                float random = Random.Range(0, 100);
                AudioClip result = (random < 33f) ? hBlock1 : (random < 66f) ? hBlock2 : hBlock3;
                source.PlayOneShot(result);
                break;
            case "hRoll":
                source.PlayOneShot(hRoll);
                break;
        }
    }
}
