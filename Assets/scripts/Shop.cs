// Inventory.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop instance;
    public List<ShopItem> items = new List<ShopItem>();
    public GameObject slotPrefab;
    public Transform slotContainer;

    public static UnityEvent<HookType> hookBought = new();
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
    }

    public void AddItem(ShopItem item)
    {
        items.Add(item);
        UpdateUI();
    }

    public void RemoveItem(ShopItem item)
    {
        items.Remove(item);
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
        foreach(var item in items)
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
            image.sprite = item.hookType.inventoryItem.icon;
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            text.text = item.hookType.inventoryItem.itemName;
            var text2 = slot.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            text2.text = item.price.ToString() + " stars";
            var button = slot.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => BuyItem(item));
            index++;
        }
    }

    public void BuyItem(ShopItem item)
    {
        if (PlayerController.instance.stars >= item.price)
        {
            GameManager.instance.AddStars(-item.price);
            hookBought.Invoke(item.hookType);
        }
    }

}
