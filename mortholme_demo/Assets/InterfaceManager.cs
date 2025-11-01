using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public HealthScript bossHealth, heroHealth;

    public TMP_Text heroHealthDisplay;
    public Image heroHealthSlider;

    public TMP_Text bossHealthDisplay;
    public Image bossHealthSlider;

    private void Awake()
    {
        Instance = this;

        RefreshUI();
    }

    public void RefreshUI()
    {
        heroHealthDisplay.text = $"Hero Health: {heroHealth.currentHealth}/{heroHealth.maxHealth}";
        heroHealthSlider.fillAmount = heroHealth.currentHealth / heroHealth.maxHealth;
        
        bossHealthDisplay.text = $"Boss Health: {bossHealth.currentHealth}/{bossHealth.maxHealth}";
        bossHealthSlider.fillAmount = bossHealth.currentHealth / bossHealth.maxHealth;
    }
}
