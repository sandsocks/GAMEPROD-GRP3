using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)] public string text;
        public string speakerName;
        public Sprite characterImage;
        public AudioClip voiceClip;
    }

    public DialogueLine[] lines;
}
