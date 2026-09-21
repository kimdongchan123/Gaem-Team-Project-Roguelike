using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;

    public LayerMask targetLayer;

    public Collider2D[] targets;

    public Transform nearestTarget;

    private void FixedUpdate()
    {
        targets = Physics2D.OverlapCircleAll(
            transform.position,
            scanRange,
            targetLayer
        );

        nearestTarget = GetNearest();
    }

    private Transform GetNearest()
    {
        Transform result = null;
        float diff = Mathf.Infinity;

        foreach (Collider2D target in targets)
        {
            if (target == null)
            {
                continue;
            }

            float positionDiff = Vector2.Distance(
                transform.position,
                target.transform.position
            );

            if (positionDiff < diff)
            {
                diff = positionDiff;
                result = target.transform;
            }
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            scanRange
        );
    }
}
