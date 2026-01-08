using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class InventoryItem
{
    public string itemName;
    public Sprite icon;
    public int quantity = 1;
    public ItemType itemType;
    public WeaponComponent weaponPart;
}

public enum ItemType
{
    Resource,
    WeaponPart,
    CraftedWeapon,
    Consumable
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 30;
    public List<InventoryItem> items = new List<InventoryItem>();
    public event Action OnInventoryUpdated;

    public bool AddItem(InventoryItem newItem)
    {
        // If the item already exists, increase quantity
        foreach (var item in items)
        {
            if (item.itemName == newItem.itemName)
            {
                item.quantity += newItem.quantity;
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // Add new item if there's space
        if (items.Count < maxSlots)
        {
            items.Add(newItem);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        Debug.LogWarning("inventory is full");
        return false;
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == itemName)
            {
                items[i].quantity -= amount;
                if (items[i].quantity <= 0)
                    items.RemoveAt(i);

                OnInventoryUpdated?.Invoke();
                return;
            }
        }
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        foreach (var item in items)
        {
            if (item.itemName == itemName && item.quantity >= amount)
                return true;
        }
        return false;
    }

    public InventoryItem GetItem(string itemName)
    {
        return items.Find(item => item.itemName == itemName);
    }

    public void PrintInventory()
    {
        Debug.Log("inventory Context: ");
        foreach (var item in items)
        {
            Debug.Log($"{item.itemName} x{item.quantity}");
        }
    }
}
