using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    private Sprite[] screens;
    private Image screen;

    private void Awake()
    {
        screen = GetComponent<Image>();
        screens = Resources.LoadAll<Sprite>("GameOverScreens");

        foreach (Sprite sprite in screens)
        {

            Debug.Log(sprite.name);

            if (sprite.name.Equals(GameManager.Instance.DeathType.ToString(), System.StringComparison.Ordinal))
            {
                screen.sprite = sprite;
                break;
            }
        }
    }
}
