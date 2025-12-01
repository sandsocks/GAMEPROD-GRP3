using UnityEngine;

[System.Serializable]
public class StoryStep
{
    [Header("Models to show during this step")]
    public GameObject[] models;

    [Header("Dialogue for this step")]
    public DialogueData dialogue;
}
