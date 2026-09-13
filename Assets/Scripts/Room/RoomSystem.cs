using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class RoomSystem : MonoBehaviour
{
    public enum RoomType
    {
        Start,
        Combat,
        Shop,
        Recovery
    }

    public enum RoomState
    {
        Unvisited,
        Active,
        Cleared
    }

    [Header("Room Info")]
    [SerializeField] private string roomId = "Room_01";
    [SerializeField] private RoomType roomType = RoomType.Combat;

    [Tooltip("체크하면 첫 입장 시 문이 잠기고 ClearRoom() 호출 전까지 나갈 수 없습니다.")]
    [SerializeField] private bool lockUntilCleared = true;

    [Tooltip("시작부터 클리어 처리할 방입니다. 시작 방에 사용하세요.")]
    [SerializeField] private bool startCleared = false;


    [Header("Doors")]
    [Tooltip("문 오브젝트들. Active = 닫힘 / Inactive = 열림으로 사용합니다.")]
    [SerializeField] private GameObject[] doors;


    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;


    public static RoomSystem CurrentRoom { get; private set; }

    public string RoomId => roomId;
    public RoomType Type => roomType;
    public RoomState State => state;

    public bool IsCleared => state == RoomState.Cleared;
    public bool IsActive => state == RoomState.Active;


    public event Action<RoomSystem> Entered;
    public event Action<RoomSystem> Cleared;


    private RoomState state;
    private BoxCollider2D roomTrigger;


    // Domain Reload를 꺼놓은 에디터 환경에서도
    // 이전 플레이의 CurrentRoom 값이 남지 않게 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        CurrentRoom = null;
    }


    private void Reset()
    {
        roomTrigger = GetComponent<BoxCollider2D>();
        roomTrigger.isTrigger = true;
    }


    private void Awake()
    {
        roomTrigger = GetComponent<BoxCollider2D>();
        roomTrigger.isTrigger = true;

        state = startCleared
            ? RoomState.Cleared
            : RoomState.Unvisited;

        // 아직 들어가지 않은 방은 입장할 수 있어야 하므로
        // 모든 문은 처음에는 열어둡니다.
        SetDoorsLocked(false);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject actor =
            other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

        if (!actor.CompareTag("Player"))
            return;


        // 매 프레임 검사하지 않고
        // 방 Trigger에 들어온 순간에만 현재 방 갱신
        CurrentRoom = this;


        // 이미 클리어한 방
        if (state == RoomState.Cleared)
        {
            SetDoorsLocked(false);
        }

        // 클리어가 필요한 방
        else if (lockUntilCleared)
        {
            state = RoomState.Active;

            SetDoorsLocked(true);
        }

        // 상점 / 회복방 등 잠글 필요 없는 방
        else
        {
            ClearRoom();
        }


        Entered?.Invoke(this);


        if (logStateChanges)
        {
            Debug.Log(
                $"[Room Enter] {roomId} / {roomType} / {state}",
                this
            );
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject actor =
            other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

        if (!actor.CompareTag("Player"))
            return;


        if (CurrentRoom == this)
        {
            CurrentRoom = null;

            if (logStateChanges)
            {
                Debug.Log(
                    $"[Room Exit] {roomId}",
                    this
                );
            }
        }
    }


    /// <summary>
    /// 현재 방의 클리어 조건이 달성됐을 때 호출.
    ///
    /// 지금은 테스트 플레이어가 K키로 호출하고,
    /// 나중에는 적 관리 시스템이 마지막 적 사망 시 호출하면 됩니다.
    /// </summary>
    public void ClearRoom()
    {
        if (state == RoomState.Cleared)
            return;


        state = RoomState.Cleared;

        SetDoorsLocked(false);

        Cleared?.Invoke(this);


        if (logStateChanges)
        {
            Debug.Log(
                $"[Room Clear] {roomId}",
                this
            );
        }
    }


    private void SetDoorsLocked(bool locked)
    {
        if (doors == null)
            return;


        foreach (GameObject door in doors)
        {
            if (door == null)
                continue;

            // 문 오브젝트가 활성화되어 있으면 닫힌 문.
            // 비활성화되어 있으면 열린 문.
            door.SetActive(locked);
        }
    }
}