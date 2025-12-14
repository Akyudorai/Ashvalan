using UnityEngine;
using System.Collections;
using Steamworks;

public class SteamScript : MonoBehaviour
{
    // - Callbacks
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;


    // - Monobehaviour
    private void Start() 
    {
        if (SteamManager.Initialized) 
        {
            //string name = SteamFriends.GetPersonaName();
            //Debug.Log("Steamworks is initialized. Player name: " + name);
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }
    }

    public void OpenSteamOverlay() 
    {
        if (SteamManager.Initialized) 
        {
            SteamFriends.ActivateGameOverlay("friends");
        }
    }

    // - Steam Callbacks
    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback) 
    {
        if (pCallback.m_bActive != 0) 
        {
            Debug.Log("Steam Overlay has been activated");
        }
        else 
        {
            Debug.Log("Steam Overlay has been closed");
        }
    }
}
