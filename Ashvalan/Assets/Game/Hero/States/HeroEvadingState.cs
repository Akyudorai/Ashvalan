using UnityEngine;

[CreateAssetMenu(menuName = "Hero/States/Evading", fileName = "HeroEvadingState")]
public class HeroEvadingState : HeroStateBehavior
{
    private void OnEnable() => state = HeroState.EVADING;

    public override void OnEnter(HeroBehaviorController hero)
    {
        // - Reset state time
        hero.stateTime = 0f;

        // - Set motion to zero
        hero.stats.MotionX = 0;
    }

    public override void OnUpdate(HeroBehaviorController hero)
    {
        // - Do nothing if combat is not active
        if (!Game.isCombatActive) return;

        // - Analyze Which Attack is Coming
        int incomingAttack = hero.detection.player.GetComponentInChildren<PlayerAnimationEvents>().currentAttack;
        /// - Reminder: 1 = sword attack, 2 = spear dash, 3 = chain pull            

        // - Reaction to Sword Attack
        if (incomingAttack == 1)
        {
            // - Select either to block or dodge
            /// - TODO: Replace with Decision Algorithm
            int rand = Random.Range(0, 100);
            if (rand > 50) hero.Block();
            else
            {
                if (hero.stats.rollTimer <= 0f) hero.DodgeRoll();
                else hero.Block();
            }
        }

        // - Reaction to Spear Dash
        if (incomingAttack == 2)
        {
            Vector3 playerPos = hero.detection.player.transform.position;
            Vector3 playerVel = hero.detection.pc.rb.linearVelocity;
            float dashSpeed = hero.detection.pc.dashPower;

            // - Boss has begun their dash attack
            if (playerVel.magnitude > 1f)
            {
                Debug.Log("Boss is dashing");
                // - Roll towards player to dodge
                if (hero.detection.distanceToPlayer < 17f && hero.stats.rollTimer <= 0f) // - May need to adjust reaction distance
                {
                    Debug.Log("I'm dodging");
                    Vector3 rollDirection = (hero.detection.player.transform.position - hero.transform.position).normalized;
                    float newX = ((rollDirection.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(rollDirection.x));
                    hero.DodgeRoll((int)newX);
                }
                else
                {
                    hero.Block();
                }
            }
        }

        // - Reaction to Chain Pull
        if (incomingAttack == 3)
        {
            // - Boss has thrown their chain
            if (hero.detection.distanceToPlayer < 11.5f + hero.transform.localScale.x)
            {
                // - Jump or Roll Backwards Away From Attack
                int rand = Random.Range(0, 100);
                if (rand > 50)
                {
                    if (hero.stats.rollTimer <= 0f)
                    {
                        Vector3 rollDirection = (hero.detection.player.transform.position - hero.transform.position).normalized;
                        float newX = ((rollDirection.x < 0) ? -1 : 1) * Mathf.Ceil(Mathf.Abs(rollDirection.x));
                        hero.DodgeRoll((int)newX);
                    }

                    else
                    {
                        hero.Block();
                    }

                }

                else
                {
                    hero.Block();
                }
            }
        }
    }

    public override void OnExit(HeroBehaviorController hero)
    {
        // - Additional logic for exiting evasion state
        hero.stats.MotionX = 0f;
    }
}
