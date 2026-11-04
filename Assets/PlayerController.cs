using UnityEngine;

// 飞机移动脚本
// 把读取输入和实际移动分开，速度不受帧率影响
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;    // 移动速度
    public float minX = -6f;   // 左右边界
    public float maxX = 4f;
    public float minY = -5f;   // 上下边界
    public float maxY = 5f;

    private float moveX;
    private float moveY;

    // 读取输入
    void Update()
    {
        // 暂停或结算时不读取输入
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
    }

    // 实际移动玩家飞机位置
    void FixedUpdate()
    {
        float newX = transform.position.x + moveX * moveSpeed * Time.fixedDeltaTime;
        float newY = transform.position.y + moveY * moveSpeed * Time.fixedDeltaTime;

        // 用边界限制飞机移动范围，别飞出屏幕
        newX = Mathf.Clamp(newX, minX, maxX);
        newY = Mathf.Clamp(newY, minY, maxY);

        transform.position = new Vector3(newX, newY, 0);
    }
}
