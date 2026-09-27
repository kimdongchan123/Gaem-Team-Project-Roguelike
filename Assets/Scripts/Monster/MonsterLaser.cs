using UnityEngine;

public class MonsterLaser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Scanner scanner;

    [Header("Laser")]
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float laserDuration = 0.15f;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float laserWidth = 0.15f;

    [Header("Aim")]
    [SerializeField] private float aimLineWidth = 0.05f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerLayer;

    private float fireTimer;

    // 레이저를 발사할 때 사용할 고정된 위치
    private Vector2 targetPosition;

    // 현재 공격 사이클에서 목표 위치가 저장되었는지
    private bool hasTarget;

    // 조준선
    private LineRenderer aimLine;

    private void Awake()
    {
        if (scanner == null)
        {
            scanner = GetComponent<Scanner>();
        }

        CreateAimLine();
    }

    private void Update()
    {
        if (scanner == null)
        {
            return;
        }

        // -----------------------------------------
        // 목표가 없다면 Scanner에서 새로운 위치를 가져옴
        // -----------------------------------------

        if (!hasTarget)
        {
            if (scanner.nearestTarget != null)
            {
                // ⭐ 이 순간의 위치만 저장
                targetPosition =
                    scanner.nearestTarget.position;

                hasTarget = true;

                fireTimer = 0f;

                // 조준선 표시
                UpdateAimLine();
            }

            return;
        }

        // -----------------------------------------
        // 목표 위치를 잡은 상태
        // -----------------------------------------

        fireTimer += Time.deltaTime;

        // 조준선은 계속 같은 위치를 바라봄
        UpdateAimLine();

        // -----------------------------------------
        // 2초가 지나면 발사
        // -----------------------------------------

        if (fireTimer >= fireInterval)
        {
            FireLaser();

            fireTimer = 0f;

            // 다음 목표를 다시 찾음
            hasTarget = false;

            HideAimLine();
        }
    }

    private void CreateAimLine()
    {
        GameObject aimObject =
            new GameObject("LaserAimLine");

        aimLine =
            aimObject.AddComponent<LineRenderer>();

        aimLine.positionCount = 2;

        aimLine.startWidth = aimLineWidth;
        aimLine.endWidth = aimLineWidth;

        aimLine.material = CreateMaterial(
            new Color(1f, 0.2f, 0.2f, 0.5f)
        );

        aimLine.enabled = false;
    }

    private void UpdateAimLine()
    {
        if (aimLine == null)
        {
            return;
        }

        Vector2 startPosition =
            transform.position;

        Vector2 direction =
            (targetPosition - startPosition).normalized;

        Vector2 endPosition =
            startPosition +
            direction * maxDistance;

        aimLine.SetPosition(
            0,
            startPosition
        );

        aimLine.SetPosition(
            1,
            endPosition
        );

        aimLine.enabled = true;
    }

    private void HideAimLine()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    private void FireLaser()
    {
        Vector2 startPosition =
            transform.position;

        // ⭐ 저장해둔 위치를 향함
        Vector2 direction =
            (targetPosition - startPosition).normalized;

        Vector2 endPosition =
            startPosition +
            direction * maxDistance;

        // -----------------------------------------
        // 플레이어가 레이저 선상에 있는지 검사
        // -----------------------------------------

        RaycastHit2D hit =
            Physics2D.Raycast(
                startPosition,
                direction,
                maxDistance,
                playerLayer
            );

        if (hit.collider != null)
        {
            PlayerHealth playerHealth =
                hit.collider.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            endPosition = hit.point;
        }

        // 실제 레이저 표시
        CreateLaserVisual(
            startPosition,
            endPosition
        );
    }

    private void CreateLaserVisual(
        Vector2 startPosition,
        Vector2 endPosition)
    {
        GameObject laserObject =
            new GameObject("Laser");

        LineRenderer laser =
            laserObject.AddComponent<LineRenderer>();

        laser.positionCount = 2;

        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth;

        laser.SetPosition(
            0,
            startPosition
        );

        laser.SetPosition(
            1,
            endPosition
        );

        laser.material = CreateMaterial(
            Color.red
        );

        Destroy(
            laserObject,
            laserDuration
        );
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Sprites/Default");

        Material material =
            new Material(shader);

        material.color = color;

        return material;
    }

    private void OnDestroy()
    {
        if (aimLine != null)
        {
            Destroy(aimLine.gameObject);
        }
    }
}
