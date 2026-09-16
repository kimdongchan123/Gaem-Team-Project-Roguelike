using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float aimAngleOffset;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 5f;

    private PlayerHealth playerHealth;
    private Vector2 moveInput;
    private Vector2 dashDirection;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            moveInput = Vector2.zero;
            return;
        }

        ReadMoveInput();
        LookAtMouse();
        TryStartDash();
        UpdateDashState();
        MovePlayer();
    }

    private void ReadMoveInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            verticalInput -= 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            verticalInput += 1f;
        }

        moveInput = new Vector2(horizontalInput, verticalInput).normalized;
    }

    private void LookAtMouse()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 lookDirection = mouseWorldPosition - transform.position;

        if (lookDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + aimAngleOffset);
    }

    private void TryStartDash()
    {
        if (Keyboard.current == null || !Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            return;
        }

        if (Time.time < nextDashTime || moveInput == Vector2.zero)
        {
            return;
        }

        isDashing = true;
        dashDirection = moveInput;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
    }

    private void UpdateDashState()
    {
        if (!isDashing || Time.time < dashEndTime)
        {
            return;
        }

        isDashing = false;
    }

    private void MovePlayer()
    {
        Vector2 moveDirection = isDashing ? dashDirection : moveInput;
        float currentMoveSpeed = isDashing ? dashSpeed : moveSpeed;

        transform.position += (Vector3)(moveDirection * currentMoveSpeed * Time.deltaTime);
    }
}
