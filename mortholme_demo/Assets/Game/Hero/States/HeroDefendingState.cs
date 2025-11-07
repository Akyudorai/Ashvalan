using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Defending", fileName = "HeroDefendingState")]
public class HeroDefendingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.DEFENDING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;

        // - Set motion to zero
        hero.stats.MotionX = 0;

        // Decide whether to look at the player before blocking
        float lookProbability = Mathf.Lerp(0.25f, 0.95f, Mathf.Clamp01(Game.currentLevel / 10f));
        GameObject player = hero.detection.player;

        if (player != null)
        {
            float direction = Mathf.Sign(player.transform.position.x - hero.transform.position.x);
            if (direction == 0f) direction = 1f;

            // - Look at the player
            if (Random.value <= lookProbability)
                hero.transform.localScale = new Vector3(direction, 1f, 1f);

            // - Look away from the player
            else
                hero.transform.localScale = new Vector3(-direction, 1f, 1f);
        }


        // - Toggle block status and trigger animation
        hero.stats.isBlocking = true;
        hero.anim.SetTrigger("Block");
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;    

    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting move state
        hero.stats.MotionX = 0f;

        // - Reset blocking status
        hero.stats.isBlocking = false;
    }

}
