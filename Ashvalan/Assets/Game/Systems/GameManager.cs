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

    [Header("Game Start Settings")]
    public GameObject heroRespawnTrigger;
    public bool triggerShouldBeOnAtStart = false;

    public delegate void CombatStartDelegate();
    public CombatStartDelegate OnCombatStart;   

    [Header("Torches")]
    public GameObject[] torches = new GameObject[10];    

    private void Awake() 
    {
        Instance = this;

        if (!triggerShouldBeOnAtStart)
            Game.isCinematicActive = true;

        if (triggerShouldBeOnAtStart)
            heroRespawnTrigger.SetActive(true);

        Game.OnTransitionComplete += OnSceneLoaded;        
    }

    private void Update() 
    {
        // - If the hero is dead and the player is ready to reset, spawn the hero
        if (heroObj == null && Game.isPlayerResetReady)
        {
            SpawnHero();
        }
        
        if (heroRespawnTrigger.activeSelf == true && Game.isCombatActive)
        {
            heroRespawnTrigger.SetActive(false);
        }
        
    }

    public void OnSceneLoaded()
    {
        // - Clear all subscribed events to prevent multiple triggers
        Game.OnTransitionComplete = null;

        // - Start Opening Cinematic        
        SpawnHero();       
    }

    public IEnumerator EndCinematic()
    {
        yield return new WaitForSeconds(2.5f);

        // - Face towards where hero would emerge 
        float direction = -1f;

        Vector3 playerScale = playerObj.transform.localScale;
        playerScale.x = Mathf.Abs(playerScale.x) * direction;
        playerObj.transform.localScale = playerScale;

        // - Start Dialogue
        GameOver();
    }

    public void SpawnHero()
    {
        if (Game.currentLevel == 10)
        {
            // - Turn off respawn trigger
            heroRespawnTrigger.SetActive(false);
    
            StartCoroutine(EndCinematic());
            return;
        }

        if (heroSpawnPoint == null) return;

        // - Instantiate the Hero
        heroObj = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
        InterfaceManager.Instance.heroHealth = heroObj.GetComponent<HealthScript>();

        // - Subscribe to OnMoveComplete to begin Dialogue
        HeroBehaviorController hero = heroObj.GetComponent<HeroBehaviorController>();
        hero.MoveTo(heroSpawnDestination.position);
        hero.MoveCompleted += DialogueManager.Instance.begin_dialogue;
        hero.MoveCompleted += (string s) =>
        {
            // - Face towards the hero 
            float direction = -1f;

            Vector3 playerScale = playerObj.transform.localScale;
            playerScale.x = Mathf.Abs(playerScale.x) * direction;
            playerObj.transform.localScale = playerScale;
        };

        // - Subscribe to begin combat when dialogue is complete.
        DialogueManager.YarnDialogueComplete += BeginCombat;

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
            StartCoroutine(DelayRespawnTrigger(4f));          
        }
               
    }

    public void BeginCombat()
    {
        // - Clear Cinematic and Dialogue states
        Game.isCinematicActive = false;
        Game.isDialogueActive = false;

        // Toggle Combat state
        Game.isCombatActive = true; // - Initiate Combat   

        // - Refresh User Interface
        InterfaceManager.Instance.RefreshUI();

    
        heroObj.GetComponent<Rigidbody2D>().excludeLayers = exclusionLayers; // - Stop hero from walking through walls
        OnCombatStart?.Invoke();
    }

    public void ClearHero()
    {
        if (heroObj != null)
        {
            Destroy(heroObj);
        }
    }
    
    private IEnumerator DelayRespawnTrigger(float duration)
    {
        yield return new WaitForSeconds(duration);
        heroRespawnTrigger.SetActive(true); // - Enable respawn trigger for next fight
    }

    public void GameOver()
    {        
        DialogueManager.YarnDialogueComplete += () =>
        {            
            TransitionHandler.Instance.StartSceneTransition("credits", 2f);                                    
        };

        DialogueManager.Instance.begin_dialogue("game_over");
    }

    public IEnumerator StartHeroWin()
    {
        yield return new WaitForSeconds(2f);
        DialogueManager.Instance.begin_hero_wins_dialogue();
    }
}
