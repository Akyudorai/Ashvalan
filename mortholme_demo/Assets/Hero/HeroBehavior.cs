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

    // - Object References
    public Transform groundCheckPoint;

    // - Motion Variables
    public float jumpForce = 10.0f;
    public bool isGrounded = false;    
    public float MotionX;
    public bool CanMove = true;
    public float MoveSpeed = 1.5f;
    public float dashPower = 5f;

    // - State System Variables
    public HeroState currentState = HeroState.IDLE;

    // - Combat Variables
    public float meleeDistance = 3f;
    public float safeRangedDistance = 10f;
    public int attackCombo = 0;
    public bool isAttacking = false;



    private void Update() 
    {
        // - If motion input is detected, move the player accordingly.
        if (MotionX != 0 && CanMove)
            transform.position += transform.right * MotionX * MoveSpeed * Time.deltaTime;

        // - AI Detection Methods
        ReadInputs();
        ScanForPlayer();
        ScanForWalls();
        VisualizeScanning();

        // - AI Behavior
        StateBehavior();

        // - Update Animator Parameters
        HandleAnimator();

        // - Check for Grounded State
        GroundCheck();

        // - Update the rigidbody Y velocity float within the animator controller in real-time  
        anim.SetFloat("AirSpeedY", rigid.linearVelocity.y);   
    }

    private void GroundCheck() 
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheckPoint.position, 0.3f, groundLayer);
        isGrounded = (col != null);
        anim.SetBool("Grounded", isGrounded); // - Update the ground state boolean within the animator controller in real-time
    }

    private void StateBehavior()
    {
        IdleState();
        ChaseState();
        RetreatState();
        AttackState();
    }


    // --------------------------------------------------------------------------------------------------------------


    // - Idle State
    private void IdleState()
    {
        if (currentState == HeroState.IDLE)
        {
            MotionX = 0;


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
                Debug.LogWarning("HERO: I'm moving in to attack.");

                // - Move Toward Player
                Vector3 direction = (player.transform.position - transform.position).normalized;                
                MotionX = ((direction.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(direction.x)) * MoveSpeed; // - Direction * MoveInput (1 or 0) * MoveSpeed
           }

           // - If the within range of a melee attack, change to an attack state
           if (distanceToPlayer <= meleeDistance)
           {
                Debug.LogWarning("HERO: I'm ready to attack.");

                // - Set State To Attack
                ChangeState(HeroState.ATTACK);
           }

           /// - TODO: Scan for incoming attacks and switch to evasion state if detected


        }
    }

    // - Retreat State
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

        }
    }

    // - Defend State
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
            if (distanceToPlayer <= meleeDistance && !isAttacking)
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

    // - Counter State
    private void CounterState()
    {
        if (currentState == HeroState.COUNTER)
        {

        }
    }

    // - Death State
    private void DeathState()
    {
        if (currentState == HeroState.DEAD)
        {

        }
    }

    // - Change State
    public void ChangeState(HeroState newState)     
    {
        currentState = newState;
    }
    
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
    }


    #region Detection Algorithms

    [Header("AI Detection")]
    public GameObject player;
    public Animator player_anim;
    public List<int> player_inputs = new List<int>();
    public float distanceToPlayer;
    public bool nearLeftEdge = false;
    public bool nearRightEdge = false;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

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

        if (currentState == HeroState.IDLE)
        {
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

        


        /// - TODO: Implement some form of pattern recognition to punish players for repeated combos

    }

    private void ScanForPlayer()
    {
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

        // - Player Scan Line
        Debug.DrawLine(transform.position, player.transform.position, Color.yellow);
    }

    #endregion

    #region HeroAbilities 

    public void DodgeRoll()
    {
        InvokeAnimation(HeroAnimation.ROLL);
        StartCoroutine(AnimationDelay());
    }

    public void StartDodgeRoll()
    {
        // - Get the dash direction based on the local scale
        Vector2 dashDirection = new Vector2(transform.localScale.x, 0f).normalized;

        // - Set a rigidbody velocity for the duration
        rigid.linearVelocity = dashDirection * dashPower;
    }

    public void StopDodgeRoll()
    {
        // - Reset the dash
        rigid.linearVelocity = Vector2.zero;
    }

    public void StartAttack1()
    {
        // - Start Attack Animation
        isAttacking = true;
        attackCombo = 1;
        InvokeAnimation(HeroAnimation.ATTACK1);
        StartCoroutine(AnimationDelay());
    }    

    public void StartAttack2()
    {
        Debug.LogWarning("HERO: Chaining my second attack!");

        // - Start Attack 2 Animation
        attackCombo = 2;
        InvokeAnimation(HeroAnimation.ATTACK2);
        StartCoroutine(AnimationDelay());
    }

    public void StartAttack3()
    {
        Debug.LogWarning("HERO: Swinging with my third attack!");

        // - Start Attack 3 Animation
        attackCombo = 3;
        InvokeAnimation(HeroAnimation.ATTACK3);
        StartCoroutine(AnimationDelay());
    }

    #endregion
}
