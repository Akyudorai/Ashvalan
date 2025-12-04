using System.Collections;
using UnityEngine;

public class HeroStats : MonoBehaviour
{
    // - Component References
    private Animator anim;
    private Rigidbody2D rigid;
    private HeroBehaviorController hero;

    // - Motion Variables
    [Header("Motion Variables")]
    public float jumpForce = 10.0f;
    public bool isGrounded = false;
    public float MotionX;
    public bool CanMove = true;
    public float MoveSpeed = 1.5f;
    public float rollPower = 5f;
    public Vector3 targetMoveDestination;

    // - Combat Variables
    [Header("Combat Variables")]
    public float meleeDistance = 3f;
    public float safeRangedDistance = 10f;
    public int attackCombo = 0;
    public bool isAttacking = false;
    public bool isBlocking = false;
    public bool isDead = false;
    public bool isStunned = false;
    public bool isRolling = false;

    // - Cooldowns
    [Header("Cooldowns")]
    public float rollTimer = 0f;
    public float rollCooldown = 3f;
    public float rollDecreasePerLevel = 0.3f;
    public float blockReduction = 0f;
    public float blockReductionPerLevel = 0.2f;
    public float attackTimer = 0f;
    public float attackCooldown = 0.5f;
    public float attackCooldownReductionPerLevel = 0.1f;

    // - Reaction Time
    [Header("Reaction Time")]
    public float baseReactionTime = 0.3f;           // - Base Reaction Time in seconds    
    public float reactionDecreasePerLevel = 0.03f;  // - Reaction Time decrease based on Level
    public float reactionTime;                      // - Current Reaction Time

    // - Damage Scaling
    [Header("Damage")]
    public float baseDamage = 1f;                   // - Baseline Damage  
    public float damageGrowthPerLevel = 0.25f;      // - Damage increase based on Level
    public float currentAttackDamage = 1f;          // - Scaled Attack Damage

    // - Health Scaling
    [Header("Health")]
    public HealthScript health;
    public float baseHealth = 100f;                 // - Baseline Health
    public float healthIncreasePerLevel = 10f;      // - Health increase based on Level


    private void Awake()
    {
        hero = GetComponent<HeroBehaviorController>();
        anim = hero != null ? hero.anim : GetComponent<Animator>();
        rigid = hero != null ? hero.rigid : GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<HealthScript>();
    }

    public void ApplyScaling()
    {
        int level = Game.currentLevel;

        // - Apply Health Scaling
        // - Linear Scaling
        //float newHealth = baseHealth + (healthIncreasePerLevel * level);
        // - Exponential Scaling
        // - 1 = 25 | 2 = 31 | 3 = 39 | 4 = 49 | 5 = 61 | 6 = 76 | 7 = 95 | 8 = 119 | 9 = 149 | 10 = 186
        float newHealth = baseHealth * Mathf.Pow(1.25f, level);
        if (health != null)
        {
            health.maxHealth = newHealth;
            health.currentHealth = newHealth;
            InterfaceManager.Instance?.RefreshUI();
        }

        // - Apply Damage Scaling
        // - Linear Scaling
        //currentAttackDamage = baseDamage + (damageGrowthPerLevel * level);
        // - Exponential Scaling
        // - 1 = 0.25 | 2 = 0.35 | 3 = 0.49 | 4 = 0.69 | 5 = 0.97 | 6 = 1.36 | 7 = 1.91 | 8 = 2.68 | 9 = 3.75 | 10 = 5.25
        currentAttackDamage = baseDamage + Mathf.Pow(1.4f, level);

        // - Apply Reaction Time Scaling, capped at 50ms
        // - Linear Scaling
        //reactionTime = baseReactionTime - (reactionDecreasePerLevel * level);
        // - Exponential Scaling
        // - 1 = 0.3 | 2 = 0.26 | 3 = 0.22 | 4 = 0.19 | 5 = 0.16 | 6 = 0.14 | 7 = 0.12 | 8 = 0.1 | 9 = 0.09 | 10 = 0.05
        reactionTime = Mathf.Max(0.05f, baseReactionTime * Mathf.Pow(0.85f, level));
        if (reactionTime < 0.05f) reactionTime = 0.05f;

        // - Apply Block Reduction Scaling
        // - Linear Scaling
        //blockReduction = blockReductionPerLevel * level;
        // - Exponential Scaling
        // - 1 = 26% | 2 = 34% | 3 = 44% | 4 = 57% | 5 = 74% | 6 = 96% | 7 = 120% | 8 = 120% | 9 = 120% | 10 = 120%
        blockReduction = 0.01f * Mathf.Min(120, 20 * Mathf.Pow(1.3f, level));
    }

    // - Called by controller each Update to tick timers
    public void TickTimers(float dt)
    {
        if (rollTimer > 0f) rollTimer -= dt;
        if (attackTimer > 0f) attackTimer -= dt;
    }
}
