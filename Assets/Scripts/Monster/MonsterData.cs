using UnityEngine;

public enum MonsterType
{
    Ranged,
    Melee
}
[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData", order = 1)]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;
    public MonsterType monsterType; 
    public GameObject monsterPrefab;
    
    [Header("능력치")] 
    
    public float attackPower;
    public float maxHp;
    public float currentHp;
    public float moveSpeed;
    public float attackRange;
    public float attackSpeed;

    [Header("원거리 전용")]
    public GameObject projectilePrefab;
}