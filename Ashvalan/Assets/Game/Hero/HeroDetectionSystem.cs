using UnityEngine;
using System.Collections.Generic;

public class HeroDetectionSystem : MonoBehaviour
{
    private HeroBehaviorController hero;

    [Header("Detection Components")]
    public GameObject player;
    public PlayerController pc;

    [Header("Detection Variables")]
    public List<int> player_inputs = new List<int>();
    public float distanceToPlayer;
    public bool nearLeftEdge = false;
    public bool nearRightEdge = false;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public bool disabled = false;

    private void Awake()
    {
        hero = GetComponent<HeroBehaviorController>();
    }

    private void Update()
    {
        if (pc != null && pc.health.isDead && !disabled)
        {
            disabled = true;

            StartCoroutine(GameManager.Instance.StartHeroWin());

            return;
        }
    }

    // - Read recent player inputs (exposed so states can query)
    public void ReadInputs()
    {
        int length = 5;
        player_inputs = PlayerInputTracker.ReadInputs(length);

        // - Count Input Types (Melee VS Ranged)
        int meleeCount = 0;
        int rangedCount = 0;
        foreach (int input in player_inputs)
        {
            if (input == 2 || input == 3) meleeCount++;
            if (input == 5 || input == 6) rangedCount++;
        }

        /// - TODO: Implement some form of pattern recognition to punish players for repeated combos

        /*
        // - Input reading decision making
        if (currentState == HeroState.IDLE)
        {
            // - Toggled off for now due to erratic behavior            
            if ((float)meleeCount / player_inputs.Count > 0.7f)
            {
                // - Player is likely spamming melee attacks
                ChangeState(HeroState.RETREAT);
                Debug.LogWarning("HERO: Boss is using a lot of melee attacks. I'm going to put some distance between us.");
            }

            else if ((float)rangedCount / player_inputs.Count > 0.7f)
            {
                // - Player is likely spamming ranged attacks            
                ChangeState(HeroState.CHASE);
                Debug.LogWarning("HERO: Boss is leaving himself open to attack. I'm going to try to get close to attack.");
            }
        }
        */
    }

    // - Passive hazard checks that should run regardless of state
    public void AnyState()
    {
        // - Scan for Nearby Fireblasts
        GameObject fireblast = GameObject.FindGameObjectWithTag("Fireblast");

        if (fireblast != null)
        {
            float distanceToFireblast = Vector3.Distance(transform.position, fireblast.transform.position);
            if (distanceToFireblast < 4f + transform.localScale.x)
            {
                hero.DodgeRoll();              
            }
        }  
    }

    // - Find and store player + player controller references
    public void ScanForPlayer()
    {
        // - If player components have not been registered yet
        if (player == null)
        {
            // - Seek for player component dependencies
            try
            {
                player = GameObject.FindGameObjectWithTag("Player");
                pc = player?.GetComponent<PlayerController>();
            }

            catch
            {
                Debug.LogError("ERROR: Unable to locate Player object in scene");
                return;
            }
        }

        // - Calculate distance between player and hero
        if (player != null)
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    // - Look for walls / edges of arena
    public void ScanForWalls()
    {
        RaycastHit2D hit;

        // - Scan for left edge
        hit = Physics2D.Raycast(transform.position, -transform.right, 5f, wallLayer);
        nearLeftEdge = hit;

        // - Scan for right edge
        hit = Physics2D.Raycast(transform.position, transform.right, 5f, wallLayer);
        nearRightEdge = hit;
    }

    public void VisualizeScanning()
    {
        // - Left Wall Scan Line
        Debug.DrawLine(transform.position, transform.position + (transform.right * -5f), (nearLeftEdge ? Color.red : Color.green));

        // - Right Wall Scan Line
        Debug.DrawLine(transform.position, transform.position + (transform.right * 5f), (nearRightEdge ? Color.red : Color.green));

        if (player != null)
        {
            // - Player Scan Line
            Debug.DrawLine(transform.position, player.transform.position, Color.yellow);
        }
    }
}
