using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles crafting weapons from components.
/// Attach this to a forge station in the hub.
/// Connect it to UI to pick components and trigger CraftWeapon().
/// </summary>
public class WeaponForge : MonoBehaviour
{
    [Header("Component Selection")]
    public WeaponComponent selectedBlade;
    public WeaponComponent selectedHandle;
    public WeaponComponent selectedCore;

    [Header("Name Generation")]
    public string[] namePrefixes = { "Echo", "Solar", "Void", "Storm", "Iron", "Shadow" };
    public string[] nameSuffixes = { "Fang", "Edge", "Brand", "Pike", "Spear", "Cleaver", "Reaver" };

    [Header("Base Weapon Template")]
    public WeaponData baseWeaponTemplate; // Optional: used as base for crafted weapons

    public event Action<WeaponData> OnWeaponCrafted;

    public bool CanCraft()
    {
        return selectedBlade != null && selectedHandle != null && selectedCore != null;
    }

    public void ClearSelection()
    {
        selectedBlade = null;
        selectedHandle = null;
        selectedCore = null;
    }

    public void SetBlade(WeaponComponent blade)  => selectedBlade  = blade;
    public void SetHandle(WeaponComponent handle) => selectedHandle = handle;
    public void SetCore(WeaponComponent core)    => selectedCore   = core;

    public WeaponData CraftWeapon()
    {
        if (!CanCraft())
        {
            Debug.LogWarning("WeaponForge: Missing components. Cannot craft weapon.");
            return null;
        }

        // Create a new WeaponData instance at runtime
        WeaponData newWeapon = ScriptableObject.CreateInstance<WeaponData>();

        // Use template if provided
        if (baseWeaponTemplate != null)
        {
            newWeapon.baseDamage = baseWeaponTemplate.baseDamage;
            newWeapon.baseAttackSpeed = baseWeaponTemplate.baseAttackSpeed;
            newWeapon.baseStaminaCost = baseWeaponTemplate.baseStaminaCost;
            newWeapon.baseCritChance = baseWeaponTemplate.baseCritChance;
            newWeapon.baseCritMultiplier = baseWeaponTemplate.baseCritMultiplier;
            newWeapon.primaryDamageType = baseWeaponTemplate.primaryDamageType;
            newWeapon.rarity = baseWeaponTemplate.rarity;
            newWeapon.icon = baseWeaponTemplate.icon;
        }

        // Assign components
        newWeapon.blade = selectedBlade;
        newWeapon.handle = selectedHandle;
        newWeapon.core = selectedCore;

        // Determine rarity by highest part rarity
        newWeapon.rarity = GetHighestRarity(selectedBlade, selectedHandle, selectedCore);

        // Roll name
        newWeapon.weaponName = GenerateWeaponName(newWeapon);

        // Default damage type from core or blade
        newWeapon.primaryDamageType = DetermineDamageType(selectedBlade, selectedCore);

        // Optionally: pick an icon from blade or core if none
        if (newWeapon.icon == null)
        {
            if (selectedBlade != null && selectedBlade.icon != null)
                newWeapon.icon = selectedBlade.icon;
            else if (selectedCore != null && selectedCore.icon != null)
                newWeapon.icon = selectedCore.icon;
        }

        // Add to inventory via InventorySystem
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddCraftedWeapon(newWeapon);
        }

        OnWeaponCrafted?.Invoke(newWeapon);
        Debug.Log($"WeaponForge: Crafted weapon '{newWeapon.weaponName}'.");

        return newWeapon;
    }

    private WeaponRarity GetHighestRarity(params WeaponComponent[] components)
    {
        WeaponRarity highest = WeaponRarity.Common;
        foreach (var comp in components)
        {
            if (comp == null) continue;
            if (comp.rarity > highest)
                highest = comp.rarity;
        }
        return highest;
    }

    private string GenerateWeaponName(WeaponData weapon)
    {
        string prefix = namePrefixes.Length > 0 ? namePrefixes[UnityEngine.Random.Range(0, namePrefixes.Length)] : "Unnamed";
        string suffix = nameSuffixes.Length > 0 ? nameSuffixes[UnityEngine.Random.Range(0, nameSuffixes.Length)] : "Blade";

        // Example: "Void Fang" or "Solar Reaver"
        return $"{prefix} {suffix}";
    }

    private DamageType DetermineDamageType(WeaponComponent blade, WeaponComponent core)
    {
        // Prefer core's element if it has one
        if (core != null && core.damageType != DamageType.Physical && core.elementalStrength > 0f)
            return core.damageType;

        // Fallback to blade
        if (blade != null && blade.damageType != DamageType.Physical && blade.elementalStrength > 0f)
            return blade.damageType;

        return DamageType.Physical;
    }
}
