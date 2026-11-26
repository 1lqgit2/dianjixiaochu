using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 激励广告弹窗
/// </summary>
public class AdConfirm : MonoBehaviour
{
    private static AdConfirm instance;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;

    /// <summary>没有传回调时（比如首页 jiliBtn 直接 SetActive 打开），看完广告默认发多少金币。</summary>
    [SerializeField] private int defaultRewardCoins = 100;

    private Action onCompleted;

    public static void Show(Action callBack)
    {
        if (instance == null)
        {
            // 场景里已经有 AdConfirm（挂在 Canvas 下）时优先用它，别再生成第二份
            GameObject existing = AdPanelService.FindDeep(AdPanelService.AdConfirmName);
            if (existing != null)
            {
                instance = existing.GetComponent<AdConfirm>();
            }
        }

        if (instance == null)
        {
            // 加载 AdConfirm 预制体
            GameObject prefab = Resources.Load<GameObject>("AdConfirm");
            if (prefab == null)
            {
                Debug.LogError("AdConfirm 预制体未找到，请确保其位于 Resources 文件夹中。");
                return;
            }

            // 实例化 AdConfirm
            GameObject adConfirmInstance = Instantiate(prefab);
            instance = adConfirmInstance.GetComponent<AdConfirm>();

            if (instance == null)
            {
                Debug.LogError("AdConfirm 脚本未附加到预制体上，请检查预制体设置。");
                Destroy(adConfirmInstance);
                return;
            }
        }

        // 设置回调并显示弹窗
        instance.onCompleted = callBack;
        AdPanelService.EnsureTopLayer(instance.gameObject);
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    private void Awake()
    {
        // 只登记第一个，不销毁别的实例：
        // 以前这里会 Destroy 掉"重复"的自己，如果 Resources 里先实例化过一份，
        // 场景里这份一被打开就会被判成重复直接销毁，表现就是点了没反应。
        if (instance == null)
        {
            instance = this;
        }

        btnClose?.onClick.AddListener(this.OnClose);
        btnConfirm?.onClick.AddListener(this.OnConfirm);
        // 初始隐藏由 AdPanelService 统一处理，这里不自我隐藏
        // （未激活对象首次被打开时才会执行 Awake，那时再 SetActive(false) 会立刻关掉自己）
    }

    private void OnEnable()
    {
        // 打开时置顶（提成顶层画布的工作由 AdPanelService 在打开时统一做）
        transform.SetAsLastSibling();
    }


    private void OnClose()
    {
        onCompleted = null;
        this.gameObject.SetActive(false);
    }

    void OnConfirm()
    {
#if UNITY_EDITOR
        GrantReward();
        this.OnClose();
#else
        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] 场景里没有 AdManager，无法播放广告");
            OnAdError("广告服务未初始化");
            return;
        }
        AdManager.Instance.ShowRewardedVideoAd(OnAdWatched, OnAdError);
#endif
    }

    private void OnAdWatched()
    {
        Debug.Log("广告观看完成");
        GrantReward();
        OnClose();
    }

    /// <summary>广告看完后发奖：优先用 Show() 传进来的回调，没有就发默认金币。</summary>
    private void GrantReward()
    {
        if (onCompleted != null)
        {
            onCompleted.Invoke();
            return;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.AddCoin(defaultRewardCoins);
            Debug.Log($"[广告] 激励视频完成，发放金币 {defaultRewardCoins}");
        }
    }

    private void OnAdError(string errorMessage)
    {
        Debug.LogError($"广告播放失败: {errorMessage}");
        // 加载 AdError 预制体
        GameObject adErrorPrefab = Resources.Load<GameObject>("AdError");
        if (adErrorPrefab != null)
        {
            // 实例化 AdError，挂到弹窗画布下，保证在屏幕中间能看到
            GameObject adErrorInstance = Instantiate(adErrorPrefab, transform, false);
            adErrorInstance.transform.localPosition = Vector3.zero;
            // 1.5 秒后销毁实例
            Destroy(adErrorInstance, 1.5f);
        }
        else
        {
            Debug.LogError("AdError 预制体未找到，请确保其位于 Resources 文件夹中。");
        }
    }
}
