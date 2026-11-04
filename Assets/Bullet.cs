using UnityEngine;

// 子弹脚本，一直往上飞，撞到敌机就广播一次击杀，然后自己回对象池
public class Bullet : MonoBehaviour
{
    public float speed = 10f;       // 子弹速度10f
    public AudioClip explosionSound;     // 广播击杀事件
    public GameObject itemPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.2f;      // 打死敌机掉落道具的概率0.2f

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        // 子弹飞出屏幕顶就回收
        if (transform.position.y > 12f)
        {
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 同一帧两颗子弹打中同一只敌机时，第一颗已经把敌机回收了
            // 第二颗的回调还在队列里，不拦一下会重复加分、掉落两次
            if (!other.gameObject.activeSelf) return;

            int score = 1;
            Enemy en = other.GetComponent<Enemy>();
            if (en != null) score = en.ScoreValue;

            // 广播击杀事件，加分、音效、震屏会各自订阅击杀事件
            EventBus.EnemyKilled(other.transform.position, explosionSound, score);

            // 按概率掉道具，掉出来的30%是火力强化，70%是加血
            if (Random.Range(0f, 1f) <= dropChance)
            {
                GameObject dropped = ObjectPool.Instance?.GetObject(itemPrefab, other.transform.position, Quaternion.identity);
                if (dropped != null)
                {
                    Item it = dropped.GetComponent<Item>();
                    if (it != null)
                        it.SetType(Random.Range(0f, 1f) < 0.3f ? Item.ItemType.PowerUp : Item.ItemType.Heal);
                }
            }

            // 敌机和子弹都回对象池，不会 Destroy
            ObjectPool.Instance?.ReturnObject(other.gameObject);
            ObjectPool.Instance?.ReturnObject(gameObject);
        }
    }
}
