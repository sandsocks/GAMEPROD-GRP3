using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)] public string text;
    public Sprite characterImage;
    public AudioClip voiceClip;
}
