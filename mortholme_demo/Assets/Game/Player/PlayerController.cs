using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PlayerController : MonoBehaviour
{   
    // - Component References
    public Animator anim;
    public Rigidbody2D rb;
    public HealthScript health;
    public AudioManager audio;
    public GameObject canvas;

    // - Object References
    public GameObject fireBlast_prefab;

    // - Internal Variables
    public float MotionX;
    public bool CanMove = true;
    public float MoveSpeed = 1.5f;
    public float dashPower = 5f;
    public bool readyToBurnHero = false;

    // - Animation Variables
    AnimatorClipInfo[] currentAnimationClip;
    public int currentFrameIndex = 0;
    public int currentFrameMax = 0;
    
    private void OnEnable()
    {
        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        anim.SetTrigger("Death");
    }

    public void Move(InputAction.CallbackContext context) 
    {
        var motion = context.ReadValue<Vector2>();
        MotionX = motion.x;
    }

    private void Update()
    {
        // - Toggle Passive Burn Ability based on Combat State
        ToggleBurnAway();

        // - Freeze Player
        if (Game.isDialogueActive || Game.isCinematicActive || health.isDead) return;

        // - Prevent inverted canvas based on look direction of player
        float playerDir = Mathf.Sign(transform.localScale.x);
        if (playerDir == 0f) playerDir = 1f;
        float canvasScale = 0.02617211f;
        canvas.transform.localScale = new Vector3( canvasScale * playerDir, canvasScale, canvasScale);

        // - If motion input is detected, move the player accordingly.
        if (MotionX != 0 && CanMove)
            transform.position += transform.right * MotionX * MoveSpeed * Time.deltaTime;    

        // - Clamp the X position of the player so they cannot walk off screen
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -21.5f, 22.5f), transform.position.y, transform.position.z);

        // - Update Animator Parameters        
        HandleAnimator();    
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
            transform.localScale = new Vector3 (1, 1, 1);

        anim.SetFloat("Movement", MotionX);
    }

#region Character Attacks
    
    // - ASSIGNED TO ATTACK
    public void Attack(InputAction.CallbackContext context) 
    {   
        // - Player cannot attack when not in combat
        if (!Game.isCombatActive) return;

        if (context.performed && InterfaceManager.Instance.GetSkillCooldown(1) <= 0f) 
        {
            anim.SetTrigger("Attack");
            PlayerInputTracker.SubmitInput(2);
            InterfaceManager.Instance.StartSkillTimer(1, 3f);
            StartCoroutine(AnimationDelay());
        }
            
    }
    
    // - ASSIGNED TO ABILITY 1
    public void SpearAttack(InputAction.CallbackContext context) 
    {   
        // - Player cannot attack when not in combat
        if (!Game.isCombatActive) return;

        if (context.performed && InterfaceManager.Instance.GetSkillCooldown(2) <= 0f)
        {
            anim.SetTrigger("Spear");
            PlayerInputTracker.SubmitInput(3);
            InterfaceManager.Instance.StartSkillTimer(2, 3f);
            StartCoroutine(AnimationDelay());
        }            
    }

    public void StartSpearDash()
    {
        // - Get the dash direction based on the local scale
        Vector2 dashDirection = new Vector2(transform.localScale.x, 0f).normalized;

        // - Allow Rigidbody Movement along X Axis
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // - Set a rigidbody velocity for the duration
        rb.linearVelocity = dashDirection * dashPower;
    }

    public void StopSpearDash() 
    {
        // - Lock Constraints for Forced Movement
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // - Reset the dash
        rb.linearVelocity = Vector2.zero;    
    }

    // - ASSIGNED TO ABILITY 2
    public void ChainAttack(InputAction.CallbackContext context) 
    {
        if (!Game.isCombatActive) return;

        if (context.performed && InterfaceManager.Instance.GetSkillCooldown(3) <= 0f)
        {
            anim.SetTrigger("Chain");
            PlayerInputTracker.SubmitInput(4);
            InterfaceManager.Instance.StartSkillTimer(3, 3f);
            StartCoroutine(AnimationDelay());
        }       
    }
    
    // - ASSIGNED TO ABILITY 3
    public void SpellAttack(InputAction.CallbackContext context) 
    {
        // - Player cannot attack when not in combat
        if (!Game.isCombatActive) return;

        if (context.performed && InterfaceManager.Instance.GetSkillCooldown(4) <= 0f)
        {
            anim.SetTrigger("Spell");
            PlayerInputTracker.SubmitInput(5);
            InterfaceManager.Instance.StartSkillTimer(4, 3f);
            StartCoroutine(AnimationDelay());
        }
            
    }

    // -- ASSIGNED TO ABILITY 4
    public void BurnSpell(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {   
            if (!readyToBurnHero) return;

            anim.SetTrigger("Buff");
            PlayerInputTracker.SubmitInput(6);
            GameManager.Instance.ClearHero();
            GameManager.Instance.LevelUp();
            StartCoroutine(AnimationDelay());
        }
            
    }

    private void ToggleBurnAway() 
    {
        

        if (!Game.isCombatActive && GameManager.Instance.heroObj != null) 
        {
            if (Vector3.Distance(transform.position, GameManager.Instance.heroObj.transform.position) < 2.5f) 
            {
                InterfaceManager.Instance.TogglePassive(true);
                readyToBurnHero = true;
                
            }

            else 
            {
                InterfaceManager.Instance.TogglePassive(false);
                readyToBurnHero = false; 
            }
        }

        else 
        {
            InterfaceManager.Instance.TogglePassive(false);
            readyToBurnHero = false;
        }

    }

#endregion

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerReset")) 
        {
            if (!Game.isCombatActive)
            {
                Game.isPlayerResetReady = true;
                Game.isCinematicActive = true;
            }       
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerReset")) 
        {
            Game.isPlayerResetReady = false;
        }
    }

}
