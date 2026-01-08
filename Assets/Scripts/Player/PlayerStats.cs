using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float healthRegenRate = 2f;

    [Header("Progression")]
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;

    private float currentHealth;
    private float currentStamina;

    // Events for UI and other systems
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<int> OnLevelUp;


    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    void Update()
    {
        RegenerateStats();
    }

    private void RegenerateStats()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        if (currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void GainXp(float amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
            OnLevelUp();
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel *= 1.25f;
        maxHealth += 10f;
        maxStamina += 10f;
        OnLevelUp?.Invoke(level);
    }
    
    private void Die()
    {
        Debug.Log("Player has died.");
    }
}
