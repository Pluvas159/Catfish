// Inventory.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public Dictionary<FishType, int> items = new Dictionary<FishType, int>();
    public GameObject slotPrefab;
    public Transform slotContainer;

    public FishType testItem;

    public int testItemCount = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple Inventory in scene");
        }
    }


    private void Start()
    {
        UpdateUI();
        for (int i = 0; i < testItemCount; i++)
        {
            AddItem(testItem);
        }
        FishAI.FishHooked += OnFishHooked;
    }

    public void AddItem(FishType item)
    {
        if (items.ContainsKey(item))
        {
            items[item]++;
        }
        else
        {
            items[item] = 1;
        }
        UpdateUI();
    }

    public void RemoveItem(FishType item)
    {
        if (items.ContainsKey(item))
        {
            items[item]--;
            if (items[item] <= 0)
            {
                items.Remove(item);
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Disable all slots
        for (int i = 0; i < slotContainer.childCount; i++)
        {
            slotContainer.GetChild(i).gameObject.SetActive(false);
        }

        // Enable and update necessary slots
        int index = 0;
        foreach(var pair in items)
        {
            if (index >= slotContainer.childCount)
            {
                // Instantiate new slot if necessary
                Instantiate(slotPrefab, slotContainer);
            }

            // Update slot
            var slot = slotContainer.GetChild(index).gameObject;
            slot.SetActive(true);
            var image = slot.GetComponentInChildren<Image>();
            image.sprite = pair.Key.inventoryItem.icon;
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            text.text = pair.Value.ToString();

            var button = slot.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SellFish(pair.Key));
            index++;
        }
    }

    private void OnFishHooked(FishAI fish)
    {
        AddItem(fish.GetFishType());
    }

    public void SellFish(FishType item){
        if (items.ContainsKey(item))
        {
            items[item]--;
            GameManager.instance.AddStars(item.price);
            if (items[item] <= 0)
            {
                items.Remove(item);
            }
        }
        UpdateUI();

    }
}
