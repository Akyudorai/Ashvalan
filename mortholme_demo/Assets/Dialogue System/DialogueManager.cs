using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public delegate void OnYarnDialogueComplete();
    public static OnYarnDialogueComplete YarnDialogueComplete;

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
}
