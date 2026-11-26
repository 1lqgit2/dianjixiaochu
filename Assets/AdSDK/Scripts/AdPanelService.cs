using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 广告面板统一入口：负责面板的初始隐藏、打开置顶、射线接收，以及两个入口按钮的兜底绑定。
/// 这样面板自己不再在 Awake 里 SetActive(false)，避免"第一次打开的那一帧又把自己关掉"。
/// </summary>
public static class AdPanelService
{
    public const string AdConfirmName = "AdConfirm";
    public const string CeBianPanelName = "cebianPanel";

    /// <summary>面板画布排序值，压过场景里所有 UI（包括 overrideSorting=10 的弹窗画布）。</summary>
    public const int TopSortingOrder = 31000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // 1) 面板初始隐藏（由这里统一收口，不依赖面板脚本自身的 Awake）
        SetPanelVisible(AdConfirmName, false);
        SetPanelVisible(CeBianPanelName, false);

        // 2) 两个入口按钮兜底接线：即使场景里的 onClick 丢了也能打开
        BindButton("jiliBtn", AdConfirmName);
        BindButton("cebianBtn", CeBianPanelName);
    }

    public static void Show(string panelName)
    {
        SetPanelVisible(panelName, true);
    }

    public static void Hide(string panelName)
    {
        SetPanelVisible(panelName, false);
    }

    public static void SetPanelVisible(string panelName, bool visible)
    {
        GameObject go = FindDeep(panelName);
        if (go == null)
        {
            // 场景里被删了就从 Resources 兜底生成，挂到主画布下
            GameObject prefab = Resources.Load<GameObject>(panelName);
            if (prefab != null)
            {
                Canvas root = FindMainCanvas();
                go = Object.Instantiate(prefab, root != null ? root.transform : null);
                go.name = panelName;
            }
        }

        if (go == null)
        {
            Debug.LogError("[广告面板] 场景和 Resources 里都找不到 " + panelName);
            return;
        }

        if (!visible)
        {
            go.SetActive(false);
            return;
        }

        EnsureTopLayer(go);
        go.SetActive(true);
        go.transform.SetAsLastSibling();

        // 场景里按钮自带的 onClick 可能也会打开同一个面板，再加上这里的兜底监听，
        // 一次点击会走到这里两次。日志做 0.5 秒去重，只留一条，避免刷屏。
        float now = Time.unscaledTime;
        if (panelName == lastLoggedPanel && now - lastLoggedTime <= 0.5f)
        {
            return;
        }

        lastLoggedPanel = panelName;
        lastLoggedTime = now;

        Debug.Log("[广告面板] 打开 " + panelName +
                  "，父节点=" + (go.transform.parent != null ? go.transform.parent.name : "无") +
                  "，activeInHierarchy=" + go.activeInHierarchy);
    }

    private static string lastLoggedPanel;
    private static float lastLoggedTime = -10f;

    private static Canvas FindMainCanvas()
    {
        Canvas[] all = Object.FindObjectsOfType<Canvas>(true);
        Canvas best = null;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null || !all[i].isRootCanvas) continue;
            // 优先取排行榜里最靠前、且不是我们自己面板上的
            if (all[i].name == AdConfirmName || all[i].name == CeBianPanelName) continue;
            if (best == null || all[i].sortingOrder <= best.sortingOrder) best = all[i];
        }
        return best;
    }

    /// <summary>
    /// 把面板做成"顶层独立 Overlay 画布"：
    /// 脱离场景里层层嵌套的画布 / 排序 / 遮罩，保证一定显示在最前面，并且点击能命中。
    /// 缩放参数从主画布的 CanvasScaler 复制，保证和设计稿尺寸一致。
    /// </summary>
    public static void EnsureTopLayer(GameObject go)
    {
        // 提根之前先记住原来的父画布，缩放参数照它抄
        Canvas sourceCanvas = go.GetComponentInParent<Canvas>();

        Canvas canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = go.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = false;
        canvas.sortingOrder = TopSortingOrder;
        canvas.worldCamera = null;

        // 提到场景根节点，避免被父级画布的 CanvasGroup / 排序 / Mask 影响
        if (go.transform.parent != null)
        {
            go.transform.SetParent(null, false);
        }

        CopyScaler(sourceCanvas, go);

        if (go.GetComponent<GraphicRaycaster>() == null)
        {
            go.AddComponent<GraphicRaycaster>();
        }
    }

    private static void CopyScaler(Canvas source, GameObject go)
    {
        if (source == null) return;

        CanvasScaler mainScaler = source.GetComponent<CanvasScaler>();
        if (mainScaler == null) return;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = go.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = mainScaler.uiScaleMode;
        scaler.referenceResolution = mainScaler.referenceResolution;
        scaler.screenMatchMode = mainScaler.screenMatchMode;
        scaler.matchWidthOrHeight = mainScaler.matchWidthOrHeight;
        scaler.referencePixelsPerUnit = mainScaler.referencePixelsPerUnit;
    }

    private static void BindButton(string buttonName, string panelName)
    {
        GameObject go = FindDeep(buttonName);
        if (go == null)
        {
            Debug.LogWarning("[广告面板] 找不到按钮 " + buttonName);
            return;
        }

        Button button = go.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[广告面板] " + buttonName + " 上没有 Button 组件");
            return;
        }

        button.onClick.AddListener(delegate { Show(panelName); });

        Canvas owner = button.GetComponentInParent<Canvas>();
        bool hasRaycaster = owner != null && owner.GetComponent<GraphicRaycaster>() != null;
        Debug.Log("[广告面板] 已为 " + buttonName + " 绑定打开 " + panelName +
                  "（所在画布=" + (owner != null ? owner.name : "无") +
                  "，GraphicRaycaster=" + hasRaycaster +
                  "，可交互=" + button.IsInteractable() + "）");
    }

    /// <summary>按名字找对象，包含未激活的。</summary>
    public static GameObject FindDeep(string name)
    {
        Transform[] all = Object.FindObjectsOfType<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == name)
            {
                return all[i].gameObject;
            }
        }
        return null;
    }
}
