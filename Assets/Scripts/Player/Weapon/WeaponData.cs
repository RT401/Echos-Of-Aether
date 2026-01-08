using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data", fileName = "NewWeapon")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public Sprite icon;
    public WeaponRarity rarity;
    public DamageType primaryDamageType = DamageType.Physical;

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseAttackSpeed = 1.0f; // attacks per second
    public float baseStaminaCost = 10f;
    public float baseCritChance = 0.05f;
    public float baseCritMultiplier = 1.5f;

    [Header("Components")]
    public WeaponComponent blade;
    public WeaponComponent handle;
    public WeaponComponent core;

    public WeaponRuntimeStats BuildRuntimeStats()
    {
        var stats = new WeaponRuntimeStats();

        // Start from base stats
        stats.damage = baseDamage;
        stats.attackSpeed = baseAttackSpeed;
        stats.staminaCost = baseStaminaCost;
        stats.critChance = baseCritChance;
        stats.critMultiplier = baseCritMultiplier;
        stats.damageType = primaryDamageType;

        // Add modifiers from components
        ApplyComponent(blade, ref stats);
        ApplyComponent(handle, ref stats);
        ApplyComponent(core, ref stats);

        // Heavy attack values can be tuned here
        stats.heavyAttackDamageMultiplier = 1.8f;
        stats.heavyAttackStaminaMultiplier = 1.5f;

        // Clamp some values to reasonable ranges
        stats.attackSpeed = Mathf.Max(0.1f, stats.attackSpeed);
        stats.staminaCost = Mathf.Max(1f, stats.staminaCost);

        return stats;
    }

    private void ApplyComponent(WeaponComponent component, ref WeaponRuntimeStats stats)
    {
        if (component == null) return;

        stats.damage += component.damageModifier;
        stats.attackSpeed += component.attackSpeedModifier;
        stats.staminaCost += component.staminaCostModifier;
        stats.critChance += component.critChanceModifier;
        stats.critMultiplier += component.critMultiplierModifier;

        // Element override or blend
        if (component.damageType != DamageType.Physical && component.elementalStrength > 0f)
        {
            stats.damageType = component.damageType;
            stats.elementalStrength += component.elementalStrength;
        }
    }
}

[System.Serializable]
public class WeaponRuntimeStats
{
    public float damage;
    public float attackSpeed; // attacks per second
    public float staminaCost;
    public float critChance;
    public float critMultiplier;

    public DamageType damageType;
    public float elementalStrength;

    public float heavyAttackDamageMultiplier;
    public float heavyAttackStaminaMultiplier;

    public float GetLightAttackDamage()
    {
        return damage;
    }

    public float GetHeavyAttackDamage()
    {
        return damage * heavyAttackDamageMultiplier;
    }

    public float GetLightStaminaCost()
    {
        return staminaCost;
    }

    public float GetHeavyStaminaCost()
    {
        return staminaCost * heavyAttackStaminaMultiplier;
    }
}
