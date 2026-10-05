using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    public float speed = 8f;

    public float lifeTime = 3f;

    private Vector2 direction;

    private float damage;

    public void SetDirection(Vector2 newDirection, float newDamage)
    {
        direction = newDirection.normalized;
        damage = newDamage;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(damage));
            }

            Destroy(gameObject);
        }
    }
}