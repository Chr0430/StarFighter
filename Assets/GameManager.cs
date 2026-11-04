using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// 全局总控
// 管理游戏状态、分数、存档
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 四个状态，主菜单、正在游玩、暂停、游戏结束
    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public PlayerHealth playerHealth;

    public string menuSceneName = "MainMenu"; // 主菜单名称

    public int CurrentScore { get { return score; } }

    private int score = 0;
    private int highScore = 0;

    void Awake()
    {
        Instance = this;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;   // 从暂停或结算重进来时，时间倍率重置为1

        // 缺哪个管理器就生成
        if (FindAnyObjectByType<AudioManager>() == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();
        if (FindAnyObjectByType<UIManager>() == null)
            new GameObject("UIManager").AddComponent<UIManager>();
        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
            Camera.main.gameObject.AddComponent<CameraShake>();

        // 以前的最高分读出来显示
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore;

        // 击杀敌机、玩家失败，都通过事件总线传递
        EventBus.OnEnemyKilled += OnEnemyKilled;
        EventBus.OnPlayerDied += OnPlayerDied;
    }

    void OnDestroy()
    {
        EventBus.OnEnemyKilled -= OnEnemyKilled;
        EventBus.OnPlayerDied -= OnPlayerDied;
    }

    // 击杀敌机加分
    void OnEnemyKilled(Vector3 pos, AudioClip clip, int score) => AddScore(score);
    // 玩家失败进结算界面
    void OnPlayerDied() => GameOver();

    void Update()
    {
        // Esc 或 P 切换暂停
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (CurrentState == GameState.Playing) Pause();
            else if (CurrentState == GameState.Paused) Resume();
        }

        // 结算界面按 R 直接重开
        if (CurrentState == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void Pause()
    {
        if (CurrentState != GameState.Playing) return;   // 已经在别的状态就不需要执行
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        SaveHighScore();
        UIManager.ShowPause(Resume, Restart, ReturnToMenu);
    }

    public void Resume()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Hide();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SaveHighScore();
        // 重载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SaveHighScore();
        SceneManager.LoadScene(menuSceneName);
    }

    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing) return;

        score += amount;
        if (scoreText != null) scoreText.text = "Score: " + score;

        // 破纪录只改内存
        // 分数一路往上涨，要是每破一次就保存
        if (score > highScore)
        {
            highScore = score;
            if (highScoreText != null) highScoreText.text = "Best: " + score;
        }
    }

    // 保存最高分数
    void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    void OnApplicationQuit()
    {
        SaveHighScore();
    }

    // 玩家掉血触发函数
    public void DamagePlayer(int amount)
    {
        if (CurrentState != GameState.Playing) return;
        if (playerHealth != null)
            playerHealth.TakeDamage(amount);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;   // 一帧里可能被叫好几次
        CurrentState = GameState.GameOver;

        Time.timeScale = 0f;
        SaveHighScore();
        if (scoreText != null)
            scoreText.text = "Score: " + score;   // HUD 显示本局分数

        UIManager.ShowGameOver(Restart, ReturnToMenu);
    }
}
