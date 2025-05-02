using UnityEngine;

[System.Serializable]
public class LootEntry : MonoBehaviour
{
    public Item item;
    [Range(0f, 1f)]
    public float dropChance;
}