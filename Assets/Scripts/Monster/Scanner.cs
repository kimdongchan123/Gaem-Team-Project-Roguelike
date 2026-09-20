using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;

    public LayerMask targetLayer;

    public RaycastHit2D[] targets;

    public Transform nearestTarget;

    void FixedUpdate()
    {
        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        
        nearestTarget = GetNearest();
    }

    private Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;
        foreach(var tmp in targets)
        {
            Vector3 myPosition = transform.position;
            Vector3 targetPosition = tmp.transform.position;
            float positionDiff = Vector3.Distance(myPosition, targetPosition);

            if(positionDiff < diff)
            {
                diff = positionDiff;
                result = tmp.transform;
            }
        }
        return result;
    }
}
