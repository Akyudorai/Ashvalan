using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SkillTimer : MonoBehaviour
{
    public float cooldown = 0f;

    public TMP_Text skillTimerDisplay;
    public Image skillTimerOverlay;

    private void Update() 
    {
        if (cooldown > 0) 
        {
            cooldown -= Time.deltaTime;
            if (cooldown < 0) cooldown = 0;

            skillTimerDisplay.text = Mathf.Ceil(cooldown).ToString();
            skillTimerOverlay.fillAmount = cooldown / 3f; // - Assuming max cooldown is 3 seconds for overlay representation
        } 
        else 
        {
            skillTimerDisplay.text = "";
            skillTimerOverlay.fillAmount = 0f;
        }
    }
}
