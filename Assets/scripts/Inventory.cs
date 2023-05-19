// Inventory.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Dictionary<InventoryItem, int> items = new Dictionary<InventoryItem, int>();
    public GameObject slotPrefab;
    public Transform slotContainer;

    public InventoryItem testItem;

    public int testItemCount = 1;



    private void Start()
    {
        UpdateUI();
        for (int i = 0; i < testItemCount; i++)
        {
            AddItem(testItem);
        }
    }

    public void AddItem(InventoryItem item)
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

    public void RemoveItem(InventoryItem item)
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
            image.sprite = pair.Key.icon;
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            text.text = pair.Value.ToString();
            index++;
        }
    }
}
