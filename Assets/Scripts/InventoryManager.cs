using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public List<Item> items = new List<Item>();

    public Transform inventoryContent;
    public GameObject inventoryItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public void ListItems()
    {
        foreach (Transform item in inventoryContent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in items)
        {
            GameObject obj = Instantiate(inventoryItem, inventoryContent);
            var itemName = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var itemIcon = obj.transform.GetChild(1).GetComponent<Image>();

            itemName.text = item.name;
            itemIcon.sprite = item.image;
        }
    }
}
