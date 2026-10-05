using System;
using UnityEngine;

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

    [Tooltip("방에 들어온 뒤 클리어 전까지 출구를 잠글지")]
    [SerializeField] private bool requiresClearToExit = true;

    [Tooltip("게임 시작부터 클리어된 방인지")]
    [SerializeField] private bool startCleared = false;


    [Header("Door Blockers")]
    [Tooltip("전투 시작 시 활성화되고 클리어 시 비활성화될 문")]
    [SerializeField] private GameObject[] doorBlockers;


    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;


    private BoxCollider2D roomTrigger;

    private RoomState state;


    public static RoomSystem CurrentRoom { get; private set; }

    public static event Action<RoomSystem> CurrentRoomChanged;

    public event Action<RoomSystem> Entered;


    public string RoomId => roomId;

    public RoomType Type => roomType;

    public RoomState State => state;

    public bool IsCleared => state == RoomState.Cleared;

    public bool HasBeenEntered { get; private set; }


    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        CurrentRoom = null;
        CurrentRoomChanged = null;
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


        if (startCleared)
        {
            state = RoomState.Cleared;

            // 시작방처럼 처음부터 공개된 방
            HasBeenEntered = true;
        }
        else
        {
            state = RoomState.Unvisited;

            HasBeenEntered = false;
        }


        // 가장 중요:
        // 게임 시작 시 모든 문은 반드시 열려 있음.
        SetDoorsLocked(false);
    }


    private void Start()
    {
        // 다른 오브젝트의 Awake 실행 순서와 관계없이
        // 미방문 방의 문이 열린 상태임을 한 번 더 보장합니다.
        if (state != RoomState.Active)
        {
            SetDoorsLocked(false);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject actor = GetActor(other);


        if (!actor.CompareTag("Player"))
        {
            return;
        }


        CurrentRoom = this;

        CurrentRoomChanged?.Invoke(this);


        bool isFirstEnter = !HasBeenEntered;


        if (isFirstEnter)
        {
            HasBeenEntered = true;
        }


        // 이미 클리어한 방
        if (state == RoomState.Cleared)
        {
            SetDoorsLocked(false);
        }

        // 클리어가 필요한 방
        else if (requiresClearToExit)
        {
            state = RoomState.Active;

            // 플레이어가 방 안까지 들어온 뒤
            // 출구를 막습니다.
            SetDoorsLocked(true);
        }

        // 전투가 필요 없는 방
        else
        {
            state = RoomState.Cleared;

            SetDoorsLocked(false);
        }


        // 상태 변경과 문 잠금 이후
        // 최초 입장 이벤트 발생
        if (isFirstEnter)
        {
            Entered?.Invoke(this);
        }


        if (showDebugLog)
        {
            Debug.Log(
                $"[Room Enter] " +
                $"{roomId} / " +
                $"{roomType} / " +
                $"{state}",
                this
            );
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject actor = GetActor(other);


        if (!actor.CompareTag("Player"))
        {
            return;
        }


        if (CurrentRoom != this)
        {
            return;
        }


        CurrentRoom = null;

        CurrentRoomChanged?.Invoke(null);


        if (showDebugLog)
        {
            Debug.Log(
                $"[Room Exit] {roomId}",
                this
            );
        }
    }


    public void ClearRoom()
    {
        if (state == RoomState.Cleared)
        {
            return;
        }


        state = RoomState.Cleared;


        // 클리어하면 모든 출구 개방
        SetDoorsLocked(false);


        if (showDebugLog)
        {
            Debug.Log(
                $"[Room Clear] {roomId}",
                this
            );
        }
    }


    private void SetDoorsLocked(bool isLocked)
    {
        if (doorBlockers == null)
        {
            return;
        }


        foreach (GameObject door in doorBlockers)
        {
            if (door == null)
            {
                continue;
            }


            door.SetActive(isLocked);
        }
    }


    private GameObject GetActor(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            return other.attachedRigidbody.gameObject;
        }


        return other.gameObject;
    }
}