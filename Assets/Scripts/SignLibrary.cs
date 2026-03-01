using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HandsOfValor/Sign Library")]
public class SignLibrary : ScriptableObject
{
    public List<SignEntry> signs = new List<SignEntry>();
}