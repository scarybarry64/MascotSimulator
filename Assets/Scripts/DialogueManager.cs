using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private DialogueBoxUI _dialogue_ui;

    public Transform _canvas_transform;
    private Queue<string> _dialogue_full;
    private bool _typing_done;

    // Start is called before the first frame update
    void Start()
    {
        _dialogue_full = new Queue<string>();
        enabled = false;
        _typing_done = false;
        //creates an instance of prefab and spawn it
        _dialogue_ui = Instantiate(Resources.Load<DialogueBoxUI>("DialogueBox"), _canvas_transform.transform.position, Quaternion.identity, _canvas_transform);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (!_dialogue_ui._dialogue_animation.GetBool("IsOpen"))
        {
            _dialogue_ui._speaker_image.sprite = dialogue._speaker_image;
            _dialogue_ui._dialogue_animation.SetBool("IsOpen", true);
            enabled = true;
            _dialogue_ui._name_object.text = dialogue._dialogue_name;

            _dialogue_full.Clear();

            foreach (string sentence in dialogue._dialogue_text)
            {
                _dialogue_full.Enqueue(sentence);
            }
            DisplayNextSentence();
        }
        else
        {
            if (!_typing_done)
                _typing_done = true;
            else
                DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (_dialogue_full.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = _dialogue_full.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        _dialogue_ui._dialogue_object.text = "";
        _typing_done = false;
        foreach (char letter in sentence.ToCharArray())
        {
            _dialogue_ui._dialogue_object.text += letter;
            yield return new WaitForSeconds(0.02f);
            if (_typing_done)
            {
                _dialogue_ui._dialogue_object.text = sentence;
                break;
            }
        }
        _typing_done = true;
    }

    void EndDialogue()
    {
        _dialogue_ui._dialogue_animation.SetBool("IsOpen", false);
        _typing_done = false;
        Debug.Log("End of conversation.");
        enabled = false;
    }
}
