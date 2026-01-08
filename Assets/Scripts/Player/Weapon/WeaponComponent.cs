using UnityEngine;

public enum WeaponPartType
{
    Blade,
    Handle,
    Core
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Void
}

public enum WeaponRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Weapons/Weapon Component", fileName = "NewWeaponComponent")]
public class WeaponComponent : ScriptableObject
{
    [Header("General")]
    public string partName;
    public WeaponPartType partType;
    public WeaponRarity rarity;
    public Sprite icon;

    [Header("Stat Modifiers")]
    public float damageModifier = 0f;
    public float attackSpeedModifier = 0f;
    public float staminaCostModifier = 0f;
    public float critChanceModifier = 0f;
    public float critMultiplierModifier = 0f;

    [Header("Element")]
    public DamageType damageType = DamageType.Physical;
    public float elementalStrength = 0f;

    [TextArea]
    public string description;
}
