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

    [SerializeField] private float maxAimOffset = 2.5f;

    [SerializeField] private float mouseDistanceForMaxOffset = 7f;


    [Header("Movement Look Ahead")]
    [SerializeField] private float movementLookAheadDistance = 0.8f;

    [SerializeField] private float movementSpeedForMaxOffset = 6f;


    [Header("Limits")]
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

        Vector3 worldPosition =
            cameraComponent.ScreenToWorldPoint(
                Input.mousePosition
            );


        worldPosition.z = 0f;

        return worldPosition;

#endif
    }


    public void SetAimDirection(Vector2 direction)
    {
        externalAimDirection = direction;

        useExternalAimDirection = true;
    }


    public void ClearExternalAimDirection()
    {
        useExternalAimDirection = false;
    }


    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;


        if (playerTarget != null)
        {
            previousPlayerPosition = playerTarget.position;
        }
    }
}