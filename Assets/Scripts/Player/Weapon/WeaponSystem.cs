using UnityEngine;
using System;

[RequireComponent(typeof(PlayerStats))]
public class WeaponSystem : MonoBehaviour
{
    [Header("Equipped Weapon")]
    public WeaponData startingWeapon;
    public WeaponData equippedWeapon { get; private set; }
    public WeaponRuntimeStats runtimeStats { get; private set; }

    [Header("Weapon Attachments")]
    public Transform weaponSocket;       // Hand bone or weapon socket
    public GameObject weaponPrefab;      // Visual prefab for the weapon (optional)

    private GameObject spawnedWeaponInstance;
    private PlayerStats playerStats;

    public event Action<WeaponData, WeaponRuntimeStats> OnWeaponEquipped;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("Tried to equip a null weapon.");
            return;
        }

        equippedWeapon = newWeapon;
        runtimeStats = newWeapon.BuildRuntimeStats();

        SpawnWeaponModel();
        OnWeaponEquipped?.Invoke(equippedWeapon, runtimeStats);
    }

    private void SpawnWeaponModel()
    {
        if (weaponSocket == null) return;

        if (spawnedWeaponInstance != null)
        {
            Destroy(spawnedWeaponInstance);
        }

        if (weaponPrefab != null)
        {
            spawnedWeaponInstance = Instantiate(weaponPrefab, weaponSocket);
            spawnedWeaponInstance.transform.localPosition = Vector3.zero;
            spawnedWeaponInstance.transform.localRotation = Quaternion.identity;
        }
    }

    // These methods are what PlayerCombat (or animation events) will call:

    public bool CanPerformLightAttack()
    {
        if (runtimeStats == null) return false;
        return playerStats.HasStamina(runtimeStats.GetLightStaminaCost());
    }

    public bool CanPerformHeavyAttack()
    {
        if (runtimeStats == null) return false;
        return playerStats.HasStamina(runtimeStats.GetHeavyStaminaCost());
    }

    public void ConsumeStaminaForLightAttack()
    {
        if (runtimeStats == null) return;
        playerStats.UseStamina(runtimeStats.GetLightStaminaCost());
    }

    public void ConsumeStaminaForHeavyAttack()
    {
        if (runtimeStats == null) return;
        playerStats.UseStamina(runtimeStats.GetHeavyStaminaCost());
    }

    public float GetLightAttackDamage()
    {
        if (runtimeStats == null) return 0f;
        return runtimeStats.GetLightAttackDamage();
    }

    public float GetHeavyAttackDamage()
    {
        if (runtimeStats == null) return 0f;
        return runtimeStats.GetHeavyAttackDamage();
    }

    public DamageType GetDamageType()
    {
        if (runtimeStats == null) return DamageType.Physical;
        return runtimeStats.damageType;
    }
}
