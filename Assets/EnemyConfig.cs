using UnityEngine;

// 敌机的配置
// 可以修改敌机的数值配置
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    public GameObject prefab;   // 用的预制体
    [Range(0.5f, 12f)] public float speed = 3f; // 下落速度3f
    public int scoreValue = 1;  // 击杀给的分数
    public EnemyMovementType movement = EnemyMovementType.Straight;  // 敌机行动模式
    public Color tint = Color.white;    // 给敌机图片染色

    public enum EnemyMovementType { Straight, Zigzag, Chase }
}
