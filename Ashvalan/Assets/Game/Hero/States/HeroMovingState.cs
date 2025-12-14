using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Moving", fileName = "HeroMovingState")]
public class HeroMovingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.MOVING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;
    }

    // - NOTE: Moving state works outside of combat
    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Calculate Distance to Destination
        float distanceToDestination = Vector3.Distance(hero.transform.position, hero.stats.targetMoveDestination);

        // - Move Towards Target Position
        float direction = Mathf.Sign(hero.stats.targetMoveDestination.x - hero.transform.position.x);
        hero.stats.MotionX = direction * hero.stats.MoveSpeed;

        // - Flip hero to face direction
        if (direction != 0)
        {
            hero.transform.localScale = new Vector3(Mathf.Sign(direction), 1f, 1f);
        }

        // - Switch to CHASE state if outside melee distance
        if (distanceToDestination < 2.5f)
        {
            Debug.LogWarning("HERO: Done Moving, switching to IDLE state.");
            hero.MoveCompleted?.Invoke("");
            hero.MoveCompleted = null;
            hero.ChangeState(HeroState.IDLE);
        }
    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting move state
        hero.stats.MotionX = 0f;
    }
}
