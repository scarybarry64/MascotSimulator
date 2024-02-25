using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] _dialogue_list;
    [SerializeField] UnityEvent m_onDialogueFinished;

    public void TriggerDialogue()
    {
        StopAllCoroutines();
        StartCoroutine(StartConversation());
    }
    public IEnumerator StartConversation()
    {
        DialogueManager manager = FindObjectOfType<DialogueManager>();
        if (manager)
        {
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
        }
        m_onDialogueFinished.Invoke();
        StopAllCoroutines();
    }
}
