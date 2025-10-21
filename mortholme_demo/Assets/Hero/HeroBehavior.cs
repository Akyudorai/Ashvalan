using UnityEngine;
using System.Collections;

public enum HeroState 
{
    Idle    = 0,
    Jump    = 1,
    Run     = 2,
    Roll    = 3,
    Block   = 4, 
    Attack1 = 5,
    Attack2 = 6,
    Attack3 = 7,    
    Hurt    = 8, 
    Dead    = 9      
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
    public LayerMask groundLayer;

    // - State System Variables
    public HeroState currentState = HeroState.Idle;

    private void Update() 
    {
        GroundCheck();

        anim.SetFloat("AirSpeedY", rigid.linearVelocity.y); // - Update the rigidbody Y velocity float within the animator controller in real-time    
    }

    private void GroundCheck() 
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheckPoint.position, 0.3f, groundLayer);
        isGrounded = (col != null);
        anim.SetBool("Grounded", isGrounded); // - Update the ground state boolean within the animator controller in real-time
    }


    // --------------------------------------------------------------------------------------------------------------

    /// - TEMP: Transition between animations to demonstrate functionality of animations
    /// - Remove and replace with AI controlled state swapping
    private int stateSwapIndex = 0;

    /// - TEMP: Transition between animations to demonstrate functionality of animations
    /// - Remove and replace with AI controlled state swapping
    private void Start() 
    {
        NextAnimationState();
    }

    /// - TEMP: Transition between animations to demonstrate functionality of animations
    /// - Remove and replace with AI controlled state swapping
    private IEnumerator TempAnimationStateSwapper() 
    {
        ChangeState((HeroState)stateSwapIndex);

        var animationClipInfo = anim.GetCurrentAnimatorClipInfo(0);
        float clipLength = animationClipInfo[0].clip.length;        
        float nextAnimationTimer = (stateSwapIndex == 5 || stateSwapIndex == 6) ? clipLength : clipLength + 2.0f; // - Adding delay between animations to allow for rest, except during attack combos.
    
        yield return new WaitForSeconds(nextAnimationTimer);

        // - Increment the state index to cycle through animations for the demo
        if (stateSwapIndex < 9) stateSwapIndex++;   
        else {
            stateSwapIndex = 0;         // - Reset back to first animation if we reach death animation
            anim.SetTrigger("Reset");   // - To reset from death state to idle state.  Temporary  
        }
        NextAnimationState();
    } 

    /// - TEMP: Transition between animations to demonstrate functionality of animations
    /// - Remove and replace with AI controlled state swapping
    private void NextAnimationState() 
    {
        StartCoroutine(TempAnimationStateSwapper());
    }

    /// - TEMP: Transition between animations to demonstrate functionality of animations
    /// - Remove and replace with AI controlled state swapping
    private void ChangeState(HeroState newState)     
    {
        currentState = newState;

        switch (newState) 
        {
            case HeroState.Idle:                    
                    anim.SetInteger("AnimState", 0);                                      
                    break;
                case HeroState.Run:
                    anim.SetInteger("AnimState", 1);
                    break;
                case HeroState.Jump:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Jump");
                    rigid.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse); //- Add some upward velocity to match animation
                    break;                
                case HeroState.Roll:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Roll");
                    break;
                case HeroState.Block:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Block");
                    break;
                case HeroState.Attack1:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Attack1");                  
                    break;
                case HeroState.Attack2:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Attack2");
                    break;
                case HeroState.Attack3:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Attack3");
                    break;
                case HeroState.Hurt:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Hurt");
                    break;
                case HeroState.Dead:
                    anim.SetInteger("AnimState", 0);
                    anim.SetTrigger("Death");
                    break;
        }
    }
}
