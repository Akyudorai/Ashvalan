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
        if (animationState == 4 && isPulling) 
        {               
            foreach (GameObject hit in targetsHit) 
            {                
                hit.transform.position = hitbox.bounds.center;                
            }
        }
    }
   
    // - 1 = Sword Swing, 2 = Spear Dash, 3 = Chain Pull
    public void AttackStarted(int i)
    {
        currentAttack = i;
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
        currentAttack = 0;
        OnAttackAnimation?.Invoke("Recovery", 0f);
    }

    // - Called as an animation event on the first frame of each animation
    public void SetAnimationState(int i) 
    {
        animationState = i;

        // - Clear the Hit List at the start of each animation
        targetsHit.Clear();
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

        HeroBehavior hb = hit.GetComponent<HeroBehavior>();

        if (hb != null)
        {
            // - Prepare Knockback Values
            Rigidbody2D rigid = hit.GetComponent<Rigidbody2D>();
            Vector3 launchDir = (hit.transform.position - transform.position).normalized;
            float launchForce = 12f;
            
            // - If the Hero is blocking, perform only a slight knockback            
            if (hb.isBlocking)
            {
                launchForce *= 0.25f;
                rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);

                // - TODO: Play Block SFX

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

        HeroBehavior hb = hit.GetComponent<HeroBehavior>();

        if (hb != null)
        {
            // - Prepare Knockback Values
            Rigidbody2D rigid = hit.GetComponent<Rigidbody2D>();
            Vector3 launchDir = (hit.transform.position - transform.position).normalized;
            launchDir += Vector3.up * 0.4f;
            float launchForce = 12f;

            // - If the Hero is blocking, perform only a slight knockback            
            if (hb.isBlocking)
            {
                launchForce *= 0.25f;
                rigid.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
                
                // - TODO: Play Block SFX


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

        HeroBehavior hb = hit.GetComponent<HeroBehavior>();

        if (hb != null)
        {
            if (hb.isBlocking)
            {
                // - TODO: Play Block SFX

                return;
            }

            // - Add Target to Hit List
            targetsHit.Add(hit);

            // - Deal Damage
            hit.GetComponent<HealthScript>().DealDamage(7f);

            // - Apply Stun to target for duration of animation

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
            switch (animationState)
            {
                case 2: // - Attacking
                    AttackHitResponse(col.gameObject);
                    break;
                case 3: // - Spear
                    SpearHitResponse(col.gameObject);
                    break;
                case 4: // - Chain
                    ChainHitResponse(col.gameObject);
                    break;
            }
        }
    }
}
