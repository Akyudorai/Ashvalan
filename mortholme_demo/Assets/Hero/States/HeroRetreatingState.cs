using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Retreating", fileName = "HeroRetreatingState")]
public class HeroRetreatingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.RETREATING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;

        // - If the player is too close, run away from the player
        if (hero.detection.distanceToPlayer < hero.stats.safeRangedDistance)
        {
            Debug.LogWarning("HERO: Boss is too close to me.");

            // - Move Away from Player
            float direction = Mathf.Sign(hero.detection.player.transform.position.x + hero.transform.position.x);
            hero.stats.MotionX = direction * hero.stats.MoveSpeed;
        }

        // - If the player is far enough, switch to Idle state
        if (hero.detection.distanceToPlayer >= hero.stats.safeRangedDistance)
        {
            Debug.LogWarning("HERO: I've successfully put enough distance between us.");

            // - Set State To Idle
            hero.ChangeState(HeroState.IDLE);
        }

    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting move state
        hero.stats.MotionX = 0f;

        // - Reset blocking status
        hero.stats.isBlocking = false;
    }
}
