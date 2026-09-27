using UnityEditor.Build.Content;
using UnityEngine;

public class RangeMonster : MonoBehaviour
{
    [SerializeField]
    private Scanner scanner;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private float fireInterval = 5f;
    [SerializeField]
    private float laserDuration = 1.5f;
    [SerializeField]
    private float maxDistance = 100;

    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask obstacleLayer;

    private float fireTimer;

    private void Awake()
    {
        if (scanner == null)
        {
            scanner = GetComponent<Scanner>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (scanner == null)
        {
            return;
        }
        if (scanner.nearestTarget == null)
        {
            fireTimer = 0;
            return;
        }

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0;
            FireLaser();
        }
    }

    private void FireLaser()
    {
        Transform target = scanner.nearestTarget;

        if (target == null)
        {
            return;
        }

        Vector2 startPosition;

        if (firePoint != null)
        {
            startPosition = firePoint.position;
        }
        else
        {
            startPosition = transform.position;
        }
        Vector2 direction = ((Vector2)target.position - startPosition).normalized;
        Vector2 endPosition = startPosition + direction * maxDistance;

        RaycastHit2D obstacleHit = Physics2D.Raycast(
            startPosition,
            direction,
            maxDistance,
            obstacleLayer
            );

        if (obstacleHit.collider != null)
        {
            endPosition = obstacleHit.point;
        }

        RaycastHit2D playerHit = Physics2D.Raycast(
            startPosition,
            direction,
            maxDistance,
            playerLayer
            );

        if (playerHit.collider != null)
        {
            bool playerBeforeObstacle =
                obstacleHit.collider == null ||
                playerHit.distance < obstacleHit.distance;

            if (playerBeforeObstacle)
            {
                PlayerHealth playerHealth = playerHit.collider.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                endPosition = playerHit.point;
            }
        }
        ShowLaser(startPosition, endPosition);
    }
    private void ShowLaser(Vector2 startPosition, Vector2 endPosition)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        lineRenderer.enabled = true;

        CancelInvoke(nameof(HideLaser));
        Invoke(nameof(HideLaser), laserDuration);
    }

    private void HideLaser()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
