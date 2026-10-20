using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public MonsterData[] monsterDataList;
    public Vector2 spawnAreaSize = new Vector2(10f, 10f);
    public Transform spawnAreaCenter;
    public float minSpawnDistance = 1.5f;
    public int maxSpawnAttempts = 30;
    [Tooltip("플레이어가 스폰 영역에 들어온 뒤 몬스터가 움직이고 공격하기까지의 대기 시간(초)")]
    public float activationDelay = 1f;

    private readonly List<GameObject> spawnedMonsters = new List<GameObject>();
    private Transform player;
    private bool activated;
    private bool playerDetected;
    private float activateTime;

    private void Start()
    {
        SpawnAllOnce();
    }

    private void Update()
    {
        if (activated)
        {
            return;
        }

        if (playerDetected)
        {
            if (Time.time >= activateTime)
            {
                ActivateMonsters();
            }
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                return;
            }
            player = playerObject.transform;
        }

        Vector2 center = spawnAreaCenter != null
            ? (Vector2)spawnAreaCenter.position
            : (Vector2)transform.position;
        Vector2 offset = (Vector2)player.position - center;

        if (Mathf.Abs(offset.x) <= spawnAreaSize.x / 2f && Mathf.Abs(offset.y) <= spawnAreaSize.y / 2f)
        {
            playerDetected = true;
            activateTime = Time.time + activationDelay;
        }
    }

    private void ActivateMonsters()
    {
        activated = true;

        foreach (GameObject monster in spawnedMonsters)
        {
            if (monster != null)
            {
                SetMonsterAI(monster, true);
            }
        }
    }

    private static void SetMonsterAI(GameObject monster, bool enabled)
    {
        foreach (MonoBehaviour behaviour in monster.GetComponents<MonoBehaviour>())
        {
            if (behaviour is MonsterChase || behaviour is MonsterAttack || behaviour is MonsterRangedAttack
                || behaviour is RangeMonster || behaviour is ChargeMonster || behaviour is SuicideMonster
                || behaviour is AcceleratingMonster || behaviour is EliteMonster || behaviour is MonsterLaser)
            {
                behaviour.enabled = enabled;
            }
        }
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

        Vector2 spawnPos = GetNonOverlappingPosition();
        GameObject monster = Instantiate(data.monsterPrefab, spawnPos, Quaternion.identity);
        monster.name = data.monsterName;
        spawnedMonsters.Add(monster);

        MonsterHealth health = monster.GetComponent<MonsterHealth>();
        if (health != null)
        {
            health.Init(data);
        }

        MonsterChase chase = monster.GetComponent<MonsterChase>();
        if (chase != null)
        {
            chase.monsterData = data;
        }

        if (!activated)
        {
            SetMonsterAI(monster, false);
        }

        return monster;
    }

    private Vector2 GetNonOverlappingPosition()
    {
        spawnedMonsters.RemoveAll(m => m == null);

        Vector2 bestPosition = GetRandomPositionInArea();
        float bestDistance = GetNearestMonsterDistance(bestPosition);

        if (bestDistance >= minSpawnDistance)
        {
            return bestPosition;
        }

        for (int i = 1; i < maxSpawnAttempts; i++)
        {
            Vector2 candidate = GetRandomPositionInArea();
            float nearestDistance = GetNearestMonsterDistance(candidate);

            if (nearestDistance >= minSpawnDistance)
            {
                return candidate;
            }

            if (nearestDistance > bestDistance)
            {
                bestDistance = nearestDistance;
                bestPosition = candidate;
            }
        }

        Debug.LogWarning("[MonsterSpawner] 겹치지 않는 위치를 찾지 못해 가장 여유 있는 위치에 소환합니다. minSpawnDistance를 줄이거나 스폰 영역을 넓혀보세요.");
        return bestPosition;
    }

    private float GetNearestMonsterDistance(Vector2 position)
    {
        float nearest = float.MaxValue;

        foreach (GameObject spawned in spawnedMonsters)
        {
            float distance = Vector2.Distance(position, spawned.transform.position);
            if (distance < nearest)
            {
                nearest = distance;
            }
        }

        return nearest;
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