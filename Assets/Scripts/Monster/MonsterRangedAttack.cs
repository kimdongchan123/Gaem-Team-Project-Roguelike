using UnityEngine;

public class MonsterRangedAttack : MonoBehaviour
{
    [Header("References")]
    public Scanner scanner;
    public MonsterData monsterData;
    public Transform firePoint;

    [Header("Fan Attack")]
    public int bulletCount = 5;
    public float spreadAngle = 60f;

    // 부채꼴을 몇 초마다 한 번씩 발사할지
    public float fireInterval = 0.2f;

    // 총을 쏘는 시간
    public float attackDuration = 3f;

    // 공격이 끝난 후 쉬는 시간
    public float attackCooldown = 3f;

    [Header("Movement")]
    public float approachSpeed = 3f;

    private Transform playerTransform;

    // 현재 공격을 시작한 후 흐른 시간
    private float attackTimer = 0f;

    // 마지막으로 총을 쏜 후 흐른 시간
    private float fireTimer = 0f;

    private bool isAttacking = false;
    private bool isCooldown = false;


    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning(
                "[MonsterRangedAttack] Player 태그를 가진 오브젝트를 찾지 못했습니다."
            );
        }
    }


    private void Update()
    {
        if (scanner == null)
        {
            return;
        }


        // =====================================================
        // 공격 중
        // =====================================================

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            fireTimer += Time.deltaTime;


            // 일정 간격마다 부채꼴 발사
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;

                if (scanner.nearestTarget != null &&
                    scanner.nearestTarget.CompareTag("Player"))
                {
                    FireFan(scanner.nearestTarget);
                }
            }


            // 공격 시간이 끝남
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                isCooldown = true;

                attackTimer = 0f;
                fireTimer = 0f;
            }

            return;
        }


        // =====================================================
        // 쿨타임 중
        // =====================================================

        if (isCooldown)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                isCooldown = false;

                attackTimer = 0f;
                fireTimer = 0f;
            }

            return;
        }


        // =====================================================
        // 플레이어 탐색
        // =====================================================

        if (scanner.nearestTarget == null ||
            !scanner.nearestTarget.CompareTag("Player"))
        {
            ApproachPlayer();
            return;
        }


        // =====================================================
        // 플레이어 발견 → 공격 시작
        // =====================================================

        isAttacking = true;

        attackTimer = 0f;
        fireTimer = 0f;

        // 발견하자마자 첫 발을 쏘고 싶다면
        FireFan(scanner.nearestTarget);
    }


    // =========================================================
    // 플레이어에게 접근
    // =========================================================

    private void ApproachPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                approachSpeed * Time.deltaTime
            );
    }


    // =========================================================
    // 부채꼴 발사
    // =========================================================

    private void FireFan(Transform target)
    {
        if (monsterData == null ||
            monsterData.projectilePrefab == null)
        {
            Debug.LogWarning(
                "[MonsterRangedAttack] 투사체 프리팹이 없습니다."
            );

            return;
        }


        Vector3 origin =
            firePoint != null
            ? firePoint.position
            : transform.position;


        // 현재 플레이어 방향
        Vector2 baseDirection =
            (target.position - origin).normalized;


        float baseAngle =
            Mathf.Atan2(
                baseDirection.y,
                baseDirection.x
            ) * Mathf.Rad2Deg;


        // 부채꼴의 시작 각도
        float startAngle =
            baseAngle - spreadAngle / 2f;


        // 총알 사이의 각도
        float angleStep =
            bulletCount > 1
            ? spreadAngle / (bulletCount - 1)
            : 0f;


        // =====================================================
        // 총알 생성
        // =====================================================

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle =
                startAngle + angleStep * i;


            float radian =
                currentAngle * Mathf.Deg2Rad;


            Vector2 direction =
                new Vector2(
                    Mathf.Cos(radian),
                    Mathf.Sin(radian)
                );


            GameObject bullet =
                Instantiate(
                    monsterData.projectilePrefab,
                    origin,
                    Quaternion.Euler(
                        0f,
                        0f,
                        currentAngle
                    )
                );


            MonsterProjectile projectile =
                bullet.GetComponent<MonsterProjectile>();


            if (projectile != null)
            {
                projectile.SetDirection(
                    direction,
                    monsterData.attackPower
                );
            }
        }
    }
}