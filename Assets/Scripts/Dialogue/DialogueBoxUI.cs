using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueBoxUI : MonoBehaviour
{
    public TextMeshProUGUI _name_object;
    public TextMeshProUGUI _dialogue_object;
    public Image _speaker_image;
    [HideInInspector] public Animator _dialogue_animation;

    // Start is called before the first frame update
    void Awake()
    {
        _dialogue_animation = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
