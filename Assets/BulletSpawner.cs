using UnityEngine;
using TMPro;

// 自动开火功能脚本
public class BulletSpawner : MonoBehaviour
{
    public static BulletSpawner Instance;

    public GameObject bulletPrefab;
    public float fireRate = 0.5f;        // 正常情况下射击间隔0.5f
    private float baseFireRate;          // 记录射击间隔的原始值，强化结束还原用
    public float minFireRate = 0.1f;     // 强化间隔可以达到的最大值0.1f
    public float powerUpDuration = 5f;   // 强化道具的持续时间5f

    private float powerUpTimer = 0f;    // 强化道具剩余的持续时间
    public bool IsPoweredUp => powerUpTimer > 0f;   // 用于判断强化有没有结束，需不需要继续闪烁
    private float timer;
    private AudioSource audioSource;
    public TextMeshProUGUI powerUpText;   // 顶部强化倒计时文字

    void Awake()
    {
        Instance = this;
        baseFireRate = fireRate;   // 记录原始射击间隔
    }

    void Start()
    {
        // 用来挂住射击音效的文件
        audioSource = GetComponent<AudioSource>();

        // 子弹池预热 20、上限 40，射速 0.5 秒一发
        if (bulletPrefab != null && ObjectPool.Instance != null)
            ObjectPool.Instance.CreatePool(bulletPrefab, 20, 40);

        // 倒计时文字
        if (powerUpText == null) CreatePowerUpText();

        // 强化道具
        EventBus.OnItemCollected += OnItemCollected;
    }

    void OnDestroy()
    {
        EventBus.OnItemCollected -= OnItemCollected;
    }

    void OnItemCollected(Item.ItemType type, Vector3 pos)
    {
        if (type == Item.ItemType.PowerUp)
        {
            ApplyPowerUp(0.05f);
            CameraShake.Instance?.Shake(0.1f, 0.1f);
        }
    }

    void Update()
    {
        // 暂停或结算时不开火，但显示倒计时文字
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            UpdatePowerUpUI();
            return;
        }

        // 强化倒计时，倒计时归零了还原射击间隔
        if (powerUpTimer > 0f)
        {
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0f)
            {
                powerUpTimer = 0f;
                fireRate = baseFireRate;
            }
        }
        UpdatePowerUpUI();

        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            timer = 0f;

            if (bulletPrefab != null && ObjectPool.Instance != null)
                ObjectPool.Instance.GetObject(bulletPrefab, transform.position, Quaternion.identity);

            // 射击音效
            if (audioSource != null && audioSource.clip != null)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySfx(audioSource.clip, 0.2f);
                else
                    audioSource.PlayOneShot(audioSource.clip, 0.2f);
            }
        }
    }

    // 每次吃强化间隔减 0.05 秒，可以叠加，同时把强化持续时间重置
    public void ApplyPowerUp(float amount)
    {
        fireRate = Mathf.Max(minFireRate, fireRate - amount);
        powerUpTimer = powerUpDuration;
    }

    // 创建倒计时文字
    void CreatePowerUpText()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("PowerUpTimerText");
        go.transform.SetParent(canvas.transform, false);
        powerUpText = go.AddComponent<TextMeshProUGUI>();
        powerUpText.text = "";
        powerUpText.fontSize = 22;
        powerUpText.color = new Color(1f, 0.65f, 0.15f);
        powerUpText.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -30f);
        rt.sizeDelta = new Vector2(320f, 36f);
    }

    void UpdatePowerUpUI()
    {
        if (powerUpText == null) return;
        powerUpText.text = powerUpTimer > 0f ? $"SPEED BOOST {powerUpTimer:F1}s" : "";
    }
}
