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

    [Tooltip("입장 후 클리어할 때까지 문을 잠글 방인지")]
    [SerializeField] private bool requiresClearToExit = true;

    [Tooltip("게임 시작부터 클리어된 방인지")]
    [SerializeField] private bool startCleared = false;


    [Header("Door Blockers")]
    [SerializeField] private GameObject[] doorBlockers;


    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;


    private BoxCollider2D roomTrigger;

    private RoomState state;


    // 현재 플레이어가 위치한 방
    public static RoomSystem CurrentRoom { get; private set; }


    // 다른 시스템이 현재 방 변경을 감지할 때 사용
    public static event Action<RoomSystem> CurrentRoomChanged;


    // 이 방에 플레이어가 들어왔을 때 발생
    public event Action<RoomSystem> Entered;


    public string RoomId => roomId;

    public RoomType Type => roomType;

    public RoomState State => state;

    public bool IsCleared => state == RoomState.Cleared;


    // 플레이어가 이 방에 실제로 한 번이라도 들어왔는지
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
        }
        else
        {
            state = RoomState.Unvisited;
        }


        HasBeenEntered = false;


        // 처음에는 문이 열려 있음
        SetDoorsLocked(false);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject actor = GetActor(other);


        if (!actor.CompareTag("Player"))
        {
            return;
        }


        // 현재 방 저장
        CurrentRoom = this;

        CurrentRoomChanged?.Invoke(this);


        // 처음 진입한 순간 방 공개 상태로 변경
        if (!HasBeenEntered)
        {
            HasBeenEntered = true;

            Entered?.Invoke(this);
        }


        // 이미 클리어한 방
        if (state == RoomState.Cleared)
        {
            SetDoorsLocked(false);
        }

        // 전투방 등 클리어가 필요한 방
        else if (requiresClearToExit)
        {
            state = RoomState.Active;

            SetDoorsLocked(true);
        }

        // 시작방 / 상점 / 회복방 등
        else
        {
            state = RoomState.Cleared;

            SetDoorsLocked(false);
        }


        if (showDebugLog)
        {
            Debug.Log(
                $"[Room Enter] {roomId} / " +
                $"{roomType} / " +
                $"{state} / " +
                $"Entered: {HasBeenEntered}",
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

        SetDoorsLocked(false);


        if (showDebugLog)
        {
            Debug.Log(
                $"[Room Clear] {roomId}",
                this
            );
        }
    }


    private void SetDoorsLocked(bool locked)
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


            door.SetActive(locked);
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