using UnityEngine;

// 游戏音效脚本
// 几个 AudioSource 重复使用
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private int poolSize = 5;   // 同时声音的上限
    private AudioSource[] pool;
    private int index = 0;

    void Awake()
    {
        Instance = this;
        pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("AudioSource_" + i);
            go.transform.SetParent(transform);
            pool[i] = go.AddComponent<AudioSource>();
        }

        // 击杀敌机爆炸和受击的音效
        EventBus.OnEnemyKilled += OnEnemyKilled;
        EventBus.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDestroy()
    {
        EventBus.OnEnemyKilled -= OnEnemyKilled;
        EventBus.OnPlayerDamaged -= OnPlayerDamaged;
    }

    // 击杀敌机爆炸声音降低一点
    void OnEnemyKilled(Vector3 pos, AudioClip clip, int score) => PlaySfx(clip, 0.4f);
    // 受击音效用默认音量
    void OnPlayerDamaged(AudioClip clip) => PlaySfx(clip);

    // 所有音效都使用这个函数播放，轮着用几个 AudioSource
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource src = pool[index];
        index = (index + 1) % pool.Length;
        src.clip = clip;
        src.volume = volume;
        src.Play();
    }
}
