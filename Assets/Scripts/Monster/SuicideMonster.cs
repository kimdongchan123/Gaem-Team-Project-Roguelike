using UnityEngine;

public class SuicideMonster : MonoBehaviour
{
    private enum State
    {
        Chasing,
        Countdown,
        Exploding,
        Dead
    }

    public Scanner scanner;

    public float chaseSpeed = 8f;

    public float explodeRange = 1.5f;

    public float countdownDuration = 2f;

    public float explosionDamage = 999f;

    public LayerMask playerLayer;

    public AudioClip warningSound;

    public GameObject explosionIndicatorPrefab;

    public GameObject explosionEffectPrefab;

    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;

    private GameObject indicatorInstance;

    private State currentState = State.Chasing;

    private float countdownTimer = 0f;

    private bool warningPlayed = false;

    private Vector3 originalScale;

    private Color originalColor;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Chasing:
                UpdateChasing();
                break;
            case State.Countdown:
                UpdateCountdown();
                break;
        }
    }

    private void UpdateChasing()
    {
        if (scanner.nearestTarget == null)
        {
            return;
        }

        if (!warningPlayed)
        {
            warningPlayed = true;
            if (audioSource != null && warningSound != null)
            {
                audioSource.PlayOneShot(warningSound);
            }
        }

        Vector3 target = scanner.nearestTarget.position;
        float distance = Vector3.Distance(transform.position, target);

        if (distance <= explodeRange)
        {
            EnterCountdown();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
    }

    private void EnterCountdown()
    {
        currentState = State.Countdown;
        countdownTimer = 0f;

        if (explosionIndicatorPrefab != null)
        {
            indicatorInstance = Instantiate(explosionIndicatorPrefab, transform.position, Quaternion.identity);
            indicatorInstance.transform.localScale = new Vector3(explodeRange * 2f, explodeRange * 2f, 1f);
        }
    }

    private void UpdateCountdown()
    {
        countdownTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(countdownTimer / countdownDuration);

        if (spriteRenderer != null)
        {
            float blink = Mathf.PingPong(Time.time * 10f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, blink);
        }

        float scaleMultiplier = Mathf.Lerp(1f, 1.4f, progress);
        transform.localScale = originalScale * scaleMultiplier;

        if (countdownTimer >= countdownDuration)
        {
            Explode();
        }
    }

    private void Explode()
    {
        currentState = State.Exploding;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(explosionDamage));
            }
        }

        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        Die();
    }

    private void Die()
    {
        currentState = State.Dead;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}