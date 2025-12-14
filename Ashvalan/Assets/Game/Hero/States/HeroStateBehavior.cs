using UnityEngine;

[CreateAssetMenu (fileName = "Hero/State", menuName = "NewHeroState")]
public class HeroStateBehavior : ScriptableObject
{
    public HeroState state;

    // - Called when the hero enters this state
    public virtual void OnEnter(HeroBehaviorController hero) { }

    // - Called every frame while the hero is in this state
    public virtual void OnUpdate(HeroBehaviorController hero) { }

    // - Called when the hero exits this state
    public virtual void OnExit(HeroBehaviorController hero) { }
}
