using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public MonsterData[] monsterDataList;
    public Vector2 spawnAreaSize = new Vector2(10f, 10f);
    public Transform spawnAreaCenter;

    private void Start()
    {
        SpawnAllOnce();
    }

    public void SpawnAllOnce()
    {
        if (monsterDataList == null || monsterDataList.Length == 0)
        {
            Debug.LogWarning("[MonsterSpawner] 몬스터 데이터 목록이 비어있습니다.");
            return;
        }

        foreach (MonsterData data in monsterDataList)
        {
            SpawnMonster(data);
        }
    }

    public GameObject SpawnMonster(MonsterData data)
    {
        if (data == null || data.monsterPrefab == null)
        {
            Debug.LogWarning($"[MonsterSpawner] {(data != null ? data.monsterName : "null")}에 프리팹이 지정되지 않았습니다.");
            return null;
        }

        Vector2 spawnPos = GetRandomPositionInArea();
        GameObject monster = Instantiate(data.monsterPrefab, spawnPos, Quaternion.identity);
        monster.name = data.monsterName;

        MonsterChase chase = monster.GetComponent<MonsterChase>();
        if (chase != null)
        {
            chase.monsterData = data;
        }

        return monster;
    }

    private Vector2 GetRandomPositionInArea()
    {
        Vector2 center = spawnAreaCenter != null
            ? (Vector2)spawnAreaCenter.position
            : (Vector2)transform.position;

        float halfWidth = spawnAreaSize.x / 2f;
        float halfHeight = spawnAreaSize.y / 2f;

        float randomX = Random.Range(center.x - halfWidth, center.x + halfWidth);
        float randomY = Random.Range(center.y - halfHeight, center.y + halfHeight);

        return new Vector2(randomX, randomY);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
    }
}