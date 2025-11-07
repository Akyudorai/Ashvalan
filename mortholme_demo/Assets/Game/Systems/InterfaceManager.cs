using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public HealthScript bossHealth, heroHealth;

    public GameObject heroHealthPanel;
    public TMP_Text heroHealthDisplay;
    public Image heroHealthSlider;

    public GameObject bossHealthPanel;
    public TMP_Text bossHealthDisplay;
    public Image bossHealthSlider;

    // - Skill Timers
    public GameObject combatskillsPanel;
    public GameObject passiveSKillsPanel;
    public SkillTimer swordSkillTimer;
    public SkillTimer dashSkillTimer;
    public SkillTimer chainSkillTimer;
    public SkillTimer fireblastSkillTimer;

    private void Awake()
    {
        Instance = this;

        ToggleCombat();
        RefreshUI();
    }

    private void Update() 
    {
        ToggleCombat();
    }

    public void ToggleCombat()
    {
        heroHealthPanel.SetActive(Game.isCombatActive);
        bossHealthPanel.SetActive(Game.isCombatActive);
        combatskillsPanel.SetActive(Game.isCombatActive);
    }

    public void TogglePassive(bool state) 
    {
        passiveSKillsPanel.SetActive(state);
    }

    public void RefreshUI()
    {
        if (heroHealth != null)
        {
            heroHealthDisplay.text = $"Hero Health: { Mathf.Ceil(heroHealth.currentHealth) } / { heroHealth.maxHealth}";
            heroHealthSlider.fillAmount = heroHealth.currentHealth / heroHealth.maxHealth;        
        }
        
        if (bossHealth != null)
        {
            bossHealthDisplay.text = $"Boss Health: { Mathf.Ceil(bossHealth.currentHealth) } / { bossHealth.maxHealth}";
            bossHealthSlider.fillAmount = bossHealth.currentHealth / bossHealth.maxHealth;
        }
        
    }

    public void StartSkillTimer(int index, float cooldown)
    {
        if (index == 1) swordSkillTimer.cooldown = cooldown;
        if (index == 2) dashSkillTimer.cooldown = cooldown;
        if (index == 3) chainSkillTimer.cooldown = cooldown;
        if (index == 4) fireblastSkillTimer.cooldown = cooldown;
    }

    public float GetSkillCooldown(int index)
    {
        if (index == 1) return swordSkillTimer.cooldown;
        if (index == 2) return dashSkillTimer.cooldown;
        if (index == 3) return chainSkillTimer.cooldown;
        if (index == 4) return fireblastSkillTimer.cooldown;

        return 0f;
    }


}
