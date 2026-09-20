using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;


    [Header("Follow")]
    [SerializeField] private float followSmoothTime = 0.18f;


    [Header("Aim Look Ahead")]
    [SerializeField] private bool useMouseLookAhead = true;

    [Tooltip("조준 방향으로 카메라가 이동하는 최대 거리")]
    [SerializeField] private float maxAimOffset = 2.5f;

    [Tooltip("마우스가 플레이어에게서 이 거리만큼 떨어졌을 때 최대 Offset 적용")]
    [SerializeField] private float mouseDistanceForMaxOffset = 7f;


    [Header("Movement Look Ahead")]
    [Tooltip("플레이어 이동 방향으로 카메라가 추가로 이동하는 거리")]
    [SerializeField] private float movementLookAheadDistance = 0.8f;

    [Tooltip("이 정도 속도에서 최대 이동 Offset 적용")]
    [SerializeField] private float movementSpeedForMaxOffset = 6f;


    [Header("Limits")]
    [Tooltip("플레이어와 카메라가 너무 멀어지는 것을 방지")]
    [SerializeField] private float maxTotalOffset = 3f;


    private Camera cameraComponent;

    private Vector3 cameraVelocity;

    private Vector3 previousPlayerPosition;

    private Vector2 externalAimDirection;

    private bool useExternalAimDirection;


    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
    }


    private void Start()
    {
        if (playerTarget == null)
        {
            Debug.LogError(
                "[PlayerCameraController] Player Target이 설정되지 않았습니다.",
                this
            );

            enabled = false;

            return;
        }


        previousPlayerPosition = playerTarget.position;
    }


    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            return;
        }


        Vector2 aimOffset = CalculateAimOffset();

        Vector2 movementOffset = CalculateMovementOffset();


        Vector2 totalOffset = aimOffset + movementOffset;

        totalOffset = Vector2.ClampMagnitude(
            totalOffset,
            maxTotalOffset
        );


        Vector3 targetPosition =
            playerTarget.position +
            new Vector3(
                totalOffset.x,
                totalOffset.y,
                0f
            );


        // 2D 카메라이므로 Z 위치는 기존 카메라 값을 유지합니다.
        targetPosition.z = transform.position.z;


        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref cameraVelocity,
            followSmoothTime
        );


        previousPlayerPosition = playerTarget.position;
    }


    private Vector2 CalculateAimOffset()
    {
        Vector2 aimDirection;
        float strength = 1f;


        if (useExternalAimDirection)
        {
            aimDirection = externalAimDirection.normalized;
        }
        else if (useMouseLookAhead)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();

            Vector2 playerPosition = playerTarget.position;

            Vector2 toMouse =
                (Vector2)mouseWorldPosition -
                playerPosition;


            float distance = toMouse.magnitude;


            if (distance <= 0.01f)
            {
                return Vector2.zero;
            }


            aimDirection = toMouse.normalized;


            strength = Mathf.Clamp01(
                distance /
                mouseDistanceForMaxOffset
            );
        }
        else
        {
            return Vector2.zero;
        }


        return aimDirection *
               maxAimOffset *
               strength;
    }


    private Vector2 CalculateMovementOffset()
    {
        Vector2 currentPosition = playerTarget.position;

        Vector2 movementDelta =
            currentPosition -
            (Vector2)previousPlayerPosition;


        float deltaTime = Mathf.Max(
            Time.deltaTime,
            0.0001f
        );


        Vector2 playerVelocity =
            movementDelta /
            deltaTime;


        float speed = playerVelocity.magnitude;


        if (speed <= 0.01f)
        {
            return Vector2.zero;
        }


        float strength = Mathf.Clamp01(
            speed /
            movementSpeedForMaxOffset
        );


        return playerVelocity.normalized *
               movementLookAheadDistance *
               strength;
    }


    private Vector3 GetMouseWorldPosition()
    {
#if ENABLE_INPUT_SYSTEM

        if (Mouse.current == null)
        {
            return playerTarget.position;
        }


        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();


        Vector3 worldPosition =
            cameraComponent.ScreenToWorldPoint(
                new Vector3(
                    mouseScreenPosition.x,
                    mouseScreenPosition.y,
                    Mathf.Abs(transform.position.z)
                )
            );


        worldPosition.z = 0f;

        return worldPosition;

#else

        Vector3 mouseScreenPosition =
            Input.mousePosition;


        Vector3 worldPosition =
            cameraComponent.ScreenToWorldPoint(
                mouseScreenPosition
            );


        worldPosition.z = 0f;

        return worldPosition;

#endif
    }


    /// <summary>
    /// 실제 플레이어 조준 시스템이 완성된 후 사용할 수 있습니다.
    /// </summary>
    public void SetAimDirection(Vector2 direction)
    {
        externalAimDirection = direction;

        useExternalAimDirection = true;
    }


    /// <summary>
    /// 다시 마우스 기준 조준 카메라로 전환합니다.
    /// </summary>
    public void ClearExternalAimDirection()
    {
        useExternalAimDirection = false;
    }


    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;

        if (playerTarget != null)
        {
            previousPlayerPosition =
                playerTarget.position;
        }
    }
}