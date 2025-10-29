using UnityEngine;
using System.Collections.Generic;

public class HeroAnimationEvents : MonoBehaviour
{
    private HeroBehavior hero;

    public int animationState = 0; // - currently used to dictate what kind of hitbox event occurs
    public List<GameObject> targetsHit = new List<GameObject>(); // - Stores a list of targets that have been involved in hitbox collisions to prevent triggering their effects multiple times 
    public BoxCollider2D hitbox;

    private void Awake()
    {
        hero = GetComponentInParent<HeroBehavior>();
    }

    // - Called as an animation event on the first frame of each animation
    public void SetAnimationState(int i)
    {
        animationState = i;

        // - Clear the Hit List at the start of each animation
        targetsHit.Clear();
    }

    private void AttackHitResponse(GameObject hit, int index)
    {
        Debug.Log("Attack ["+index+"] Hit Response");

        // - Add Target to Hit List
        targetsHit.Add(hit);

        // - Deal Damage
        // float damage = 0;
        // if (index == 1) damage = 1;
        // if (index == 2) damage = 2;
        // if (index == 3) damage = 3;       
    }

    public void EndAttack(int i)
    {
        // - Is it safe to attack again?  
        int playerState = hero.player_anim.GetInteger("AnimState");
        if (playerState != 2 && playerState != 3 && playerState != 4 && playerState != 5)
        {
            Debug.LogWarning("HERO: Seems to be safe to continue attacking!");

            if (i == 1) hero.StartAttack2();
            if (i == 2) hero.StartAttack3();

            // - End of Attack Chain
            else
            {
                hero.isAttacking = false;
                hero.attackCombo = 0;
            }
        }

        // - If not, do I back off, dodge, or block?
        else
        {
            // - Calculate Threat from current attack animation
            Debug.LogWarning("HERO: I'm in danger. Switching to retreat mode!");
            hero.ChangeState(HeroState.RETREAT);
        }
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
