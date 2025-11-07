using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Death", fileName = "HeroDeathState")]
public class HeroDeathState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.DEAD;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;

        // - Set motion to zero
        hero.stats.MotionX = 0;

        // - Toggle Death state and trigger animation
        hero.stats.isDead = true;
        hero.anim.SetTrigger("Death");
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;


    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting move state
    }
}
