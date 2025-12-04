using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Idle", fileName = "HeroIdleState")]
public class HeroIdleState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.IDLE;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Set motion to zero
        hero.stats.MotionX = 0f;

        // - Reset state time
        hero.stateTime = 0f;
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;

        Debug.LogWarning("IDLE STATE");

        // - Immediately switch to CHASE state if combat is active;
        if (hero.detection.distanceToPlayer > hero.stats.meleeDistance)
        {
            hero.ChangeState(HeroState.CHASING);
        }        
    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting idle state
    }
}
