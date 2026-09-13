using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class TempPlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;


    [Header("Debug")]
    [SerializeField] private bool showDebugHUD = true;


    private Rigidbody2D rb;

    private Vector2 moveInput;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }


    private void Update()
    {
        moveInput = ReadMovementInput();


        // K = 현재 방 강제 클리어
        if (WasClearKeyPressed())
        {
            ClearCurrentRoom();
        }
    }


    private void FixedUpdate()
    {
        Vector2 targetPosition =
            rb.position +
            moveInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);
    }


    private Vector2 ReadMovementInput()
    {
#if ENABLE_INPUT_SYSTEM

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return Vector2.zero;


        float x = 0f;
        float y = 0f;


        if (keyboard.aKey.isPressed ||
            keyboard.leftArrowKey.isPressed)
        {
            x -= 1f;
        }

        if (keyboard.dKey.isPressed ||
            keyboard.rightArrowKey.isPressed)
        {
            x += 1f;
        }

        if (keyboard.sKey.isPressed ||
            keyboard.downArrowKey.isPressed)
        {
            y -= 1f;
        }

        if (keyboard.wKey.isPressed ||
            keyboard.upArrowKey.isPressed)
        {
            y += 1f;
        }


        return new Vector2(x, y).normalized;

#else

        float x = 0f;
        float y = 0f;


        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1f;
        }

        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow))
        {
            y += 1f;
        }


        return new Vector2(x, y).normalized;

#endif
    }


    private bool WasClearKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM

        return Keyboard.current != null &&
               Keyboard.current.kKey.wasPressedThisFrame;

#else

        return Input.GetKeyDown(KeyCode.K);

#endif
    }


    private void ClearCurrentRoom()
    {
        RoomSystem currentRoom = RoomSystem.CurrentRoom;


        if (currentRoom == null)
        {
            Debug.Log("[Test Player] 현재 방이 없습니다.");
            return;
        }


        if (currentRoom.IsCleared)
        {
            Debug.Log(
                $"[Test Player] {currentRoom.RoomId}은 이미 클리어된 방입니다."
            );

            return;
        }


        Debug.Log(
            $"[Test Player] 강제 클리어 : {currentRoom.RoomId}"
        );

        currentRoom.ClearRoom();
    }


    private void OnGUI()
    {
        if (!showDebugHUD)
            return;


        RoomSystem room = RoomSystem.CurrentRoom;


        string roomInfo =
            room == null
                ? "현재 방 : 통로 / 없음"
                : $"현재 방 : {room.RoomId} | {room.Type} | {room.State}";


        GUI.Label(
            new Rect(15, 15, 700, 25),
            roomInfo
        );

        GUI.Label(
            new Rect(15, 40, 700, 25),
            "이동 : WASD / 방향키    현재 방 강제 클리어 : K"
        );
    }
}