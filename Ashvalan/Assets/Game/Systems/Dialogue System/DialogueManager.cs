using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public delegate void OnYarnDialogueComplete();
    public static OnYarnDialogueComplete YarnDialogueComplete;

    public TMP_Text characterNameDisplay, characterDialogueDisplay; 

    [SerializeField] private DialogueRunner runner;

    private void Awake()
    {
        Instance = this;

        if (runner == null)
            runner = GetComponent<DialogueRunner>();        
    }

    [YarnCommand("dialogue_complete")]
    public static void dialogue_complete()
    {
        Game.isDialogueActive = false;
        YarnDialogueComplete?.Invoke();
        YarnDialogueComplete = null;
    }

    public void begin_dialogue(string command = "")
    {
        Game.isDialogueActive = true;
        if (command != "") runner.StartDialogue(command);
        else runner.StartDialogue($"d{(Game.currentLevel + 1).ToString()}");   
    }

    public void begin_hero_wins_dialogue()
    {
        // - Subscribe to move to credits when dialogue complete
        YarnDialogueComplete += () =>
        {
            TransitionHandler.Instance.StartSceneTransition("credits", 2f);  
        };

        if (Game.currentLevel < 3) begin_dialogue("hero_wins_1");
        else if (Game.currentLevel < 6) begin_dialogue("hero_wins_2");
        else begin_dialogue("hero_wins_3");   
    }

    [YarnCommand("toggle_boss_dialogue")]
    public static void toggle_boss_dialogue()
    {
        Instance.characterNameDisplay.alignment = TextAlignmentOptions.Right;
        Instance.characterDialogueDisplay.alignment = TextAlignmentOptions.Right;
    }

    [YarnCommand("toggle_hero_dialogue")]
    public static void toggle_hero_dialogue()
    {
        Instance.characterNameDisplay.alignment = TextAlignmentOptions.Left;
        Instance.characterDialogueDisplay.alignment = TextAlignmentOptions.Left;
    }
}
