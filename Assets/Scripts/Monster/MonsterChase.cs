using UnityEngine;

public class MonsterChase : MonoBehaviour
{
    public Scanner scanner;

    public float moveSpeed = 5f;


    private Transform target;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;              
        }
    
    }
    private void Update()
    {
        if(target == null)
        {
            return;
        }
        if (scanner.nearestTarget != null) {
            moveSpeed = 0f;
        }
        else {
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }

    }
}
