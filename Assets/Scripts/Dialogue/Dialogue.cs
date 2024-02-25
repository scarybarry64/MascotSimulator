using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public string _dialogue_name;

    [TextArea(3, 10)]
    public string[] _dialogue_text;

    public Sprite _speaker_image;
}
