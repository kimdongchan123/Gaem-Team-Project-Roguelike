using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Hit")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.15f;

    private int damage;
    private float moveSpeed;
    private float destroyTime;
    private float explosionRadius;
    private float effectDuration;
    private float nextAreaDamageTime;
    private Vector2 targetPosition;
    private WeaponAttackType attackType;
    private PlayerHealth ownerHealth;
    private bool hasTargetPosition;
    private bool isInitialized;

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (attackType == WeaponAttackType.BlackHole)
        {
            UpdateAreaEffect();
            return;
        }

        if (attackType == WeaponAttackType.HomingProjectile)
        {
            RotateToNearestTarget();
        }

        transform.position += transform.right * moveSpeed * Time.deltaTime;

        if (hasTargetPosition && Vector2.Distance(transform.position, targetPosition) <= hitRadius)
        {
            Explode();
            return;
        }

        if (Time.time >= destroyTime)
        {
            if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
            {
                Explode();
                return;
            }

            Destroy(gameObject);
            return;
        }

        CheckHit();
    }

    public void Initialize(int damageAmount, float projectileSpeed, float projectileLifeTime)
    {
        damage = damageAmount;
        moveSpeed = projectileSpeed;
        destroyTime = Time.time + projectileLifeTime;
        attackType = WeaponAttackType.StraightProjectile;
        explosionRadius = 0f;
        effectDuration = 0f;
        hasTargetPosition = false;
        isInitialized = true;
    }

    public void Initialize(WeaponData weapon, int damageAmount, Vector2 newTargetPosition, PlayerHealth newOwnerHealth)
    {
        damage = damageAmount;
        moveSpeed = weapon.ProjectileSpeed;
        destroyTime = Time.time + weapon.ProjectileLifeTime;
        explosionRadius = weapon.ExplosionRadius;
        effectDuration = weapon.EffectDuration;
        targetPosition = newTargetPosition;
        attackType = weapon.AttackType;
        ownerHealth = newOwnerHealth;
        hasTargetPosition = attackType == WeaponAttackType.TargetExplosion || attackType == WeaponAttackType.BlackHole;
        isInitialized = true;

        if (attackType == WeaponAttackType.BlackHole)
        {
            transform.position = newTargetPosition;
            moveSpeed = 0f;
            destroyTime = Time.time + effectDuration;
            nextAreaDamageTime = Time.time;
            transform.localScale = Vector3.one * Mathf.Max(explosionRadius, 0.5f);
            return;
        }

        if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
        {
            transform.localScale = Vector3.one * 0.35f;
            return;
        }
    }

    public void SetTargetLayer(LayerMask newTargetLayer)
    {
        targetLayer = newTargetLayer;
    }

    public void SetPlayerLayer(LayerMask newPlayerLayer)
    {
        playerLayer = newPlayerLayer;
    }

    private void CheckHit()
    {
        Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, hitRadius, targetLayer);
        if (hitCollider == null)
        {
            return;
        }

        if (attackType == WeaponAttackType.ExplosiveProjectile || attackType == WeaponAttackType.TargetExplosion)
        {
            Explode();
            return;
        }

        MonsterHealth monsterHealth = hitCollider.GetComponent<MonsterHealth>();
        if (monsterHealth != null)
        {
            monsterHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        DamageMonstersInArea();
        DamagePlayerInArea();
        Destroy(gameObject);
    }

    private void UpdateAreaEffect()
    {
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time < nextAreaDamageTime)
        {
            return;
        }

        nextAreaDamageTime = Time.time + 0.35f;
        DamageMonstersInArea();
    }

    private void DamageMonstersInArea()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayer);
        foreach (Collider2D hitCollider in hitColliders)
        {
            MonsterHealth monsterHealth = hitCollider.GetComponent<MonsterHealth>();
            if (monsterHealth != null)
            {
                monsterHealth.TakeDamage(damage);
            }
        }
    }

    private void DamagePlayerInArea()
    {
        if (ownerHealth == null)
        {
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (playerCollider == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private void RotateToNearestTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 6f, targetLayer);
        Transform nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hitCollider in hitColliders)
        {
            float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = hitCollider.transform;
            }
        }

        if (nearestTarget == null)
        {
            return;
        }

        Vector2 direction = nearestTarget.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
