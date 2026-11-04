using UnityEngine;

// 击杀敌机的屏幕震动脚本
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;   // 记录相机原来的位置
    private float shakeTime;
    private float shakeMag;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;

        // 击杀或受击触发屏幕震动
        EventBus.OnEnemyKilled += OnEnemyKilled;
        EventBus.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDestroy()
    {
        EventBus.OnEnemyKilled -= OnEnemyKilled;
        EventBus.OnPlayerDamaged -= OnPlayerDamaged;
    }

    // 击杀时抖动幅度小，受击时抖动幅度大
    void OnEnemyKilled(Vector3 pos, AudioClip clip, int score) => Shake(0.1f, 0.1f);
    void OnPlayerDamaged(AudioClip clip) => Shake(0.15f, 0.2f);

    public void Shake(float duration, float magnitude)
    {
        shakeTime = duration;
        shakeMag = magnitude;
    }

    void Update()
    {
        if (shakeTime > 0f)
        {
            // 用 unscaledDeltaTime，结算和暂停时 timeScale 是 0
            // 用 Time.deltaTime 会减不动，镜头就在结算界面上一直抖
            shakeTime -= Time.unscaledDeltaTime;
            float x = Random.Range(-shakeMag, shakeMag);
            float y = Random.Range(-shakeMag, shakeMag);
            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            if (shakeTime <= 0f)
                transform.localPosition = originalPos;
        }
    }
}
