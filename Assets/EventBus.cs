using UnityEngine;
using System;

// 事件总线
// 控制各种事件的发布与订阅
// 发布方和订阅方不用互相认识
public static class EventBus
{
    // 敌机被击杀，位置、爆炸音效、得分
    public static event Action<Vector3, AudioClip, int> OnEnemyKilled;
    // 玩家受击，受击音效
    public static event Action<AudioClip> OnPlayerDamaged;
    // 玩家阵亡
    public static event Action OnPlayerDied;
    // 拾取道具，类型、位置
    public static event Action<Item.ItemType, Vector3> OnItemCollected;

    // 发布入口
    public static void EnemyKilled(Vector3 pos, AudioClip clip, int score) => OnEnemyKilled?.Invoke(pos, clip, score);
    public static void PlayerDamaged(AudioClip clip) => OnPlayerDamaged?.Invoke(clip);
    public static void PlayerDied() => OnPlayerDied?.Invoke();
    public static void ItemCollected(Item.ItemType type, Vector3 pos) => OnItemCollected?.Invoke(type, pos);
}
