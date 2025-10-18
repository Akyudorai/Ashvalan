using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerController pc;

    private void Awake() 
    {
        pc = GetComponentInParent<PlayerController>();
    }

    public void StartSpearDash() 
    {
        pc.StartSpearDash();
    }

    public void StopSpearDash()
    {
        pc.StopSpearDash();
    }
}
