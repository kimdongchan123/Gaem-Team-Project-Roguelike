using UnityEngine;

public class MonsterChase : MonoBehaviour
{
    public Scanner scanner;

    public MonsterData monsterData;

    public float moveSpeed = 5f;

    private Transform target;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("[MonsterChase] Player 태그를 가진 오브젝트를 찾지 못했습니다.");
        }

        if (monsterData != null && scanner != null)
        {
            scanner.scanRange = monsterData.attackRange;
            Debug.Log("[MonsterChase] scanner.scanRange를 " + monsterData.attackRange + "로 설정함");
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }
        if (scanner.nearestTarget != null)
        {
            moveSpeed = 0f;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }
}