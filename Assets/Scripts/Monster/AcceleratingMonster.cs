using UnityEngine;

public class AcceleratingMonster : MonoBehaviour
{
    public MonsterData monsterData;

    public Transform firePoint;

    public int bulletCount = 5;

    public float spreadAngle = 60f;

    public float baseSpeed = 2f;

    public float tier1Distance = 5f;
    public float tier1Speed = 5f;

    public float tier2Distance = 10f;
    public float tier2Speed = 10f;

    public float tier3Distance = 20f;
    public float tier3Speed = 20f;

    public float attackDuration = 3f;

    public float attackCooldown = 3f;

    private Transform playerTransform;

    private float attackTimer = 0f;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[AcceleratingMonster] Player 태그를 가진 오브젝트를 찾지 못했습니다.");
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float attackRange = monsterData != null ? monsterData.attackRange : 1f;

        Debug.Log("[AcceleratingMonster] 거리: " + distance + ", attackRange: " + attackRange);

        if (distance <= attackRange)
        {
            UpdateAttack();
        }
        else
        {
            ApproachPlayer(distance);
        }
    }

    private float GetSpeedForDistance(float distance)
    {
        if (distance >= tier3Distance)
        {
            return tier3Speed;
        }
        if (distance >= tier2Distance)
        {
            return tier2Speed;
        }
        if (distance >= tier1Distance)
        {
            return tier1Speed;
        }
        return baseSpeed;
    }

    private void ApproachPlayer(float distance)
    {
        float speed = GetSpeedForDistance(distance);
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
    }

    private void UpdateAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            FireFan();
        }
    }

    private void FireFan()
    {
        if (monsterData == null || monsterData.projectilePrefab == null)
        {
            Debug.LogWarning("[AcceleratingMonster] 투사체 프리팹이 없습니다.");
            return;
        }

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 baseDirection = (playerTransform.position - origin).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle - spreadAngle / 2f;
        float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            GameObject bullet = Instantiate(monsterData.projectilePrefab, origin, Quaternion.Euler(0f, 0f, currentAngle));

            MonsterProjectile projectile = bullet.GetComponent<MonsterProjectile>();
            if (projectile != null)
            {
                projectile.lifeTime = attackDuration;
                projectile.SetDirection(direction, monsterData.attackPower);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tier1Distance);
        Gizmos.DrawWireSphere(transform.position, tier2Distance);
        Gizmos.DrawWireSphere(transform.position, tier3Distance);
    }
}