using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float aimAngleOffset;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 5f;
    [SerializeField] private Transform aimPivot;
    private PlayerHealth playerHealth;
    private PlayerStats stats;
    private PlayerRunSession session;
    private SpriteRenderer body;
    private Rigidbody2D rigidBody;
    private Vector2 moveInput;
    private Vector2 dashDirection;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    public bool IsDashing => isDashing;
    public float DashRemaining => Mathf.Max(0f,nextDashTime-Time.time);
    public Vector2 AimDirection => aimPivot != null ? (Vector2)aimPivot.right : (Vector2)transform.right;
    public float MoveSpeed => stats != null ? stats.MoveSpeed : moveSpeed;
    public float DashCooldown => stats != null ? stats.DashCooldown : dashCooldown;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>(); stats = GetComponent<PlayerStats>();
        session = GetComponent<PlayerRunSession>(); body = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        if (rigidBody != null)
        {
            rigidBody.gravityScale = 0f;
            rigidBody.freezeRotation = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }
    public void SetAimPivot(Transform pivot) { aimPivot = pivot; }
    public void ResetMovement() { isDashing = false; moveInput = Vector2.zero; nextDashTime = 0f; }
    private void Update()
    {
        if ((playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanMove))
        { moveInput = Vector2.zero; isDashing = false; return; }
        ReadMoveInput(); LookAtMouse(); TryStartDash(); UpdateDashState();
        if (rigidBody == null) MovePlayer();
    }
    public static Vector2 NormalizeMovement(float x, float y) => new Vector2(x,y).normalized;
    private void ReadMoveInput()
    {
        Keyboard keys = Keyboard.current;
        if (keys == null) { moveInput = Vector2.zero; return; }
        moveInput = NormalizeMovement((keys.dKey.isPressed ? 1f:0f)-(keys.aKey.isPressed ? 1f:0f),
            (keys.wKey.isPressed ? 1f:0f)-(keys.sKey.isPressed ? 1f:0f));
    }
    private void LookAtMouse()
    {
        if (Mouse.current != null && Camera.main != null)
            AimAt(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }
    public void AimAt(Vector2 worldPosition)
    {
        Vector2 direction = worldPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude <= 0.0001f) return;
        if (body != null) body.flipX = direction.x < 0f;
        // The body stays upright; only the weapon pivot rotates through 360 degrees.
        float angle = Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg + aimAngleOffset;
        if (aimPivot != null) aimPivot.rotation = Quaternion.Euler(0f,0f,angle);
    }
    private void TryStartDash()
    {
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) TryDash();
    }
    public bool TryDash()
    {
        if (isDashing || Time.time < nextDashTime || (playerHealth != null && playerHealth.IsDead) ||
            (session != null && !session.CanMove)) return false;
        dashDirection = moveInput.sqrMagnitude > 0f ? moveInput : AimDirection.normalized;
        if (dashDirection.sqrMagnitude <= 0f) return false;
        isDashing = true; dashEndTime = Time.time+dashDuration; nextDashTime = Time.time+DashCooldown;
        return true;
    }
    private void UpdateDashState() { if (isDashing && Time.time >= dashEndTime) isDashing = false; }
    private void MovePlayer()
    {
        Vector2 velocity = (isDashing ? dashDirection : moveInput) * (isDashing ? dashSpeed : MoveSpeed);
        if (rigidBody != null) rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
        else transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (rigidBody == null) return;
        if ((playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanMove))
        {
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }
        MovePlayer();
    }
}
