using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public int gold;
    public TextMeshProUGUI goldText;

    public List<Item> items = new List<Item>();
    public List<Bait> baits = new List<Bait>();

    public Transform inventoryContent;
    public Transform invBaitContent;

    public Transform shopContent;
    public Transform shopBaitContent;
    public Transform shopUpgContent;

    public GameObject inventoryItem;
    public GameObject baitItem;

    public GameObject shopItem;
    public GameObject shopBuyItem;

    public GameObject toolTip;
    public TextMeshProUGUI toolTipText;

    public Bait currentEquippedBait;

    Canvas canvas;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Gold"))
        {
            IncrementGold(PlayerPrefs.GetInt("Gold"));
        }

        toolTip.SetActive(false);
        Cursor.visible = true;

        canvas = FindObjectOfType<Canvas>();


        bool equipFound = false;
        foreach (Bait bait in baits)
        {
            if (bait.equipped && !equipFound)
            {
                currentEquippedBait = bait;
                equipFound = true;
            }
        }

        if (!equipFound) currentEquippedBait = baits[0];
        currentEquippedBait.equipped = true;
    }

    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out localPoint);
        localPoint.x += 1380 / 5;
        localPoint.y -= 1000 / 5;

        toolTip.transform.localPosition = localPoint;
    }
    public void ShowToolTip(string msg)
    {
        toolTip.SetActive(true);
        toolTipText.text = msg;
    }

    public void HideToolTip()
    {
        toolTip.SetActive(false);
        toolTipText.text = "";
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public void SellItem(Item item)
    {
        items.Remove(item);
        gold += item.realPrice;
        goldText.text = gold + "g";
        ListShopItems();
    }

    public void BuyBait(Bait bait)
    {
        foreach (Bait selectBait in baits)
        {
            if (selectBait.name == bait.name && selectBait.amount < selectBait.maxAmount)
            {
                selectBait.amount++;
                IncrementGold(-selectBait.price);
            }
        }
        ListBait();
    }

    public void IncrementGold(int amount)
    {
        gold += amount;
        PlayerPrefs.SetInt("Gold", gold);
        goldText.text = gold + "g";

    }

    public void EquipBait(Bait bait)
    {
        currentEquippedBait.equipped = false;
        currentEquippedBait = bait;
        bait.equipped = true;
        ListBait();
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

            itemName.text = item.itemName;
            itemIcon.sprite = item.image;
        }
    }

    public void ListBait()
    {
        foreach (Transform item in invBaitContent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in baits)
        {
            GameObject obj = Instantiate(baitItem, invBaitContent);
            var itemName = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var itemIcon = obj.transform.GetChild(1).GetComponent<Image>();
            var equipButton = obj.transform.GetChild(2).GetComponent<Button>();
            var equipedButton = obj.transform.GetChild(3).GetComponent<Button>();
            var notEnoughButton = obj.transform.GetChild(4).GetComponent<Button>();
            var tooltip = obj.transform.GetComponent<Tooltip>();

            tooltip.description = item.description;

            if (item.amount <= 0)
            {
                equipedButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(false);
                notEnoughButton.gameObject.SetActive(true);
            }

            if (item.equipped) {
                equipedButton.gameObject.SetActive(true);
                equipButton.gameObject.SetActive(false);
                notEnoughButton.gameObject.SetActive(false);
            }
            else if (item.amount > 0)
            {
                equipedButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(true);
                notEnoughButton.gameObject.SetActive(false);
            }

            itemName.text = item.amount + " - " + item.baitName;
            itemIcon.sprite = item.image;
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => { EquipBait(item); });
        }
    }

    public void ListShopItems()
    {
        foreach (Transform item in shopContent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in items)
        {
            GameObject obj = Instantiate(shopItem, shopContent);
            var itemName = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var itemIcon = obj.transform.GetChild(1).GetComponent<Image>();
            var sellButton = obj.transform.GetChild(2).GetComponent<Button>();

            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell - " + item.realPrice + "g";

            itemName.text = item.itemName;
            itemIcon.sprite = item.image;
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => { SellItem(item); });
        }
    }

    public void ListBaitBuy()
    {
        foreach (Transform item in shopBaitContent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in baits)
        {
            GameObject obj = Instantiate(shopBuyItem, shopBaitContent);
            var itemName = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var itemIcon = obj.transform.GetChild(1).GetComponent<Image>();
            var buyButton = obj.transform.GetChild(2).GetComponent<Button>();
            var inactiveButton = obj.transform.GetChild(3).GetComponent<Button>();
            var tooltip = obj.transform.GetComponent<Tooltip>();

            tooltip.description = item.description;

            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy " + item.price + "g";

            if (item.price > gold)
            {
                inactiveButton.gameObject.SetActive(true);
                buyButton.gameObject.SetActive(false);
            } else
            {
                inactiveButton.gameObject.SetActive(false);
                buyButton.gameObject.SetActive(true); 
            }

            itemName.text = item.baitName;
            itemIcon.sprite = item.image;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => { BuyBait(item); });
        }
    }

    public void SwitchInvTab(int i)
    {
        switch (i)
        {
            case 0:
                SetAllInactiveTabs();
                inventoryContent.parent.gameObject.SetActive(true);
                ListItems();
                break;
            case 1:
                SetAllInactiveTabs();
                invBaitContent.parent.gameObject.SetActive(true);
                ListBait();
                break;
        }
    }

    public void SwitchShopTab(int i)
    {
        switch (i)
        {
            case 0:
                SetAllShopTabsInactive();
                shopContent.parent.gameObject.SetActive(true);
                ListShopItems();
                break;
            case 1:
                SetAllShopTabsInactive();
                shopBaitContent.parent.gameObject.SetActive(true);
                ListBaitBuy();
                break;
            case 2:
                SetAllShopTabsInactive();
                shopUpgContent.parent.gameObject.SetActive(true);
                break;
        }
    }

    void SetAllInactiveTabs()
    {
        inventoryContent.parent.gameObject.SetActive(false);
        invBaitContent.parent.gameObject.SetActive(false);
    }

    void SetAllShopTabsInactive()
    {
        shopContent.parent.gameObject.SetActive(false);
        shopBaitContent.parent.gameObject.SetActive(false);
        shopUpgContent.parent.gameObject.SetActive(false);
    }

    public void ResetInventory()
    {
        if (baits.Count > 0)
        {
            foreach (var bait in baits)
            {
                bait.amount = 0;
            }
        }
    }
}
