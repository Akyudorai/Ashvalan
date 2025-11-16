using UnityEngine;
using System.Collections.Generic;

/*
    Animation States
    - 0 = Idle
    - 1 = Moving
    - 2 = Attack
    - 3 = Spear
    - 4 = Chain
    - 5 = Spell
    - 6 = Buff
*/


public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerController pc;

    public int animationState = 0; // - currently used to dictate what kind of hitbox event occurs
    public List<GameObject> targetsHit = new List<GameObject>(); // - Stores a list of targets that have been involved in hitbox collisions to prevent triggering their effects multiple times 
    public BoxCollider2D hitbox;

    public bool isPulling = false;
    public int currentAttack = 0;

    public delegate void AttackHandler(string phase, float impactTime);
    public static event AttackHandler OnAttackAnimation;


    private void Awake() 
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        // - Handle the pull effect of the chain attack
        if (isPulling)
        {
            foreach (GameObject hit in targetsHit)
            {
                hit.transform.position = hitbox.bounds.center;
            }
        }
    }
   
    public void PlayClip(string name)
    {
        pc.audio.PlayClip(name);
    }

    // - 1 = Sword Swing, 2 = Spear Dash, 3 = Chain Pull
    public void AttackStarted(int i)
    {
        currentAttack = i;
        animationState = 2;
        float impactTime =
            (i == 1) ? 0.27f :  // - ~0.27s and ~0.25s between first and second hitbox triggers
            (i == 2) ? 0.40f :  // - ~0.4s before hitbox triggers
            (i == 3) ? 0.34f :  // - ~0.34s before hitbox triggers
            (i == 4) ? 0.3f :   // - ~0.25s before fireblast detonates
            0.5f;               // 0.5s doesn't represent anything, just a placeholder

        OnAttackAnimation?.Invoke("Threat", impactTime);
    }

    public void AttackEnded(int i)
    {   
        // - For some reason, this event is not being called properly on the chain attack,
        // - So we're calling it as an animation event at the beginning of the Idle animation.
        // - Thus, the need for the currentAttack check here.
        if (currentAttack != 0) 
        {
            currentAttack = 0;        
            OnAttackAnimation?.Invoke("Recovery", 0f);
        }
    }

    // - Called as an animation event on the first frame of each animation
    public void SetAnimationState(int i) 
    {
        animationState = i;

        Debug.Log("Setting Animation State To: " + i);

        // - Clear the Hit List at the start of each animation
        targetsHit.Clear();
    }

    public void SetAttackState(int i)
    {
        currentAttack = i;
    }

    public void StartSpearDash() 
    {
        pc.StartSpearDash();
    }

    public void StopSpearDash()
    {
        pc.StopSpearDash();
    }

    private void AttackHitResponse(GameObject hit) 
    {
        Debug.Log("Attack Hit Response");

        HeroBehaviorController hb = hit.GetComponent<HeroBehaviorController>();

        if (hb != null)
        {
            // - Prepare Knockback Values
            Rigidbody2D rigid = hit.GetComponent<Rigidbody2D>();
            Vector3 launchDir = (hit.transform.position - transform.position).normalized;
            float launchForce = 15f;
            
            // - If the Hero is blocking, perform only a slight knockback            
            if (hb.stats.isBlocking)
            {
                // - If hero is blocking and looking towards this object (player)
                if (Mathf.Sign(hb.gameObject.transform.localScale.x) != Mathf.Sign(pc.gameObject.transform.localScale.x))
                {
                    // - Hero is facing the player (looking towards this object)
                    launchForce *= 0.25f;
                    rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

                    // - Deal Damage reduced by Block Percentage
                    hit.GetComponent<HealthScript>().DealDamage(10f * (1f - hb.stats.blockReduction));

                    // - TODO: Play Block SFX
                    pc.audio.PlayClip("hBlock");
                }

                else
                {
                    // - Hero is not facing the player (looking away from this object)
                    launchForce *= 1.25f; // - Stronger knockback for missing the block
                    launchDir += Vector3.up * 0.2f; // - Add slight upward force
                    rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

                    // - Apply Stun (prevents blocking or attacking immediately after being hit)
                    hb.ApplyStun(0.5f);

                    // - Turn off isBlocking on hero
                    hb.ChangeState(HeroState.IDLE);
                    hb.stats.isBlocking = false;

                    // - Deal Full Damage since blocking in wrong direction
                    hit.GetComponent<HealthScript>().DealDamage(10f);
                }
                    
                return;
            }
            
            // - Add Target to Hit List
            targetsHit.Add(hit);

            // - Deal Damage
            hit.GetComponent<HealthScript>().DealDamage(10f);

            // - Launch Target Back with Full Force            
            rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

        }
    }

    private void SpearHitResponse(GameObject hit) 
    {
        Debug.Log("Spear Hit Response");

        HeroBehaviorController hb = hit.GetComponent<HeroBehaviorController>();

        if (hb != null)
        {
            // - Prepare Knockback Values
            Rigidbody2D rigid = hit.GetComponent<Rigidbody2D>();
            Vector3 launchDir = (hit.transform.position - transform.position).normalized;
            launchDir += Vector3.up * 0.4f;
            float launchForce = 35f;

            // - If the Hero is blocking, perform only a slight knockback            
            if (hb.stats.isBlocking)
            {
                // - If hero is blocking and looking towards this object (player)
                if (Mathf.Sign(hb.gameObject.transform.localScale.x) != Mathf.Sign(pc.gameObject.transform.localScale.x))
                {
                    // - Hero is facing the player (looking towards this object)
                    launchForce *= 0.25f;
                    rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

                    // - Deal Damage reduced by Block Percentage
                    hit.GetComponent<HealthScript>().DealDamage(15f * (1f - hb.stats.blockReduction));

                    // - TODO: Play Block SFX
                    pc.audio.PlayClip("hBlock");
                }

                else
                {
                    // - Hero is not facing the player (looking away from this object)
                    launchForce *= 1.25f; // - Stronger knockback for missing the block
                    launchDir += Vector3.up * 0.2f; // - Add slight upward force
                    rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

                    // - Apply Stun (prevents blocking or attacking immediately after being hit)
                    hb.ApplyStun(0.5f);

                    // - Turn off isBlocking on hero
                    hb.ChangeState(HeroState.IDLE);
                    hb.stats.isBlocking = false;

                    // - Apply Stun (prevents blocking or attacking immediately after being hit)
                    hb.ApplyStun(0.5f);

                    // - Deal Full Damage since blocking in wrong direction
                    hit.GetComponent<HealthScript>().DealDamage(15f);
                }

                return;
            }

            // - Add Target to Hit List
            targetsHit.Add(hit);

            // - Deal Damage
            hit.GetComponent<HealthScript>().DealDamage(15f);

            // - Launch Target Back with Full Force            
            rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

        }
    }

    private void ChainHitResponse(GameObject hit)
    {
        Debug.Log("Chain Hit Response");

        HeroBehaviorController hb = hit.GetComponent<HeroBehaviorController>();

        if (hb != null)
        {
            if (hb.stats.isBlocking)
            {
                // - If hero is blocking and looking towards this object (player)
                if (Mathf.Sign(hb.gameObject.transform.localScale.x) != Mathf.Sign(pc.gameObject.transform.localScale.x))
                {
                    // - Hero is facing the player (looking towards this object)

                    // - Deal Damage reduced by Block Percentage
                    hit.GetComponent<HealthScript>().DealDamage(7f * (1f - hb.stats.blockReduction));

                    // - TODO: Play Block SFX
                    pc.audio.PlayClip("hBlock");

                    return;
                }

                else
                {
                    // - Hero is not facing the player (looking away from this object)

                    // - Turn off isBlocking on hero
                    hb.ChangeState(HeroState.IDLE);
                    hb.stats.isBlocking = false;                    
                }
            }

            // - Add Target to Hit List
            targetsHit.Add(hit);

            // - Deal Damage
            hb.health.DealDamage(7f);

            hb.ApplyStun(1.5f);
        }
    }

    public void StartPull() 
    {
        isPulling = true;
    }

    public void StopPull()
    {
        isPulling = false;        
    }

    public void SpellActivation() 
    {
        // - Scan for nearby enemy
        Collider2D[] foundColliders = Physics2D.OverlapCircleAll(transform.position, 20f);
        
        foreach (var hit in foundColliders) 
        {

            if (hit.tag == "Hero")
            {   
                // - Instantiate time-delayed Flame Burst at their location
                // -- TODO: Instantiate based on velocity / motion of the target rather than exact position
                Instantiate(pc.fireBlast_prefab, hit.gameObject.transform.position, Quaternion.identity);
                return;
            }
        }
    }


    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Hero")
        {            
            switch (currentAttack)
            {
                case 1: // - Attacking
                    AttackHitResponse(col.gameObject);
                    break;
                case 2: // - Spear
                    SpearHitResponse(col.gameObject);
                    break;
                case 3: // - Chain
                    ChainHitResponse(col.gameObject);
                    break;
            }
        }
    }
}
