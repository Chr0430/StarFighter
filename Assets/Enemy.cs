using UnityEngine;

// 敌机脚本
// 速度、得分、飞法、颜色从 EnemyConfig 中获取
// 只执行飞行和判断撞没撞上
public class Enemy : MonoBehaviour
{
    public float speed = 3f;              // 敌机飞行速度3f
    private EnemyConfig config;           // EnemySpawner 生成时使用
    private float baseX;                  // 出生时的横坐标，蛇形走位时用
    private float timeAlive;              // 活了几秒，算正弦相位用
    private bool hasDived;                // 俯冲型敌机，是否完成了俯冲
    private Vector3 diveDir;              // 冲出去的方向，锁定后不再更改

    // 击杀敌机给几分，默认1分
    public int ScoreValue => config != null ? config.scoreValue : 1;

    // 每次从池里取出来调一次，把这个敌机的配置输入
    public void SetConfig(EnemyConfig cfg)
    {
        config = cfg;
        if (cfg != null) speed = cfg.speed;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = (cfg != null && cfg.tint != Color.white) ? cfg.tint : Color.white;
    }

    // 只重置跟配置无关的状态，配置相关的交给 SetConfig
    void OnEnable()
    {
        baseX = transform.position.x;
        timeAlive = 0f;
        hasDived = false;
        diveDir = Vector3.zero;
    }

    void Update()
    {
        // 分数越高下落得越快
        float difficulty = 1f;
        if (GameManager.Instance != null)
            difficulty = 1f + GameManager.Instance.CurrentScore * 0.01f;

        timeAlive += Time.deltaTime;
        float moveSpeed = speed * difficulty;

        EnemyConfig.EnemyMovementType movement =
            config != null ? config.movement : EnemyConfig.EnemyMovementType.Straight;

        if (movement == EnemyConfig.EnemyMovementType.Zigzag)
        {
            // 蛇形：竖着匀速掉，横向按正弦来回摆
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            float x = baseX + Mathf.Sin(timeAlive * 3f) * 1.5f;
            transform.position = new Vector3(x, transform.position.y, 0f);
        }
        else if (movement == EnemyConfig.EnemyMovementType.Chase)
        {
            // 俯冲型：先直着飞下来，进屏幕后锁定飞船那一刻的位置俯冲一次
            if (!hasDived)
            {
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                if (transform.position.y < 4f)
                {
                    Transform player = GameManager.Instance?.playerHealth?.transform;
                    if (player != null)
                    {
                        diveDir = (player.position - transform.position).normalized;
                        hasDived = true;
                    }
                    // 场景里没飞船就一直向下飞
                }
            }
            else
            {
                // 沿锁定的方向冲，速度是正常下落的 2.2 倍
                transform.position += diveDir * moveSpeed * 2.2f * Time.deltaTime;
            }
        }
        else
        {
            // 直线往下
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }

        // 敌机从屏幕底下溜走就扣一点血
        if (transform.position.y < -7f)
        {
            GameManager.Instance?.DamagePlayer(1);
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 撞上飞船，扣血，敌机自己消失
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.DamagePlayer(1);
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }
}
