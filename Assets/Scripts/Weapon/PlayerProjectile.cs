using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Hit")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float hitRadius = 0.15f;

    private int damage;
    private float moveSpeed;
    private float destroyTime;

    private void Update()
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime;

        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }

        CheckHit();
    }

    public void Initialize(int damageAmount, float projectileSpeed, float projectileLifeTime)
    {
        damage = damageAmount;
        moveSpeed = projectileSpeed;
        destroyTime = Time.time + projectileLifeTime;
    }

    public void SetTargetLayer(LayerMask newTargetLayer)
    {
        targetLayer = newTargetLayer;
    }

    private void CheckHit()
    {
        Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, hitRadius, targetLayer);
        if (hitCollider == null)
        {
            return;
        }

        MonsterHealth monsterHealth = hitCollider.GetComponent<MonsterHealth>();
        if (monsterHealth != null)
        {
            monsterHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
