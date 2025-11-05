using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{       
    public static GameManager Instance;

    public GameObject heroPrefab;
    public Transform heroSpawnPoint;
    public Transform heroSpawnDestination;
    public LayerMask exclusionLayers;

    public GameObject heroObj;
    public GameObject playerObj;

    public GameObject heroRespawnTrigger;

    [Header("Torches")]
    public GameObject[] torches = new GameObject[10];    

    private void Awake() 
    {
        Instance = this;
    }

    private void Update() 
    {
        // - If the hero is dead and the player is ready to reset, spawn the hero
        if (heroObj == null && Game.isPlayerResetReady)
        {
            SpawnHero();
        }
        
        // - Toggle on Respawn Trigger if Hero is Dead
        if (heroObj == null && heroRespawnTrigger.activeSelf == false) 
        {
            heroRespawnTrigger.SetActive(true);
        }
    }

    public void SpawnHero()
    {
        if (heroSpawnPoint == null) return;

        // - Instantiate the Hero
        heroObj = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
        heroObj.GetComponent<HeroBehavior>().MoveTo(heroSpawnDestination.position);
        InterfaceManager.Instance.heroHealth = heroObj.GetComponent<HealthScript>();

        // - Delay combat start to allow for hero to reach destination
        StartCoroutine(StartCombatRoutine());

        // - Turn off respawn trigger
        heroRespawnTrigger.SetActive(false);

        // - Reset Player Health
        playerObj.GetComponent<HealthScript>().currentHealth = playerObj.GetComponent<HealthScript>().maxHealth;
    }
    
    public void LevelUp()
    {
        if (Game.currentLevel < 10)
        {
            torches[Game.currentLevel].SetActive(true); // - Light the torch for the current level
            Game.currentLevel += 1; // - Increment the difficulty level
            heroRespawnTrigger.SetActive(true); // - Enable respawn trigger for next fight
        }
               
    }

    // - Temporary Method to Start Combat after Spawning Hero
    private IEnumerator StartCombatRoutine() 
    {
        yield return new WaitForSeconds(7f);
        InterfaceManager.Instance.RefreshUI();
        BeginCombat();
    }

    public void BeginCombat() 
    {
        Game.isCombatActive = true; // - Initiate Combat         
        heroObj.GetComponent<Rigidbody2D>().excludeLayers = exclusionLayers; // - Stop hero from walking through walls
    }

    public void ClearHero() 
    {
        if (heroObj != null) 
        {
            Destroy(heroObj);
        }
    }
}
