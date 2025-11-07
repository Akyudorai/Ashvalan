using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HeroAnimation 
{
    IDLE    = 0,
    JUMP    = 1,
    RUN     = 2,
    ROLL    = 3,
    BLOCK   = 4, 
    ATTACK1 = 5,
    ATTACK2 = 6,
    ATTACK3 = 7,    
    HURT    = 8, 
    DEAD    = 9
}

public enum HeroState
{ 
    IDLE,
    MOVING,
    CHASING,
    RETREATING,
    EVADING,
    DEFENDING,
    ATTACKING,
    DEAD
}


public class HeroBehaviorController : MonoBehaviour
{
    // - Component References
    [Header("Component References")]
    public Animator anim;
    public Rigidbody2D rigid;
    public HealthScript health;
    public Transform groundCheckPoint;
    public AudioManager audio;

    // - State System Variables
    [Header("State Assets (Scriptable Objects)")]
    public HeroStateBehavior[] stateAssets;

    [Header("State Handling")]
    public HeroStateBehavior activeStateAsset;
    public HeroState currentState = HeroState.IDLE;
    public float stateTime = 0f;
    public float lastDecisionTime = 0f;
    
    // - Reset Time
    public float unstuckTime = 5f;
    public float unstuckDecreasePerLevel = 0.3f;

    // - Action Buffering
    public bool actionIsBuffered = false;

    [Header("Modular Systems")]
    public HeroDetectionSystem detection;
    public HeroStats stats;

    public delegate void OnMovementComplete(string command);
    public OnMovementComplete MoveCompleted;

    private void OnEnable()
    {
        // - Ensure references exist
        if (stateAssets == null) stats = GetComponent<HeroStats>();
        if (detection == null) detection = GetComponent<HeroDetectionSystem>();

        // - Subscribe to Animation Attack Events
        PlayerAnimationEvents.OnAttackAnimation += RespondToAttack;

        // - Subscibe to Death Events
        if (stats != null && stats.health != null)
            stats.health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        // - Unsubscribe to Animation Attack Events
        PlayerAnimationEvents.OnAttackAnimation -= RespondToAttack;

        // - Unsubscribe to Death Events
        if (stats != null && stats.health != null)
            stats.health.OnDeath -= OnDeath;        
    }

    private void Awake()
    {
        // - If components not assigned in inspector, fetch them
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (rigid == null) rigid = GetComponent<Rigidbody2D>();
        if (stats == null) stats = GetComponent<HeroStats>();
        if (detection == null) detection = GetComponent<HeroDetectionSystem>();

        // - Apply scaling through stats system
        stats?.ApplyScaling();

        // - Initialize Active Asset for current state
        SetActiveStateAsset(currentState);
        lastDecisionTime = 0f;

        // - Subscribe to on Combat Start
        GameManager.Instance.OnCombatStart += CombatBegin;
    }

    private void Update()
    {
        if (stats != null && stats.isDead) return;

        // - Update the rigidbody Y velocity float within the animator controller in real-time 
        if (anim != null && rigid != null)
            anim.SetFloat("AirSpeedY", rigid.linearVelocity.y);

        // - Check for Grounded State
        GroundCheck();

        // - Update Animator Parameters
        HandleAnimator();

        // - Unstuck Timer
        stateTime += Time.deltaTime;

        // - If Stunned or Dead, do not perform any action
        if (stats != null && (stats.isStunned || stats.isRolling)) return;

        // - If motion input is detected, move the player accordingly.
        if (stats.MotionX != 0 && stats.CanMove)
            transform.position += transform.right * stats.MotionX * stats.MoveSpeed * Time.deltaTime;

        // - Visualize Detections
        if (detection != null)
            detection.VisualizeScanning();

        // - Decision tick is gated by reactionTIme from stats
        float reaction = stats != null ? stats.reactionTime : 0.25f;
        if (Time.time - lastDecisionTime >= reaction)
        {
            detection?.ReadInputs();
            detection?.ScanForPlayer();
            detection?.ScanForWalls();

            // - Passive checks that run every tick
            detection?.AnyState();
            Unstuck();          

            // - Delegate to the active ScriptableObject state asset
            if (activeStateAsset != null)
            {                
                activeStateAsset.OnUpdate(this);
            }
                

            else
                Debug.LogError("ERROR: No active state asset found for current state");

            lastDecisionTime = Time.time;
        }

        // - Timers in stats update
        stats?.TickTimers(Time.deltaTime);
    }

    private void CombatBegin()
    {
        ChangeState(HeroState.CHASING);
    }

    private void GroundCheck()
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheckPoint.position, 0.3f, detection.groundLayer);
        stats.isGrounded = (col != null);
        anim.SetBool("Grounded", stats.isGrounded); // - Update the ground state boolean within the animator controller in real-time
    }

    public void ApplyStun(float length)
    {
        StartCoroutine(StunHero(length));
    }

    private IEnumerator StunHero(float length)
    {
        stats.isStunned = true;
        yield return new WaitForSeconds(length);
        stats.isStunned = false;
    }

    private void Unstuck()
    {
        if (stateTime >= (unstuckTime - (unstuckDecreasePerLevel * Game.currentLevel)))
        {
            Debug.LogWarning("HERO: I seem to be stuck.  Resetting to IDLE state.");
            ChangeState(HeroState.CHASING);
            stats.isBlocking = false;
            stats.isAttacking = false;
            stats.isRolling = false;
            stateTime = 0f;
        }
    }

    // --------------------------------------------------------------------------------------------------------------

    #region AI Responses

    private void RespondToAttack(string phase, float impactTime)
    {   
        // - If an action is already buffered, do not buffer another action
        if (actionIsBuffered) return;

        // - Delay Response based on Response Time
        StartCoroutine(DelayResponseToAttack(phase, impactTime, stats.reactionTime - (Time.time - lastDecisionTime)));
    }   

    private IEnumerator DelayResponseToAttack(string phase, float impactTime,float delay)
    {
        actionIsBuffered = true;

        yield return new WaitForSeconds(delay);

        if (phase == "Threat")
        {            
            if (impactTime > stats.reactionTime && detection.distanceToPlayer < 5f && stats.rollTimer <= 0f)
            {
                // - Dodge Roll
                DodgeRoll();
            }

            else if (impactTime <= stats.reactionTime && detection.distanceToPlayer < 2.5f)
            {
                // - Block
                Block();
            }

            else if (detection.distanceToPlayer > 5f || impactTime > stats.reactionTime * 2f || !detection.nearLeftEdge || !detection.nearRightEdge)
            {
                // - Switch to Evasion State to prepare for delayed reaction rather than instant reaction
                ChangeState(HeroState.EVADING);

                //// - Calculate Safe Spot
                //Vector3 direction = (player.transform.position + transform.position).normalized;               
                //Vector3 newPosition = new Vector3(((direction.x < 0) ? -1 : 1) *Mathf.Ceil(Mathf.Abs(direction.x)) * MoveSpeed, transform.position.y, transform.position.z);

                //// - Walk Away
                //targetMoveDestination = newPosition;
                //ChangeState(HeroState.MOVING);
            }
        }

        else if (phase == "Recovery")
        {
            ChangeState(HeroState.CHASING);
        }
    } 

    private void OnDeath()
    {
        ChangeState(HeroState.DEAD);
        gameObject.layer = LayerMask.NameToLayer("Dodge");
    
        // - End Combat
        Game.isCombatActive = false;
    }

    #endregion

    // --------------------------------------------------------------------------------------------------------------

    #region State System

    // - Change State
    public void ChangeState(HeroState newState)
    {
        // - Call Exit on the current active state asset
        if (activeStateAsset != null)
        {
            activeStateAsset.OnExit(this);
            activeStateAsset = null;
        }

        // - Find matching asset for new state
        SetActiveStateAsset(newState);

        // - Call enter on the new active state asset
        if (activeStateAsset != null)
            activeStateAsset.OnEnter(this);
    }

    private void SetActiveStateAsset(HeroState state)
    {   
        // - Reset active state asset
        activeStateAsset = null;
        if (stateAssets == null)
        {
            Debug.LogError("ERROR: No state assets assigned to HeroBehavior.");
            return;
        }
        // - Loop through state assets to find matching state
        for (int i = 0; i < stateAssets.Length; i++)
        {
            // - If matching state found, set as active state asset
            if (stateAssets[i] != null && stateAssets[i].state == state)
            {
                activeStateAsset = stateAssets[i];
                break;
            }
        }
        
        // - Log error if no matching asset found
        if (activeStateAsset == null)
        {
            Debug.LogError($"ERROR: No state asset found for state {state}.");
        }
    }

    #endregion

    
    #region Animations

    // - Fire Animations
    private void InvokeAnimation(HeroAnimation animation)
    {
        switch (animation)
        {
            case HeroAnimation.IDLE:
                anim.SetInteger("AnimState", 0);
                break;
            case HeroAnimation.RUN:
                anim.SetInteger("AnimState", 1);
                break;
            case HeroAnimation.JUMP:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Jump");
                rigid.AddForce(Vector3.up * stats.jumpForce, ForceMode2D.Impulse); //- Add some upward velocity to match animation
                break;
            case HeroAnimation.ROLL:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Roll");
                break;
            case HeroAnimation.BLOCK:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Block");
                break;
            case HeroAnimation.ATTACK1:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Attack1");
                break;
            case HeroAnimation.ATTACK2:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Attack2");
                break;
            case HeroAnimation.ATTACK3:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Attack3");
                break;
            case HeroAnimation.HURT:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Hurt");
                break;
            case HeroAnimation.DEAD:
                anim.SetInteger("AnimState", 0);
                anim.SetTrigger("Death");
                break;
        }
    }


    private IEnumerator AnimationDelay()
    {
        var animationClipInfo = anim.GetCurrentAnimatorClipInfo(0);
        float clipLength = animationClipInfo[0].clip.length;
        stats.CanMove = false;

        yield return new WaitForSeconds(clipLength);

        stats.CanMove = true;
    }

    private void HandleAnimator()
    {
        // - Flip the direction of player sprite based on movement direction
        if (stats.CanMove && stats.MotionX < -0.01f && transform.localScale.x == 1)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (stats.CanMove && stats.MotionX > 0.01 && transform.localScale.x == -1)
            transform.localScale = new Vector3(1, 1, 1);

        // - Toggle whether running or idle animation should be used
        anim.SetInteger("AnimState", (stats.MotionX != 0) ? 1 : 0);
        anim.SetBool("IdleBlock", stats.isBlocking);
    }

    #endregion 

    #region HeroAbilities 

    public void RespondToAttack(string phase, float impactTime, float lastDecisionTime)
    {
        // if action buffered, do nothing. Simple guard here
        if (stats.isAttacking) return;

        float delay = Mathf.Max(0f, stats.reactionTime - (Time.time - lastDecisionTime));
        StartCoroutine(DelayResponse(phase, impactTime, delay));
    }
    
    private IEnumerator DelayResponse(string phase, float impactTime, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (phase == "Threat")
        {
            var detection = GetComponent<HeroDetectionSystem>();
            float distance = (detection != null) ? detection.distanceToPlayer : 999f;

            if (impactTime > stats.reactionTime && distance < 5f && stats.rollTimer <= 0f)
            {
                DodgeRoll();
            }
            else if (impactTime <= stats.reactionTime && distance < 2.5f)
            {
                Block();
            }
            else if (distance > 5f || impactTime > stats.reactionTime * 2f)
            {
                ChangeState(HeroState.EVADING);
            }
        }
        else if (phase == "Recovery")
        {
            ChangeState(HeroState.CHASING);
        }
    }

    public void MoveTo(Vector3 destination) 
    {        
        stats.targetMoveDestination = destination;
        ChangeState(HeroState.MOVING);   
        Debug.Log("HERO: Received MoveTo command.");
    }

    public void Block()
    {
        // - Change State to Defend
        ChangeState(HeroState.DEFENDING);
    }

    public void DodgeRoll(int overrideDir = 0)
    {
        if (stats.isRolling || stats.rollTimer > 0f) return;

        int dir = overrideDir;

        // - If no override, pick based on edges or random
        var detection = GetComponent<HeroDetectionSystem>();
        if (dir == 0)
        {
            if (detection != null && detection.nearLeftEdge) dir = 1;
            else if (detection != null && detection.nearRightEdge) dir = -1;
            else dir = (Random.Range(0, 100) > 50) ? 1 : -1;
        }

        StartDodgeRoll(dir);
    }

    public void StartDodgeRoll(int dir)
    {
        // - Call the Animation
        anim?.SetTrigger("Roll");

        // - Change Collision Layer
        gameObject.layer = LayerMask.NameToLayer("Dodge");

        // - Get the dash direction based on the local scale
        Vector2 rollDirection = new Vector2(dir, 0f).normalized;

        // - Set the look direction to be in the direction of the dash
        transform.localScale = new Vector3(dir, 1f, 1f);

        // - Set a rigidbody velocity for the duration
        if (rigid != null) rigid.linearVelocity = rollDirection * stats.rollPower;

        // - Set IsRolling to True
        stats.isRolling = true;

        // - Start Roll Cooldown
        stats.rollTimer = stats.rollCooldown - (stats.rollDecreasePerLevel * Game.currentLevel);
    }

    public void StopDodgeRoll()
    {
        // - Switch collision layer back to Default to remove I-Frames
        gameObject.layer = LayerMask.NameToLayer("Default");

        // - Reset the dash
        if (rigid != null) rigid.linearVelocity = Vector2.zero;

        // - Set IsRolling to False
        stats.isRolling = false;
    }


    public void StartAttack1()
    {
        if (stats.isAttacking || stats.attackTimer > 0f) return;

        Debug.LogWarning("HERO: Beginning my first attack!");

        stats.isAttacking = true;
        stats.MotionX = 0f;
        stats.attackCombo = 1;
        stats.attackTimer = stats.attackCooldown - (stats.attackCooldownReductionPerLevel * Game.currentLevel);
        anim?.SetTrigger("Attack1");
        StartCoroutine(AnimationDelayAndEndAttack());
    }

    public void StartAttack2()
    {
        if (stats.isAttacking || stats.attackTimer > 0f) return;

        stats.isAttacking = true;
        stats.MotionX = 0f;
        stats.attackCombo = 2;
        stats.attackTimer = stats.attackCooldown - (stats.attackCooldownReductionPerLevel * Game.currentLevel);
        anim?.SetTrigger("Attack1");
        StartCoroutine(AnimationDelayAndEndAttack());
    }

    public void StartAttack3()
    {
        if (stats.isAttacking || stats.attackTimer > 0f) return;

        stats.isAttacking = true;
        stats.MotionX = 0f;
        stats.attackCombo = 3;
        stats.attackTimer = stats.attackCooldown - (stats.attackCooldownReductionPerLevel * Game.currentLevel);
        anim?.SetTrigger("Attack1");
        StartCoroutine(AnimationDelayAndEndAttack());
    }

    private IEnumerator AnimationDelayAndEndAttack()
    {
        if (anim == null) yield break;

        var info = anim.GetCurrentAnimatorClipInfo(0);
        float length = (info.Length > 0) ? info[0].clip.length : 0.5f;
        stats.CanMove = false;

        yield return new WaitForSeconds(length);

        stats.CanMove = true;
        stats.isAttacking = false;
    }

    #endregion
}
