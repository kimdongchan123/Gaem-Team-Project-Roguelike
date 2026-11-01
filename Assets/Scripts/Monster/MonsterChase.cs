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
        // 사거리 안에서는 멈추고, 벗어나면 다시 추적 (moveSpeed는 건드리지 않음)
        if (scanner != null && scanner.nearestTarget != null)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }
}