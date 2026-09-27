using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomPeekTrigger : MonoBehaviour
{
    [Header("Room Fog")]
    [SerializeField] private RoomFogController roomFog;


    private void Reset()
    {
        BoxCollider2D trigger =
            GetComponent<BoxCollider2D>();

        trigger.isTrigger = true;
    }


    private void Awake()
    {
        BoxCollider2D trigger =
            GetComponent<BoxCollider2D>();

        trigger.isTrigger = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject actor = GetActor(other);


        if (!actor.CompareTag("Player"))
        {
            return;
        }


        if (roomFog == null)
        {
            return;
        }


        roomFog.StartPeek(
            actor.transform
        );
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject actor = GetActor(other);


        if (!actor.CompareTag("Player"))
        {
            return;
        }


        if (roomFog == null)
        {
            return;
        }


        roomFog.StopPeek();
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