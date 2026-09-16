using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDelay = 1f;
    [SerializeField] private LayerMask playerLayer;

    private float nextAttackTime;

    public void SetTestAttackData(int newDamage, float newAttackRange, float newAttackDelay, LayerMask newPlayerLayer)
    {
        damage = newDamage;
        attackRange = newAttackRange;
        attackDelay = newAttackDelay;
        playerLayer = newPlayerLayer;
    }

    private void Update()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (playerCollider == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackDelay;
        playerHealth.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
