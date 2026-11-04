using UnityEngine;
using TMPro;

// 飞船的血量
// 掉血后无敌时间，飞机显示闪光
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;   // 血量上限
    private int currentHealth;

    public TextMeshProUGUI healthText;  // 显示当前血量

    public float invincibleDuration = 1.5f;  // 无敌时间1.5f长
    private float invincibleTimer = 0f;
    private bool isInvincible = false;

    private SpriteRenderer spriteRenderer;

    public AudioClip hitSound;  // 受击音效
    public AudioClip healSound; // 加血音效

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHealthUI();

        // 加血道具功能
        EventBus.OnItemCollected += HandleItemCollected;
    }

    void OnDestroy()
    {
        EventBus.OnItemCollected -= HandleItemCollected;
    }

    // 加血道具功能，捡到就回一点血
    void HandleItemCollected(Item.ItemType type, Vector3 pos)
    {
        if (type == Item.ItemType.Heal) Heal(1);
    }

    void Update()
    {
        if (isInvincible)
        {
            // 无敌时间用透明度来回闪
            invincibleTimer -= Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.PingPong(Time.time * 10f, 1f));

            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                spriteRenderer.color = Color.white;
            }
        }
        else if (BulletSpawner.Instance != null && BulletSpawner.Instance.IsPoweredUp)
        {
            // 火力强化期间显示橙色并轻微闪，提醒玩家强化时间
            float flicker = 0.75f + 0.25f * Mathf.PingPong(Time.time * 8f, 1f);
            spriteRenderer.color = new Color(1f * flicker, 0.55f * flicker, 0.1f * flicker, 1f);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        currentHealth -= damage;

        // 广播受击事件，音效和震屏订阅该事件
        EventBus.PlayerDamaged(hitSound);

        if (currentHealth <= 0)
        {
            // 血空了，广播阵亡事件，然后自己的飞机隐藏
            EventBus.PlayerDied();
            gameObject.SetActive(false);
        }
        else
        {
            isInvincible = true;
            invincibleTimer = invincibleDuration;
        }

        UpdateHealthUI();
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth) return;   // 满血不会吃回血道具
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healSound != null)
            AudioManager.Instance?.PlaySfx(healSound);

        UpdateHealthUI();
    }

    // 用右上角文字显示当前血量，每次血量发生变动都会调用
    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth;
    }
}
