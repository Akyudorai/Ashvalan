using System.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Attacking", fileName = "HeroAttackingState")]
public class HeroAttackingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.ATTACKING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;

        // - Set motion to zero
        hero.stats.MotionX = 0;

        // - Toggle attacking status and reset combo
        hero.stats.attackCombo = 0;
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        Debug.LogWarning("Active State is Updating");

        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;

        Debug.LogWarning("POST UPDATING");

        // - Set motion to zero
        hero.stats.MotionX = 0;

        // - If still within melee range, perform a melee attack
        if (hero.detection.distanceToPlayer <= hero.stats.meleeDistance && !hero.stats.isAttacking && hero.stats.attackTimer <= 0f)
        {
            Debug.LogWarning("HERO: I'm starting my attack!");

            // - Attack the player
            hero.StartAttack1();
        }

        // - If the player gets out of range, switch to chase mode
        else if (hero.detection.distanceToPlayer > hero.stats.meleeDistance && !hero.stats.isAttacking)
        {
            Debug.LogWarning("HERO: The boss got away from me.  I'm moving after him!");

            // - Chase the player again
            hero.ChangeState(HeroState.CHASING);
        }

    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting move state
        hero.stats.MotionX = 0f;

        // - Reset blocking status
        hero.stats.isAttacking = false;
    }
}
