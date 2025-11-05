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
    CHASE,
    RETREAT,
    EVADE,
    DEFEND,
    ATTACK,
    COUNTER,
    DEAD
}


public class HeroBehavior : MonoBehaviour
{
    // - Component References
    public Animator anim;
    public Rigidbody2D rigid;
    public HealthScript health;

    // - Object References
    public Transform groundCheckPoint;

    // - Motion Variables
    public float jumpForce = 10.0f;
    public bool isGrounded = false;    
    public float MotionX;
    public bool CanMove = true;
    public float MoveSpeed = 1.5f;
    public float rollPower = 5f;
    public Vector3 targetMoveDestination;

    // - State System Variables
    public HeroState currentState = HeroState.IDLE;
    public float stateTime = 0f;
    
    // - Combat Variables
    public float meleeDistance = 3f;
    public float safeRangedDistance = 10f;
    public int attackCombo = 0;
    public bool isAttacking = false;
    public bool isBlocking = false;
    public bool isDead = false;
    public bool isStunned = false;
    public bool isRolling = false;

    [Header("AI Scaling Parameters")]
    public int difficultyLevel = 0;

    // - Health Scaling
    public float baseHealth = 100f;                 // - Baseline Health
    public float healthIncreasePerLevel = 10f;      // - Health increase based on Level

    // - Damage Scaling
    public float baseDamage = 1f;                   // - Baseline Damage  
    public float damageGrowthPerLevel = 0.25f;      // - Damage increase based on Level
    public float currentAttackDamage = 1f;          // - Scaled Attack Damage

    // - Reaction Time
    public float baseReactionTime = 0.3f;           // - Base Reaction Time in seconds    
    public float reactionDecreasePerLevel = 0.02f;  // - Reaction Time decrease based on Level
    public float reactionTime;                      // - Current Reaction Time

    // - Cooldowns
    public float rollTimer = 0f;
    public float rollCooldown = 3f;
    public float rollDecreasePerLevel = 0.3f;
    public float blockReduction = 0f;
    public float blockReductionPerLevel = 0.2f;
    public float attackTimer = 0f;
    public float attackCooldown = 0.5f;
    public float attackCooldownReductionPerLevel = 0.1f;

    // - Reset Time
    public float unstuckTime = 5f;
    public float unstuckDecreasePerLevel = 0.3f;

    public float lastDecisionTime = 0f;

    // - Action Buffering
    public bool actionIsBuffered = false;
    
    [Header("AI Detection")]
    public GameObject player;
    public PlayerController pc;
    public List<int> player_inputs = new List<int>();
    public float distanceToPlayer;
    public bool nearLeftEdge = false;
    public bool nearRightEdge = false;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private void OnEnable()
    {
        health.OnDeath += OnDeath;
        PlayerAnimationEvents.OnAttackAnimation += RespondToAttack;
    }

    private void OnDisable()
    {
        health.OnDeath -= OnDeath;
        PlayerAnimationEvents.OnAttackAnimation -= RespondToAttack;
    }

    private void Awake() 
    {
        //Initialize();
        ApplyScaling();
        lastDecisionTime = 0f;
    }

    private void Update() 
    {
        if (isDead) return;

        // - Check for Grounded State
        GroundCheck();

        // - Update the rigidbody Y velocity float within the animator controller in real-time  
        anim.SetFloat("AirSpeedY", rigid.linearVelocity.y);

        // - Update Animator Parameters
        HandleAnimator();

        // - Unstuck Timer
        stateTime += Time.deltaTime;

        // - If Stunned or Dead, do not perform any action
        if (isStunned || isRolling) return;

        // - If motion input is detected, move the player accordingly.
        if (MotionX != 0 && CanMove)
            transform.position += transform.right * MotionX * MoveSpeed * Time.deltaTime;

        // - Countdown Timers
        rollTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        // - AI Detection Methods
        VisualizeScanning();        
        if (Time.time - lastDecisionTime >= reactionTime)
        {            
            ScanForPlayer();
            ScanForWalls();
            
            // - AI Behavior
            StateBehavior();

            lastDecisionTime = Time.time;
        }
        
    }

    private void GroundCheck() 
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheckPoint.position, 0.3f, groundLayer);
        isGrounded = (col != null);
        anim.SetBool("Grounded", isGrounded); // - Update the ground state boolean within the animator controller in real-time
    }

    private void StateBehavior()
    {
        MovingState();

        if (!Game.isCombatActive) return;
        
        ReadInputs();

        AnyState();
        IdleState();
        ChaseState();
        //RetreatState();
        AttackState();        
        EvadeState();
        DefendState();
        Unstuck();
    }

    public void ApplyStun(float length)
    {
        StartCoroutine(StunHero(length));
    }

    private IEnumerator StunHero(float length)
    {
        isStunned = true;
        yield return new WaitForSeconds(length);
        isStunned = false;
    }

    private void Unstuck()
    {
        if (stateTime >= (unstuckTime - (unstuckDecreasePerLevel * Game.currentLevel)))
        {
            Debug.LogWarning("HERO: I seem to be stuck.  Resetting to IDLE state.");
            ChangeState(HeroState.CHASE);
            isBlocking = false;
            isAttacking = false;
            isRolling = false;
            stateTime = 0f;
        }
    }

    private void ApplyScaling() 
    {
        // - Apply Health Scaling
        float newHealth = baseHealth + (healthIncreasePerLevel * Game.currentLevel);
        health.maxHealth = newHealth;
        health.currentHealth = newHealth;
        InterfaceManager.Instance.RefreshUI();

        // - Apply Damage Scaling
        currentAttackDamage = baseDamage + (damageGrowthPerLevel * Game.currentLevel);

        // - Apply Reaction Time Scaling
        reactionTime = baseReactionTime - (reactionDecreasePerLevel * Game.currentLevel);
        if (reactionTime < 0.05f) reactionTime = 0.05f; // - Cap minimum reaction time

        // - Apply Block Damage Reduction Scaling
        blockReduction = blockReductionPerLevel * Game.currentLevel;
    }

    // --------------------------------------------------------------------------------------------------------------

    #region AI Responses

    private void RespondToAttack(string phase, float impactTime)
    {   
        // - If an action is already buffered, do not buffer another action
        if (actionIsBuffered) return;

        // - Delay Response based on Response Time
        StartCoroutine(DelayResponseToAttack(phase, impactTime, reactionTime - (Time.time - lastDecisionTime)));
    }   

    private IEnumerator DelayResponseToAttack(string phase, float impactTime,float delay)
    {
        actionIsBuffered = true;

        yield return new WaitForSeconds(delay);

        if (phase == "Threat")
        {            
            if (impactTime > reactionTime && distanceToPlayer < 5f && rollTimer <= 0f)
            {
                // - Dodge Roll
                DodgeRoll();
            }

            else if (impactTime <= reactionTime && distanceToPlayer < 2.5f)
            {
                // - Block
                Block();
            }

            else if (distanceToPlayer > 5f || impactTime > reactionTime * 2f || !nearLeftEdge || !nearRightEdge)
            {
                // - Switch to Evasion State to prepare for delayed reaction rather than instant reaction
                ChangeState(HeroState.EVADE);

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
            ChangeState(HeroState.CHASE);
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
        ExitState(currentState);
        EnterState(newState);
    }

    private void EnterState(HeroState state)
    {
        currentState = state;
        stateTime = 0f;

        switch (state)
        {
            case HeroState.IDLE:
                MotionX = 0;
                break;
            case HeroState.MOVING:
                break;
            case HeroState.CHASE:
                break;
            case HeroState.RETREAT:
                break;
            case HeroState.ATTACK:
                MotionX = 0;
                isAttacking = false;
                attackCombo = 0;
                break;
            case HeroState.EVADE:
                MotionX = 0;
                break;
            case HeroState.DEFEND:
                MotionX = 0;
                isBlocking = true;
                anim.SetTrigger("Block");
                break;
            case HeroState.DEAD:
                MotionX = 0;
                isDead = true;
                anim.SetTrigger("Death");
                break;
        }
    }

    private void ExitState(HeroState state)
    {
        switch (state)
        {
            case HeroState.IDLE:
                break;
            case HeroState.CHASE:
                break;
            case HeroState.RETREAT:
                break;
            case HeroState.ATTACK:
                break;
            case HeroState.EVADE:
                break;
            case HeroState.DEFEND:
                isBlocking = false;
                break;
        }
    }

    #region Passive State Logic

    // - Effects that can be triggered from Any AI state
    private void AnyState()
    {
        // - Scan for Nearby Fireblasts
        GameObject fireblast = GameObject.FindGameObjectWithTag("Fireblast");

        if (fireblast != null)
        {
            float distanceToFireblast = Vector3.Distance(transform.position, fireblast.transform.position);
            if (distanceToFireblast < 4f + transform.localScale.x && rollTimer <= 0f)
            {
                // - Roll Away.  It's unblockable!
                DodgeRoll();
            }
        }         
    }

    // - Idle State
    private void IdleState()
    {
        if (currentState == HeroState.IDLE)
        {
            ChangeState(HeroState.CHASE);
        }
    }

    // - Moving State
    private void MovingState()
    {
        if (currentState == HeroState.MOVING)
        {
            // - If not at target destination, move towards the destination
            float distanceToDestination = Vector3.Distance(transform.position, targetMoveDestination);
            if (distanceToDestination > 1f)
            {
                Debug.LogWarning($"HERO: Moving to destination {targetMoveDestination}.");
                Vector3 direction = (player.transform.position - transform.position).normalized;
                MotionX = ((direction.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(direction.x)) * MoveSpeed;                
            }

            else if (distanceToDestination < 1f)
            {
                Debug.LogWarning("HERO: Done Moving, switching to IDLE state.");
                ChangeState(HeroState.IDLE);
            }
        }
    }

    // - Chase State
    private void ChaseState()
    {
        if (currentState == HeroState.CHASE)
        {
            // - If out of range of a melee attack, move towards the player
            if (distanceToPlayer > meleeDistance)
            {
                //Debug.LogWarning("HERO: I'm moving in to attack.");

                // - Move Toward Player
                Vector3 direction = (player.transform.position - transform.position).normalized;
                MotionX = ((direction.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(direction.x)) * MoveSpeed; // - Direction * MoveInput (1 or 0) * MoveSpeed
            }

            // - If the within range of a melee attack, change to an attack state
            if (distanceToPlayer <= meleeDistance)
            {
                //Debug.LogWarning("HERO: I'm ready to attack.");

                // - Set State To Attack
                ChangeState(HeroState.ATTACK);
            }           
        }
    }

    // - DISABLED: Retreat State
    private void RetreatState()
    {
        if (currentState == HeroState.RETREAT)
        {
            // - If the player is too close, run away from the player
            if (distanceToPlayer < safeRangedDistance)
            {
                Debug.LogWarning("HERO: Boss is too close to me.");

                // - Move Toward Player
                Vector3 direction = (player.transform.position + transform.position).normalized;
                MotionX = ((direction.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(direction.x)) * MoveSpeed; // - Direction * MoveInput (1 or 0) * MoveSpeed
            }

            // - If the player is far enough, switch to Idle state
            if (distanceToPlayer >= safeRangedDistance)
            {
                Debug.LogWarning("HERO: I've successfully put enough distance between us.");

                // - Set State To Idle
                ChangeState(HeroState.IDLE);
            }
        }
    }

    // - Evade State
    private void EvadeState()
    {
        if (currentState == HeroState.EVADE)
        {
            // - Analyze Which Attack is Coming
            int incomingAttack = player.GetComponentInChildren<PlayerAnimationEvents>().currentAttack;
            /// Reminder: 1 = sword attack, 2 = spear dash, 3 = chain pull            

            // - Reaction to Sword Attack
            if (incomingAttack == 1)
            {
                // - Select either to block or dodge
                /// - TODO: Replace with Decision Algorithm
                int rand = Random.Range(0, 100);
                if (rand > 50) Block();
                else 
                {
                    if (rollTimer <= 0f) DodgeRoll();
                    else Block();
                }
            }

            // - Reaction to Spear Dash
            if (incomingAttack == 2)
            {
                Vector3 playerPos = player.transform.position;
                Vector3 playerVel = pc.rb.linearVelocity;
                float dashSpeed = pc.dashPower;

                // - Boss has begun their dash attack
                if (playerVel.magnitude > 1f)
                {
                    Debug.Log("Boss is dashing");
                    // - Roll towards player to dodge
                    if (distanceToPlayer < 17f && rollTimer <= 0f) // - May need to adjust reaction distance
                    {
                        Debug.Log("I'm dodging");
                        Vector3 rollDirection = (player.transform.position - transform.position).normalized;
                        float newX = ((rollDirection.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(rollDirection.x));
                        DodgeRoll((int)newX);
                    } 
                    else 
                    {
                        // - Jump to avoid attack
                        rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                        ChangeState(HeroState.CHASE);
                    }
                }
            }

            // - Reaction to Chain Pull
            if (incomingAttack == 3)
            {
                // - Boss has thrown their chain
                if (distanceToPlayer < 11.5f + transform.localScale.x)
                {
                    // - Jump or Roll Backwards Away From Attack
                    int rand = Random.Range(0, 100);
                    if (rand > 50)
                    {
                        if (rollTimer <= 0f) 
                        {
                            Vector3 rollDirection = (player.transform.position - transform.position).normalized;
                            float newX = ((rollDirection.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(rollDirection.x));
                            DodgeRoll((int)newX);
                        }

                        else 
                        {
                            rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                            ChangeState(HeroState.CHASE);
                        }
                        
                    }

                    else
                    {
                        rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                        ChangeState(HeroState.CHASE);
                    }
                }
            }                                               
        }
    }

    private void DefendState() 
    {
        if (currentState == HeroState.DEFEND) 
        {

        }
    }

    // - Attack State
    private void AttackState()
    {
        if (currentState == HeroState.ATTACK)
        {
            MotionX = 0;

            // - If still within melee range, perform a melee attack
            if (distanceToPlayer <= meleeDistance && !isAttacking && attackTimer <= 0f)
            {
                Debug.LogWarning("HERO: I'm starting my attack!");
                // - Attack the player
                StartAttack1();                
            }

            // - If the player gets out of range, switch to chase mode
            else if (distanceToPlayer > meleeDistance && !isAttacking)
            {
                Debug.LogWarning("HERO: The boss got away from me.  I'm moving after him!");
                // - Chase the player again
                ChangeState(HeroState.CHASE);
            }
        }
    }

    // - Death State
    private void DeathState()
    {
        if (currentState == HeroState.DEAD)
        {

        }
    }

    #endregion

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
                rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse); //- Add some upward velocity to match animation
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
        CanMove = false;

        yield return new WaitForSeconds(clipLength);

        CanMove = true;
    }

    private void HandleAnimator()
    {
        // - Flip the direction of player sprite based on movement direction
        if (CanMove && MotionX < -0.01f && transform.localScale.x == 1)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (CanMove && MotionX > 0.01 && transform.localScale.x == -1)
            transform.localScale = new Vector3(1, 1, 1);

        // - Toggle whether running or idle animation should be used
        anim.SetInteger("AnimState", (MotionX != 0) ? 1 : 0);
        anim.SetBool("IdleBlock", isBlocking);
    }

    #endregion 

    #region Detection Algorithms    

    private void ReadInputs()
    {
        // - Read Inputs up to X in length based on AI skill level
        /// - TODO: Making length a dynamic variable based on skill level
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

        // - Input reading decision making
        if (currentState == HeroState.IDLE)
        {   
            // - Toggled off for now due to erratic behavior
            /*
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
            */            
        }

        


        /// - TODO: Implement some form of pattern recognition to punish players for repeated combos

    }

    private void ScanForPlayer()
    {
        if (player == null) 
        {
            try {
                player = GameObject.FindGameObjectWithTag("Player");
                pc = player.GetComponent<PlayerController>();
            }
            catch {
                Debug.LogError("HERO: Unable to locate Player object in scene.");
                return;
            }
        }

        // - Calculate Distance to Player
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    private void ScanForWalls()
    {
        RaycastHit2D hit;

        // - Look for Left Wall
        hit = Physics2D.Raycast(transform.position, -transform.right, 5f, wallLayer);
        nearLeftEdge = hit;

        // - Look for Right Wall
        hit = Physics2D.Raycast(transform.position, transform.right, 5f, wallLayer);
        nearRightEdge = hit;
    }

    private void VisualizeScanning()
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

    #endregion

    #region HeroAbilities 

    public void MoveTo(Vector3 destination) 
    {        
        targetMoveDestination = destination;
        ChangeState(HeroState.MOVING);   
        Debug.Log("HERO: Received MoveTo command.");
    }

    private void Block()
    {
        // - Change State to Defend
        ChangeState(HeroState.DEFEND);
    }

    private void DodgeRoll(int overrideDir = 0)
    {
        int dir = overrideDir;

        if (dir == 0)
        {
            if (nearLeftEdge) dir = 1;          // - Roll to Right
            else if (nearRightEdge) dir = -1;   // - Roll to Left
            else
            {   
                // - Random Roll Direction
                int rand = Random.Range(0, 100);
                dir = (rand > 50) ? 1 : -1;
            }
        }
        
        StartDodgeRoll(dir);      
    }

    public void StartDodgeRoll(int dir)
    {
        // - Call the Animation
        anim.SetTrigger("Roll");

        // - Change Collision Layer
        gameObject.layer = LayerMask.NameToLayer("Dodge");

        // - Get the dash direction based on the local scale
        Vector2 rollDirection = new Vector2(dir, 0f).normalized;

        // - Set the look direction to be in the direction of the dash
        transform.localScale = new Vector3(dir, 1f, 1f);

        // - Set a rigidbody velocity for the duration
        rigid.linearVelocity = rollDirection * rollPower;

        // - Set IsRolling to True
        isRolling = true;

        // - Start Roll Cooldown
        rollTimer = rollCooldown - (rollDecreasePerLevel * Game.currentLevel);
    }

    public void StopDodgeRoll()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        // - Reset the dash
        rigid.linearVelocity = Vector2.zero;

        // - Set IsRolling to False
        isRolling = false;
    }    

    public void StartAttack1()
    {
        // - Start Attack Animation
        isAttacking = true;
        attackCombo = 1;
        InvokeAnimation(HeroAnimation.ATTACK1);
        StartCoroutine(AnimationDelay());
        attackTimer = attackCooldown - (attackCooldownReductionPerLevel * Game.currentLevel);
    }    

    public void StartAttack2()
    {
        Debug.LogWarning("HERO: Chaining my second attack!");

        // - Start Attack 2 Animation
        attackCombo = 2;
        InvokeAnimation(HeroAnimation.ATTACK2);
        StartCoroutine(AnimationDelay());
        attackTimer = attackCooldown - (attackCooldownReductionPerLevel * Game.currentLevel);
    }

    public void StartAttack3()
    {
        Debug.LogWarning("HERO: Swinging with my third attack!");

        // - Start Attack 3 Animation
        attackCombo = 3;
        InvokeAnimation(HeroAnimation.ATTACK3);
        StartCoroutine(AnimationDelay());
        attackTimer = attackCooldown - (attackCooldownReductionPerLevel * Game.currentLevel);
    }

    #endregion
}
