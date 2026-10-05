using UnityEngine;

public class ChargeMonster : MonoBehaviour
{
    private enum State
    {
        Idle,
        Telegraph,
        Charging,
        Cooldown
    }

    public Scanner scanner;

    public MonsterData monsterData;

    public LayerMask playerLayer;

    public float telegraphDuration = 1f;

    public float chargeDistance = 10f;

    public float chargeCooldown = 3f;

    public float approachSpeed = 3f;

    private Transform playerTransform;

    private State currentState = State.Idle;

    private float stateTimer = 0f;

    private Vector3 chargeEndPosition;

    private float chargeElapsedTime;

    private float chargeMaxDuration;

    private SpriteRenderer spriteRenderer;

    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[ChargeMonster] Player 태그를 가진 오브젝트를 찾지 못했습니다.");
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Telegraph:
                UpdateTelegraph();
                break;
            case State.Charging:
                UpdateCharging();
                break;
            case State.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    private void UpdateIdle()
    {
        if (scanner.nearestTarget != null && scanner.nearestTarget.CompareTag("Player"))
        {
            Debug.Log("[ChargeMonster] 감지됨 -> Telegraph 진입");
            currentState = State.Telegraph;
            stateTimer = 0f;
            return;
        }

        ApproachPlayer();
    }

    private void ApproachPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, approachSpeed * Time.deltaTime);
    }

    private void UpdateTelegraph()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= telegraphDuration)
        {
            LockChargeTarget();
            Debug.Log("[ChargeMonster] Charging 진입, 목표 좌표: " + chargeEndPosition + ", 내 위치: " + transform.position);
            currentState = State.Charging;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
        }
    }

    private void LockChargeTarget()
    {
        Vector3 targetPosition = scanner.nearestTarget != null ? scanner.nearestTarget.position : transform.position;

        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float actualDistance = Mathf.Min(chargeDistance, distanceToTarget);

        chargeEndPosition = transform.position + direction * actualDistance;

        float speed = monsterData != null ? monsterData.moveSpeed : 5f;
        chargeMaxDuration = speed > 0f ? (actualDistance / speed) + 0.2f : 1f;
        chargeElapsedTime = 0f;
    }

    private void UpdateCharging()
    {
        float speed = monsterData != null ? monsterData.moveSpeed : 5f;

        transform.position = Vector3.MoveTowards(transform.position, chargeEndPosition, speed * Time.deltaTime);

        chargeElapsedTime += Time.deltaTime;

        bool reachedByDistance = Vector3.Distance(transform.position, chargeEndPosition) < 0.2f;
        bool reachedByTimeout = chargeElapsedTime >= chargeMaxDuration;

        if (reachedByDistance || reachedByTimeout)
        {
            PerformAttack();
            currentState = State.Cooldown;
            stateTimer = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    private void PerformAttack()
    {
        float attackRange = monsterData != null ? monsterData.attackRange : 1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                int damage = monsterData != null ? Mathf.RoundToInt(monsterData.attackPower) : 1;

                try
                {
                    playerHealth.TakeDamage(damage);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[ChargeMonster] TakeDamage 처리 중 에러 발생: " + e.Message);
                }
            }
        }
    }

    private void UpdateCooldown()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= chargeCooldown)
        {
            currentState = State.Idle;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chargeDistance);
    }
}