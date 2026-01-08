using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global access point for the player's inventory.
/// Other systems (loot drops, forge, UI, mission rewards)
/// should talk to this instead of directly to PlayerInventory where possible.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("References")]
    public PlayerInventory playerInventory;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("InventorySystem: No PlayerInventory found in the scene.");
            }
        }
    }

    // ------------------------
    // Item Management
    // ------------------------

    public bool AddItem(InventoryItem newItem)
    {
        if (playerInventory == null) return false;
        return playerInventory.AddItem(newItem);
    }

    public bool AddResource(string name, Sprite icon, int quantity)
    {
        if (playerInventory == null) return false;

        InventoryItem item = new InventoryItem
        {
            itemName = name,
            icon = icon,
            quantity = quantity,
            itemType = ItemType.Resource,
            weaponPart = null
        };

        return playerInventory.AddItem(item);
    }

    public bool AddWeaponPart(WeaponComponent component, int quantity = 1)
    {
        if (playerInventory == null || component == null) return false;

        InventoryItem item = new InventoryItem
        {
            itemName = component.partName,
            icon = component.icon,
            quantity = quantity,
            itemType = ItemType.WeaponPart,
            weaponPart = component
        };

        return playerInventory.AddItem(item);
    }

    public bool AddCraftedWeapon(WeaponData weapon, int quantity = 1)
    {
        if (playerInventory == null || weapon == null) return false;

        InventoryItem item = new InventoryItem
        {
            itemName = weapon.weaponName,
            icon = weapon.icon,
            quantity = quantity,
            itemType = ItemType.CraftedWeapon,
            weaponPart = null // not a single part
        };

        return playerInventory.AddItem(item);
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        if (playerInventory == null) return;
        playerInventory.RemoveItem(itemName, amount);
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        if (playerInventory == null) return false;
        return playerInventory.HasItem(itemName, amount);
    }

    public InventoryItem GetItem(string itemName)
    {
        if (playerInventory == null) return null;
        return playerInventory.GetItem(itemName);
    }

    public List<InventoryItem> GetAllItems()
    {
        return playerInventory != null ? playerInventory.items : new List<InventoryItem>();
    }
}
