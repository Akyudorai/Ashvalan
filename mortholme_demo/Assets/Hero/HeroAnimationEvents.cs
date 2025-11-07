using UnityEngine;
using System.Collections.Generic;

public class HeroAnimationEvents : MonoBehaviour
{
    private HeroBehaviorController hero;

    public int animationState = 0; // - currently used to dictate what kind of hitbox event occurs
    public List<GameObject> targetsHit = new List<GameObject>(); // - Stores a list of targets that have been involved in hitbox collisions to prevent triggering their effects multiple times 
    public BoxCollider2D hitbox;

    private void Awake()
    {
        hero = GetComponentInParent<HeroBehaviorController>();
    }

    // - Called as an animation event on the first frame of each animation
    public void SetAnimationState(int i)
    {
        animationState = i;

        // - Clear the Hit List at the start of each animation
        targetsHit.Clear();
    }

    public void PlayClip(string name)
    {
        hero.audio.PlayClip(name);
    }

    private void AttackHitResponse(GameObject hit, int index)
    {
        Debug.Log("Attack [" + index + "] Hit Response");

        // - Add Target to Hit List
        targetsHit.Add(hit);

        hero.audio.PlayClip("hSword");

        // - Deal Damage
        float damage = 0;
        if (index == 1) damage = hero.stats.currentAttackDamage;
        if (index == 2) damage = hero.stats.currentAttackDamage * 1.5f;
        if (index == 3) damage = hero.stats.currentAttackDamage * 2f;
        hit.GetComponent<HealthScript>().DealDamage(damage);
    }

    public void EndAttack(int i)
    {
        // - Is it safe to attack again?  
        int playerState = hero.detection.pc.GetComponentInChildren<PlayerAnimationEvents>().animationState;
        if (playerState != 2 && playerState != 3 && playerState != 4 && playerState != 5)
        {
            Debug.LogWarning("HERO: Seems to be safe to continue attacking!");

            if (i == 1 && hero.stats.attackTimer <= 0.1f) hero.StartAttack2();
            if (i == 2 && hero.stats.attackTimer <= 0.1f) hero.StartAttack3();

            // - End of Attack Chain
            else
            {
                hero.stats.isAttacking = false;
                hero.stats.attackCombo = 0;
            }
        }

        // - If not, do I back off, dodge, or block?
        else
        {
            // - Calculate Threat from current attack animation
            Debug.LogWarning("HERO: I'm in danger. Switching to retreat mode!");
            hero.ChangeState(HeroState.EVADING);
        }
    }

    public void StopDodgeRoll()
    {
        hero.StopDodgeRoll();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name);

        if (col.gameObject.tag == "Player")
        {
            switch (animationState)
            {
                case 5: // - Attack 1
                    AttackHitResponse(col.gameObject, 1);
                    break;
                case 6: // - Attack 2
                    AttackHitResponse(col.gameObject, 2);
                    break;
                case 7: // - Attack 3
                    AttackHitResponse(col.gameObject, 3);
                    break;
            }
        }
    }
}
