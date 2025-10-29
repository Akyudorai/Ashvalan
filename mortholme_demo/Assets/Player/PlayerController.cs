using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;

public class PlayerController : MonoBehaviour
{   
    // - Component References
    public Animator anim;
    public Rigidbody2D rb;

    // - Object References
    public GameObject fireBlast_prefab;

    // - Internal Variables
    public float MotionX;
    public bool CanMove = true;
    public float MoveSpeed = 1.5f;
    
    public float dashPower = 5f; 



    public void Move(InputAction.CallbackContext context) 
    {
        var motion = context.ReadValue<Vector2>();
        MotionX = motion.x;
    }

    private void Update() 
    {      
        // - If motion input is detected, move the player accordingly.
        if (MotionX != 0 && CanMove) 
            transform.position += transform.right * MotionX * MoveSpeed * Time.deltaTime;

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
        if (context.performed) 
        {
            anim.SetTrigger("Attack");
            PlayerInputTracker.SubmitInput(2);
            StartCoroutine(AnimationDelay());
        }
            
    }
    
    // - ASSIGNED TO ABILITY 1
    public void SpearAttack(InputAction.CallbackContext context) 
    {   
        if (context.performed)
        {
            anim.SetTrigger("Spear");
            PlayerInputTracker.SubmitInput(3);
            StartCoroutine(AnimationDelay());
        }            
    }

    public void StartSpearDash()
    {
        // - Get the dash direction based on the local scale
        Vector2 dashDirection = new Vector2(transform.localScale.x, 0f).normalized;

        // - Set a rigidbody velocity for the duration
        rb.linearVelocity = dashDirection * dashPower;
    }

    public void StopSpearDash() 
    {
        // - Reset the dash
        rb.linearVelocity = Vector2.zero;    
    }

    // - ASSIGNED TO ABILITY 2
    public void ChainAttack(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            anim.SetTrigger("Chain");
            PlayerInputTracker.SubmitInput(4);
            StartCoroutine(AnimationDelay());
        }       
    }
    
    // - ASSIGNED TO ABILITY 3
    public void SpellAttack(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            anim.SetTrigger("Spell");
            PlayerInputTracker.SubmitInput(5);
            StartCoroutine(AnimationDelay());
        }
            
    }

    // -- ASSIGNED TO ABILITY 4
    public void BuffSpell(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            anim.SetTrigger("Buff");
            PlayerInputTracker.SubmitInput(6);
            StartCoroutine(AnimationDelay());
        }
            
    }

    

    

    
#endregion
}
