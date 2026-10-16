using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Hit")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.15f;

    private float damage;
    private float moveSpeed;
    private float remainingLifetime;
    private float explosionRadius;
    private float areaDamageInterval;
    private float homingTurnSpeed;
    private float nextAreaDamageTime;
    private Vector2 targetPosition;
    private WeaponAttackType attackType;
    private PlayerHealth ownerHealth;
    private bool isInitialized;
    private bool hasResolved;
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();
    private readonly HashSet<MonsterHealth> damagedTargets = new HashSet<MonsterHealth>();

    private void Update()
    {
        if (!isInitialized || hasResolved || Time.timeScale <= 0f) return;
        if (attackType == WeaponAttackType.BlackHole)
        {
            UpdateAreaEffect();
            return;
        }

        if (remainingLifetime <= 0f)
        {
            Expire();
            return;
        }

        float stepTime = Mathf.Min(Time.deltaTime, remainingLifetime);
        if (attackType == WeaponAttackType.HomingProjectile)
        {
            RotateToNearestTarget(stepTime);
        }

        float distance = moveSpeed * stepTime;
        bool reachedTarget = false;
        if (attackType == WeaponAttackType.TargetExplosion)
        {
            Vector2 offset = targetPosition - (Vector2)transform.position;
            reachedTarget = offset.magnitude <= distance;
            distance = Mathf.Min(distance, offset.magnitude);
            if (offset.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
            }
        }

        if (CheckHit(distance)) return;
        transform.position += transform.right * distance;
        remainingLifetime -= stepTime;
        if (reachedTarget)
        {
            transform.position = targetPosition;
            Explode();
        }
        else if (remainingLifetime <= 0f)
        {
            Expire();
        }
    }

    public void Initialize(float damageAmount, float projectileSpeed, float projectileLifeTime)
    {
        damage = Mathf.Max(0f, damageAmount);
        moveSpeed = Mathf.Max(0f, projectileSpeed);
        remainingLifetime = Mathf.Max(0f, projectileLifeTime);
        attackType = WeaponAttackType.StraightProjectile;
        explosionRadius = 0f;
        ownerHealth = null;
        hasResolved = false;
        isInitialized = true;
    }

    public void Initialize(WeaponData weapon, float damageAmount, Vector2 newTargetPosition, PlayerHealth newOwnerHealth)
    {
        Initialize(damageAmount, weapon.ProjectileSpeed, weapon.ProjectileLifeTime);
        attackType = weapon.AttackType;
        explosionRadius = weapon.ExplosionRadius;
        areaDamageInterval = weapon.AreaDamageInterval;
        homingTurnSpeed = weapon.HomingTurnSpeed;
        hitRadius = weapon.HitRadius;
        ownerHealth = newOwnerHealth;
        Vector2 origin = transform.position;
        targetPosition = origin + Vector2.ClampMagnitude(newTargetPosition - origin, weapon.TargetRange);

        if (attackType == WeaponAttackType.BlackHole)
        {
            transform.position = targetPosition;
            moveSpeed = 0f;
            remainingLifetime = weapon.EffectDuration;
            nextAreaDamageTime = Time.time;
            transform.localScale = Vector3.one * explosionRadius * 2f;
        }
        else if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
        {
            transform.localScale = Vector3.one * 0.35f;
        }
    }

    public void SetTargetLayer(LayerMask newTargetLayer) => targetLayer = newTargetLayer;
    public void SetPlayerLayer(LayerMask newPlayerLayer) => playerLayer = newPlayerLayer;

    private ContactFilter2D TargetFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayer);
        filter.useTriggers = true;
        return filter;
    }

    private bool CheckHit(float distance)
    {
        if (hasResolved) return false;
        Physics2D.OverlapCircle(transform.position, hitRadius, TargetFilter(), overlaps);
        foreach (Collider2D overlap in overlaps)
        {
            MonsterHealth target = overlap.GetComponentInParent<MonsterHealth>();
            if (target != null && !target.IsDead)
            {
                ResolveHit(target);
                return true;
            }
        }

        // Sweep the complete travel distance, rather than testing only the end position.
        Physics2D.CircleCast(transform.position, hitRadius, transform.right, TargetFilter(), castHits, distance);
        MonsterHealth nearestTarget = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (RaycastHit2D hit in castHits)
        {
            MonsterHealth target = hit.collider.GetComponentInParent<MonsterHealth>();
            if (target != null && !target.IsDead && hit.distance < nearestDistance)
            {
                nearestTarget = target;
                nearestDistance = hit.distance;
            }
        }

        if (nearestTarget == null) return false;
        transform.position += transform.right * nearestDistance;
        ResolveHit(nearestTarget);
        return true;
    }

    private void ResolveHit(MonsterHealth target)
    {
        if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
        {
            Explode();
            return;
        }
        hasResolved = true;
        target.TakeDamage(damage, ownerHealth != null ? ownerHealth.GetComponent<PlayerRunSession>() : null);
        Destroy(gameObject);
    }

    private void Expire()
    {
        if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
        {
            Explode();
            return;
        }
        hasResolved = true;
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (hasResolved) return;
        hasResolved = true;
        DamageMonstersInArea();
        DamagePlayerInArea();
        Destroy(gameObject);
    }

    private void UpdateAreaEffect()
    {
        if (remainingLifetime <= 0f)
        {
            hasResolved = true;
            Destroy(gameObject);
            return;
        }
        if (Time.time >= nextAreaDamageTime)
        {
            nextAreaDamageTime = Time.time + areaDamageInterval;
            DamageMonstersInArea();
        }
        remainingLifetime -= Time.deltaTime;
    }

    private void DamageMonstersInArea()
    {
        damagedTargets.Clear();
        Physics2D.OverlapCircle(transform.position, explosionRadius, TargetFilter(), overlaps);
        foreach (Collider2D collider in overlaps)
        {
            MonsterHealth target = collider.GetComponentInParent<MonsterHealth>();
            if (target != null && !target.IsDead && damagedTargets.Add(target))
            {
                target.TakeDamage(damage, ownerHealth != null ? ownerHealth.GetComponent<PlayerRunSession>() : null);
            }
        }
    }

    private void DamagePlayerInArea()
    {
        if (ownerHealth == null || ownerHealth.IsDead) return;
        // Preserve explosive self damage, and check all owner colliders rather than an arbitrary player.
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);
        foreach (Collider2D collider in players)
        {
            if (collider.GetComponentInParent<PlayerHealth>() == ownerHealth)
            {
                ownerHealth.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damage)));
                return;
            }
        }
    }

    private void RotateToNearestTarget(float stepTime)
    {
        Physics2D.OverlapCircle(transform.position, 6f, TargetFilter(), overlaps);
        Vector2 nearestPosition = Vector2.zero;
        float nearestDistance = float.PositiveInfinity;
        foreach (Collider2D collider in overlaps)
        {
            MonsterHealth target = collider.GetComponentInParent<MonsterHealth>();
            if (target == null || target.IsDead) continue;
            Vector2 position = collider.bounds.center;
            float distance = ((Vector2)transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = position;
            }
        }
        if (float.IsPositiveInfinity(nearestDistance)) return;
        Vector2 direction = nearestPosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, angle, homingTurnSpeed * stepTime);
        transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
