using UnityEngine;
using System.Collections.Generic;

// 刷怪脚本
// 按照分数算生成间隔，难度递增
// 从配置表里随机挑一种敌机生成
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    // 敌机配置表
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

    public float minX = -6f;    // 敌机的左右生成范围
    public float maxX = 4f;

    public float baseSpawnRate = 1.5f;  // 开局最初多少秒生成一个
    public float minSpawnRate = 0.2f;   // 生成敌机的最高难度间隔

    private float timer;

    void Start()
    {
        // 配置表空的就使用默认配置
        if (enemyConfigs.Count == 0 && enemyPrefab != null)
        {
            EnemyConfig def = ScriptableObject.CreateInstance<EnemyConfig>();
            def.name = "DefaultEnemyConfig";
            def.prefab = enemyPrefab;
            def.speed = 3f;
            def.scoreValue = 1;
            enemyConfigs.Add(def);
        }

        // 每种敌机各预热 10 个、上限 30 个
        foreach (var cfg in enemyConfigs)
        {
            if (cfg != null && cfg.prefab != null && ObjectPool.Instance != null)
                ObjectPool.Instance.CreatePool(cfg.prefab, 10, 30);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // 分数越高，难度越大，刷敌机越快
        // 每 10 分难度才提一档
        float spawnRate = baseSpawnRate;
        if (GameManager.Instance != null)
        {
            int currentScore = GameManager.Instance.CurrentScore;
            spawnRate = Mathf.Max(minSpawnRate, baseSpawnRate - (currentScore / 10) * 0.1f);
        }

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            timer = 0f;
            if (enemyConfigs.Count == 0) return;

            // 随机抽一种敌机
            EnemyConfig cfg = enemyConfigs[Random.Range(0, enemyConfigs.Count)];
            if (cfg == null || cfg.prefab == null) return;

            // 横向范围随机，纵向固定在屏幕上方外面
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(randomX, 7f, 0);

            GameObject e = ObjectPool.Instance?.GetObject(cfg.prefab, spawnPos, Quaternion.identity);
            if (e != null)
            {
                Enemy en = e.GetComponent<Enemy>();
                if (en != null) en.SetConfig(cfg); // 把这次的配置输入
            }
        }
    }
}
