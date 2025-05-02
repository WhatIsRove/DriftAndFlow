using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public Transform questContent;

    public List<Quest> quests = new List<Quest>();

    public GameObject questItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddQuest(Quest quest)
    {
        quests.Add(quest);
    }

    public void RemoveQuest(Quest quest)
    {
        quests.Remove(quest);
    }

    public void ListQuests()
    {
        foreach (Transform quest in questContent)
        {
            Destroy(quest.gameObject);
        }

        var sortedQuests = quests.OrderByDescending(q => q.completed && !q.collected) .ThenByDescending(q => !q.completed).ToList();

        foreach (var quest in sortedQuests)
        {
            GameObject obj = Instantiate(questItem, questContent);

            var questName = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var questItemName = obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var questItemIcon = obj.transform.GetChild(2).GetComponent<Image>();
            var questSlider = obj.transform.GetComponentInChildren<Slider>();
            var questCounterText = obj.transform.GetChild(3).GetComponentInChildren<TextMeshProUGUI>();
            var questRewardText = obj.transform.Find("RewardText").GetComponent<TextMeshProUGUI>();
            var questCompleteButton = obj.transform.GetComponent<Button>();

            questName.text = quest.questName;
            questItemName.text = quest.questDesc;
            questItemIcon.sprite = quest.questItems[0].image;
            questSlider.maxValue = quest.questMaxCounter;
            questSlider.value = quest.questCounter;

            if (quest.completed && !quest.collected)
            {
                questCounterText.text = "Click to collect";
            } else questCounterText.text = quest.questCounter + " / " + quest.questMaxCounter;

            questRewardText.text = quest.reward+"g";

            questCompleteButton.onClick.RemoveAllListeners();
            questCompleteButton.onClick.AddListener(() => { CompleteQuest(quest); });
        }
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest.completed && !quest.collected)
        {
            quest.collected = true;
            FindObjectOfType<InventoryManager>().IncrementGold(quest.reward);
            ListQuests();
        }
    }

    public void IncrementQuests(Item item)
    {

        foreach (var quest in quests)
        {
            foreach (var questItem in quest.questItems)
            {
                if (questItem.itemName == item.itemName && quest.questCounter < quest.questMaxCounter)
                {
                    quest.questCounter++;
                }
                else if (quest.questCounter >= quest.questMaxCounter) quest.completed = true;
            }
            
        }
    }

    public void ResetQuests()
    {
        foreach (var quest in quests)
        {
            quest.questCounter = 0;
            quest.completed = false;
            quest.collected = false;
        }
    }
}
