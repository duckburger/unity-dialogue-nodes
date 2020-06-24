using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Dialogue Character", menuName = "Dialogue Editor/Character Asset")]
public class DialogueCharacter : ScriptableObject
{
    public string charName;
    public Sprite icon;
    public Color textColor = Color.black;

}
