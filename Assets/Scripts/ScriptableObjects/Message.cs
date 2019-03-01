using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Message",fileName = "MessageSO")]
public class Message : ScriptableObject
{
    public string text;
    public float timeBetweenLetters = 0.01f;
    public Color color = Color.white;
}