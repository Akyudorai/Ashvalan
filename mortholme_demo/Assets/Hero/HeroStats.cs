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
    public float reactionDecreasePerLevel = 0.02f;  // - Reaction Time decrease based on Level
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
        float newHealth = baseHealth + (healthIncreasePerLevel * level);
        if (health != null)
        {
            health.maxHealth = newHealth;
            health.currentHealth = newHealth;
            InterfaceManager.Instance?.RefreshUI();
        }

        // - Apply Damage Scaling
        currentAttackDamage = baseDamage + (damageGrowthPerLevel * level);

        // - Apply Reaction Time Scaling, capped at 50ms
        reactionTime = baseReactionTime - (reactionDecreasePerLevel * level);
        if (reactionTime < 0.05f) reactionTime = 0.05f;

        // - Apply Block Reduction Scaling
        blockReduction = blockReductionPerLevel * level;
    }

    // - Called by controller each Update to tick timers
    public void TickTimers(float dt)
    {
        if (rollTimer > 0f) rollTimer -= dt;
        if (attackTimer > 0f) attackTimer -= dt;
    }
}
