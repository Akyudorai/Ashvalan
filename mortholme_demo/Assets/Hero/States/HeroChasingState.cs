using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Chasing", fileName = "HeroChasingState")]
public class HeroChasingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.CHASING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;
        
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;

        // - Move towards player
        float direction = Mathf.Sign(hero.detection.player.transform.position.x - hero.transform.position.x);
        hero.stats.MotionX = direction * hero.stats.MoveSpeed;

        // - Flip hero to face player
        if (direction != 0)
        {
            hero.transform.localScale = new Vector3(Mathf.Sign(direction), 1f, 1f);
        }

        // - Switch to ATTACK state if within melee distance
        if (hero.detection.distanceToPlayer <= hero.stats.meleeDistance)
        {
            hero.ChangeState(HeroState.ATTACKING);
        }
    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting chase state
        hero.stats.MotionX = 0f;
    }
}
