using UnityEngine;

// 掉落道具功能脚本
public class Item : MonoBehaviour
{
    public enum ItemType { Heal, PowerUp }
    public ItemType itemType = ItemType.Heal; // 掉落的时候由 Bullet 指定
    public float speed = 2f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateAppearance();
    }

    // 从对象池里取出来时调用，设定道具类型及颜色
    // 不能在 Start 中改，复用的时候 Start 不会再执行
    public void SetType(ItemType t)
    {
        itemType = t;
        UpdateAppearance();
    }

    // 按道具类型上色
    void UpdateAppearance()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (itemType == ItemType.Heal)
            sr.color = new Color(0.25f, 1f, 0.35f); // 绿色道具加血
        else
            sr.color = new Color(1f, 0.55f, 0.1f);   // 橙色道具火力强化
    }

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 掉出屏幕底就收回去
        if (transform.position.y < -7f)
        {
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发广播，加血跳转 PlayerHealth，强化跳转 BulletSpawner
            EventBus.ItemCollected(itemType, transform.position);
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }
}
