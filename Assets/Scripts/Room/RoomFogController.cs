using UnityEngine;

public class RoomFogController : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private RoomSystem roomSystem;


    [Header("Fog")]
    [SerializeField] private SpriteRenderer fogRenderer;


    [Header("Peek Mask")]
    [SerializeField] private SpriteMask peekMask;

    [SerializeField] private float peekDistance = 7f;

    [Range(10f, 120f)]
    [SerializeField] private float peekAngle = 55f;

    [SerializeField] private int maskTextureSize = 256;


    private Sprite runtimeMaskSprite;

    private Transform peekTarget;


    private void Awake()
    {
        CreatePeekMaskSprite();

        SetPeekActive(false);
    }


    private void OnEnable()
    {
        if (roomSystem != null)
        {
            roomSystem.Entered += HandleRoomEntered;
        }
    }


    private void OnDisable()
    {
        if (roomSystem != null)
        {
            roomSystem.Entered -= HandleRoomEntered;
        }
    }


    private void Start()
    {
        if (roomSystem == null)
        {
            Debug.LogError(
                "[RoomFogController] RoomSystem이 연결되지 않았습니다.",
                this
            );

            return;
        }


        if (roomSystem.HasBeenEntered)
        {
            RevealRoom();
        }
        else
        {
            HideRoom();
        }
    }


    private void Update()
    {
        if (peekTarget == null)
        {
            return;
        }


        if (peekMask == null)
        {
            return;
        }


        UpdatePeekMaskTransform();
    }


    public void StartPeek(Transform player)
    {
        if (roomSystem != null &&
            roomSystem.HasBeenEntered)
        {
            return;
        }


        peekTarget = player;

        SetPeekActive(true);

        UpdatePeekMaskTransform();
    }


    public void StopPeek()
    {
        peekTarget = null;

        SetPeekActive(false);
    }


    private void HandleRoomEntered(RoomSystem room)
    {
        RevealRoom();
    }


    private void RevealRoom()
    {
        if (fogRenderer != null)
        {
            fogRenderer.enabled = false;
        }


        SetPeekActive(false);

        peekTarget = null;
    }


    private void HideRoom()
    {
        if (fogRenderer != null)
        {
            fogRenderer.enabled = true;
        }
    }


    private void SetPeekActive(bool isActive)
    {
        if (peekMask != null)
        {
            peekMask.enabled = isActive;
        }
    }


    private void UpdatePeekMaskTransform()
    {
        Vector3 roomCenter = fogRenderer.bounds.center;


        Vector2 direction =
            roomCenter -
            peekTarget.position;


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) *
            Mathf.Rad2Deg;


        peekMask.transform.position =
            peekTarget.position;


        peekMask.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        peekMask.transform.localScale =
            new Vector3(
                peekDistance,
                peekDistance,
                1f
            );
    }


    private void CreatePeekMaskSprite()
    {
        if (peekMask == null)
        {
            return;
        }


        int size = Mathf.Max(
            32,
            maskTextureSize
        );


        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );


        texture.filterMode = FilterMode.Bilinear;

        texture.wrapMode = TextureWrapMode.Clamp;


        Color clearColor =
            new Color(0f, 0f, 0f, 0f);

        Color visibleColor =
            Color.white;


        Vector2 origin =
            new Vector2(
                0f,
                size * 0.5f
            );


        float halfAngle =
            peekAngle * 0.5f;


        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point =
                    new Vector2(x, y);


                Vector2 direction =
                    point - origin;


                float distance =
                    direction.magnitude;


                if (distance <= 0.01f)
                {
                    texture.SetPixel(
                        x,
                        y,
                        visibleColor
                    );

                    continue;
                }


                float angle =
                    Vector2.Angle(
                        Vector2.right,
                        direction
                    );


                bool insideDistance =
                    distance <= size;

                bool insideAngle =
                    angle <= halfAngle;


                texture.SetPixel(
                    x,
                    y,
                    insideDistance &&
                    insideAngle
                        ? visibleColor
                        : clearColor
                );
            }
        }


        texture.Apply();


        runtimeMaskSprite = Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
                size,
                size
            ),
            new Vector2(
                0f,
                0.5f
            ),
            size
        );


        peekMask.sprite =
            runtimeMaskSprite;
    }


    private void OnDestroy()
    {
        if (runtimeMaskSprite != null)
        {
            Destroy(runtimeMaskSprite.texture);

            Destroy(runtimeMaskSprite);
        }
    }
}