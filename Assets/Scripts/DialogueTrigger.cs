using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] _dialogue_list;

    public void TriggerDialogue()
    {
        StopAllCoroutines();
        StartCoroutine(StartConversation());
    }
    public IEnumerator StartConversation()
    {
        DialogueManager manager = FindObjectOfType<DialogueManager>();

        //loop through all dialogues
        foreach (Dialogue speaker in _dialogue_list)
        {
            //start dialogue
            manager.StartDialogue(speaker);
            //a dialogue is still active
            while (!manager.IsDialogueFinished())
            {
                //wait for dialogue to finish
                yield return null;
            }
        }
        Debug.Log("I'm done talking");
    }
}
