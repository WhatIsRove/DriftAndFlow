using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName ="New Quest", menuName = "Quest/Create New Item")]
public class Quest : ScriptableObject
{
    //public int id;
    public string questName;
    [TextArea]
    public string questDesc;
    public int questCounter;
    public int questMaxCounter;
    public List<Item> questItems;
    public int reward;
    public bool collected = false;
    public bool completed = false;
}
