using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemDrop
{
    public Item item;
    [Range(0f, 1f)]
    public float dropChance;
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Loot/Create New Loot Table")]
public class LootTable : ScriptableObject
{
    public List<ItemDrop> drops;

    public Item GetRandomItem()
    {
        float totalWeight = 0f;
        foreach (var drop in drops) { 
            totalWeight += drop.dropChance;
        }

        float roll = Random.Range(0f, totalWeight);

        float currentTotal = 0f;
        foreach (var drop in drops)
        {
            currentTotal += drop.dropChance;
            if (roll <= currentTotal) return drop.item;
        }

        return null;
    }
}