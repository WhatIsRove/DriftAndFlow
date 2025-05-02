using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bait", menuName = "Bait/Create New Bait")]
public class Bait : ScriptableObject
{
    public string baitName;
    public Sprite image;
    [TextArea]
    public string description;
    public int amount;
    public int maxAmount = 20;
    public int price;
    public bool equipped = false;

    public float rarityBoost;
    public float sizeBoost;
    public float goldValueBoost;

    public List<Biome> effectiveBiomes;
}