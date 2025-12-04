using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public delegate void DeathHandler();
    public DeathHandler OnDeath;


    public float currentHealth;
    public float maxHealth = 100f;
    public bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void DealDamage(float amount)
    {
        currentHealth -= amount;
        InterfaceManager.Instance.RefreshUI();
    
        if (currentHealth < 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }
}
