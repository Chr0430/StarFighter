using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 暂停和结算面板
public class UIManager : MonoBehaviour
{
    private static UIManager Instance;
    private GameObject panelRoot;   // 所有面板都挂这个节点下

    void Awake()
    {
        Instance = this;

        // 新建 Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("UICanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        panelRoot = new GameObject("DynamicPanels");
        panelRoot.transform.SetParent(canvas.transform, false);
    }

    // 结算面板，按钮用 GameManager 的方法
    public static void ShowGameOver(System.Action onRestart, System.Action onMenu)
    {
        if (Instance == null) return;
        Instance.BuildPanel("Game Over", new List<(string, System.Action)>
        {
            ("Restart", onRestart),
            ("Menu", onMenu)
        });
    }

    // 暂停面板，比结算面板多一个继续功能
    public static void ShowPause(System.Action onResume, System.Action onRestart, System.Action onMenu)
    {
        if (Instance == null) return;
        Instance.BuildPanel("Paused", new List<(string, System.Action)>
        {
            ("Resume", onResume),
            ("Restart", onRestart),
            ("Menu", onMenu)
        });
    }

    public static void Hide()
    {
        if (Instance == null) return;
        foreach (Transform t in Instance.panelRoot.transform) Destroy(t.gameObject);
    }

    private void BuildPanel(string title, List<(string, System.Action)> buttons)
    {
        // 先清除面板，免得两个面板叠一起
        foreach (Transform t in panelRoot.transform) Destroy(t.gameObject);

        GameObject panel = new GameObject(title + "Panel");
        panel.transform.SetParent(panelRoot.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        // 按钮数量决定高度
        float panelHeight = 200f + buttons.Count * 60f;
        prt.sizeDelta = new Vector2(320f, panelHeight);
        prt.anchoredPosition = Vector2.zero;

        // 半透明黑底
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject tObj = new GameObject("Title");
        tObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI tt = tObj.AddComponent<TextMeshProUGUI>();
        tt.text = title;
        tt.fontSize = 36;
        tt.alignment = TextAlignmentOptions.Center;
        tt.color = Color.white;
        RectTransform trt = tObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.6f);
        trt.anchorMax = new Vector2(1f, 0.9f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        // 按钮从上往下排，每个隔 60
        float startY = -20f;
        for (int i = 0; i < buttons.Count; i++)
        {
            CreateButton(panel, buttons[i].Item1, new Vector2(0f, startY - i * 60f), buttons[i].Item2);
        }
    }

    private void CreateButton(GameObject parent, string label, Vector2 pos, System.Action onClick)
    {
        GameObject btn = new GameObject(label + "Btn");
        btn.transform.SetParent(parent.transform, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 44f);
        rt.anchoredPosition = pos;

        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.9f, 1f);
        Button b = btn.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(() => onClick?.Invoke());

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(btn.transform, false);
        TextMeshProUGUI tm = txt.AddComponent<TextMeshProUGUI>();
        tm.text = label;
        tm.fontSize = 24;
        tm.alignment = TextAlignmentOptions.Center;
        tm.color = Color.white;
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
