using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(WeaponSystem))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2f;                  // Radius for hit detection
    public float attackRadius = 0.8f;               // Sphere radius for detection
    public LayerMask hitMask;                       // What counts as "damageable"

    [Header("Animation")]
    public Animator animator;
    public string lightAttackTrigger = "LightAttack";
    public string heavyAttackTrigger = "HeavyAttack";

    private WeaponSystem weaponSystem;
    private PlayerStats playerStats;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    void Awake()
    {
        weaponSystem = GetComponent<WeaponSystem>();
        playerStats = GetComponent<PlayerStats>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // On cooldown -> can't attack yet
        if (isAttacking && Time.time >= nextAttackTime)
            isAttacking = false;
    }

    // ---------------------
    // Input Methods
    // ---------------------
    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryLightAttack();
    }

    public void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryHeavyAttack();
    }

    // ---------------------
    // Attack Handling
    // ---------------------
    private void TryLightAttack()
    {
        if (isAttacking) return;
        if (!weaponSystem.CanPerformLightAttack()) return;

        float attackSpeed = weaponSystem.runtimeStats.attackSpeed;
        float cooldown = 1f / attackSpeed;

        nextAttackTime = Time.time + cooldown;
        isAttacking = true;

        weaponSystem.ConsumeStaminaForLightAttack();

        if (animator)
            animator.SetTrigger(lightAttackTrigger);
    }

    private void TryHeavyAttack()
    {
        if (isAttacking) return;
        if (!weaponSystem.CanPerformHeavyAttack()) return;

        float attackSpeed = weaponSystem.runtimeStats.attackSpeed * 0.65f; // heavy attacks slightly slower
        float cooldown = 1f / attackSpeed;

        nextAttackTime = Time.time + cooldown;
        isAttacking = true;

        weaponSystem.ConsumeStaminaForHeavyAttack();

        if (animator)
            animator.SetTrigger(heavyAttackTrigger);
    }

    // ---------------------
    // Animation Event Hook
    // ---------------------
    // Call this from animation events on the attack animations
    public void DealDamageEvent()
    {
        DealDamage();
    }

    // ---------------------
    // Damage Application
    // ---------------------
    private void DealDamage()
    {
        float damage = isAttacking
            ? weaponSystem.GetLightAttackDamage()
            : weaponSystem.GetHeavyAttackDamage();

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * attackRange,
            attackRadius,
            hitMask
        );

        foreach (Collider hit in hits)
        {
            //var damageable = hit.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    damageable.TakeDamage(damage);
            //}
        }
    }

    // ---------------------
    // Gizmos for Debugging
    // ---------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * attackRange,
            attackRadius
        );
    }
}
