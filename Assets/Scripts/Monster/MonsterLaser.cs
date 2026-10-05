using UnityEngine;

public class MonsterLaser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Scanner scanner;

    [Header("Laser")]
    [SerializeField] private float aimDuration = 2f;
    [SerializeField] private float laserDuration = 0.15f;
    [SerializeField] private float cooldown = 3f;

    // 맵을 뚫고 나갈 정도의 레이저 길이
    [SerializeField] private float laserDistance = 100f;

    [SerializeField] private float laserWidth = 0.15f;

    [Header("Aim")]
    [SerializeField] private float aimLineWidth = 0.05f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerLayer;

    [Header("Approach")]
    [SerializeField] private float approachSpeed = 3f;

    private Transform playerTransform;

    private float timer;

    // 감지 당시 플레이어 위치
    private Vector2 targetPosition;

    // 감지 당시 플레이어 방향
    private Vector2 laserDirection;

    private LineRenderer aimLine;

    // 현재 생성된 레이저
    private GameObject currentLaser;

    private enum State
    {
        Searching,
        Aiming,
        Cooldown
    }

    private State currentState = State.Searching;


    private void Awake()
    {
        if (scanner == null)
        {
            scanner = GetComponent<Scanner>();
        }

        CreateAimLine();
    }


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
                "[MonsterLaser] Player 태그를 가진 오브젝트를 찾지 못했습니다."
            );
        }
    }


    private void Update()
    {
        if (scanner == null)
        {
            return;
        }

        switch (currentState)
        {
            case State.Searching:
                SearchTarget();
                break;

            case State.Aiming:
                AimTarget();
                break;

            case State.Cooldown:
                Cooldown();
                break;
        }
    }


    // =========================================================
    // 플레이어 탐색
    // =========================================================

    private void SearchTarget()
    {
        if (scanner.nearestTarget != null &&
            scanner.nearestTarget.CompareTag("Player"))
        {
            // ★ 플레이어를 발견한 순간의 위치 저장
            targetPosition = scanner.nearestTarget.position;

            // ★ 발견 순간의 방향도 저장
            laserDirection =
                (targetPosition - (Vector2)transform.position).normalized;

            // 조준 시작
            timer = 0f;

            currentState = State.Aiming;

            UpdateAimLine();

            return;
        }

        // 플레이어가 발견되지 않았다면 접근
        ApproachPlayer();
    }


    // =========================================================
    // 조준
    // =========================================================

    private void AimTarget()
    {
        timer += Time.deltaTime;

        // ★ 플레이어가 움직여도 targetPosition은 변하지 않음
        UpdateAimLine();

        if (timer >= aimDuration)
        {
            FireLaser();

            timer = 0f;

            currentState = State.Cooldown;

            HideAimLine();
        }
    }


    // =========================================================
    // 쿨다운
    // =========================================================

    private void Cooldown()
    {
        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            timer = 0f;

            currentState = State.Searching;
        }
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
    // 조준선 생성
    // =========================================================

    private void CreateAimLine()
    {
        GameObject aimObject =
            new GameObject(
                "LaserAimLine_" + gameObject.name
            );

        aimLine =
            aimObject.AddComponent<LineRenderer>();

        aimLine.positionCount = 2;

        aimLine.startWidth = aimLineWidth;
        aimLine.endWidth = aimLineWidth;

        aimLine.material =
            CreateMaterial(
                new Color(1f, 0.2f, 0.2f, 0.5f)
            );

        aimLine.enabled = false;
    }


    // =========================================================
    // 조준선
    // =========================================================

    private void UpdateAimLine()
    {
        if (aimLine == null)
        {
            return;
        }

        Vector2 startPosition = transform.position;

        // ★ 감지 당시 위치까지 조준
        aimLine.SetPosition(
            0,
            startPosition
        );

        aimLine.SetPosition(
            1,
            targetPosition
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


    // =========================================================
    // 레이저 발사
    // =========================================================

    private void FireLaser()
    {
        Vector2 startPosition = transform.position;

        /*
         * ★ 중요한 부분
         *
         * 플레이어가 움직였더라도
         * 처음 감지했던 방향 그대로 발사한다.
         */

        Vector2 endPosition =
            startPosition +
            laserDirection * laserDistance;


        Debug.Log(
            "[" + gameObject.name +
            "] 레이저 발사"
        );


        // =====================================================
        // 혹시 기존 레이저가 남아있다면 제거
        // =====================================================

        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }


        // =====================================================
        // 새 레이저 생성
        // =====================================================

        CreateLaserVisual(
            startPosition,
            endPosition
        );


        // =====================================================
        // 데미지
        // =====================================================

        CheckDamage();
    }


    // =========================================================
    // 레이저 생성
    // =========================================================

    private void CreateLaserVisual(
        Vector2 startPosition,
        Vector2 endPosition)
    {
        currentLaser =
            new GameObject(
                "Laser_" + gameObject.name
            );

        LineRenderer laser =
            currentLaser.AddComponent<LineRenderer>();

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

        laser.material =
            CreateMaterial(Color.red);


        // 레이저가 일정 시간 후 사라짐
        Destroy(
            currentLaser,
            laserDuration
        );

        // 참조 제거
        Invoke(nameof(ClearLaserReference), laserDuration);
    }


    private void ClearLaserReference()
    {
        currentLaser = null;
    }


    // =========================================================
    // 데미지 판정
    // =========================================================

    private void CheckDamage()
    {
        /*
         * 레이저가 처음 조준했던 위치에
         * 플레이어가 아직 있다면 데미지
         */

        Collider2D hit =
            Physics2D.OverlapCircle(
                targetPosition,
                laserWidth,
                playerLayer
            );

        if (hit == null)
        {
            return;
        }

        PlayerHealth playerHealth =
            hit.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }


    // =========================================================
    // Material
    // =========================================================

    private Material CreateMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Sprites/Default");

        Material material =
            new Material(shader);

        material.color = color;

        return material;
    }


    // =========================================================
    // Destroy
    // =========================================================

    private void OnDestroy()
    {
        if (aimLine != null)
        {
            Destroy(aimLine.gameObject);
        }

        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }
    }
}