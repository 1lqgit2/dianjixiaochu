using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 广告统一入口 —— 只保留抖音小游戏（TTSDK）渠道。
/// 真正的实现链路：AdsControl → AdManager → TikTokPlatform。
/// 场景里没有挂 AdManager 时，这里会自动补一个并把配置传过去。
/// </summary>
public class AdsControl : MonoBehaviour
{
    private static AdsControl instance;

    public static AdsControl Instance { get { return instance; } }

    [Header("抖音小游戏")]
    [Tooltip("小游戏 AppID，只用于 ECPM/转化回传，可留空")]
    public string appId = "";

    [Tooltip("激励视频广告位 ID，可填多个，每次随机取一个")]
    public List<string> videoAdIds = new List<string>();

    [Tooltip("插屏广告位 ID，可填多个，每次随机取一个")]
    public List<string> interAdIds = new List<string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAdManager();
    }

    /// <summary>
    /// 抖音广告必须由 AdManager 驱动。场景里没挂时自动补一个，
    /// 并把 AdsControl 上填的广告位配置同步过去。
    /// </summary>
    private void EnsureAdManager()
    {
        if (AdManager.Instance != null)
        {
            return;
        }

        var manager = gameObject.GetComponent<AdManager>();
        if (manager == null)
        {
            manager = gameObject.AddComponent<AdManager>();
        }

        if (!string.IsNullOrEmpty(appId))
        {
            manager.AppId = appId;
        }

        if (videoAdIds != null && videoAdIds.Count > 0)
        {
            manager.videoAdIdList = new List<string>(videoAdIds);
        }

        if (interAdIds != null && interAdIds.Count > 0)
        {
            manager.interAdIdList = new List<string>(interAdIds);
        }

        Debug.Log("[广告] 场景中没有 AdManager，已在 AdsControl 所在物体上自动创建");
    }

    /// <summary>
    /// 播放抖音激励视频。
    /// 用户看完 → onSuccess；中途关闭、加载失败或 SDK 异常 → onFail。
    /// </summary>
    public void ShowRewardAd(Action onSuccess, Action<string> onFail = null)
    {
        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] AdManager 未初始化，无法播放激励视频");
            onFail?.Invoke("AdManager 未初始化");
            return;
        }

        AdManager.Instance.ShowRewardedVideoAd(onSuccess, onFail);
    }

    /// <summary>
    /// 播放抖音插屏广告。抖音侧有 15 秒冷启动限制和 30 秒间隔限制，
    /// 不满足条件时 TikTokPlatform 会打警告并跳过，不会报错。
    /// </summary>
    public void ShowInterstital()
    {
        if (IsRemoveAds())
        {
            return;
        }

        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] AdManager 未初始化，无法播放插屏广告");
            return;
        }

        AdManager.Instance.ShowInterstitialAd();
    }

    /// <summary>
    /// 抖音激励视频是点击时才拉起的，不需要预先加载，
    /// 只要 AdManager 就绪就认为可播。
    /// </summary>
    public bool IsRewardedVideoAvailable()
    {
        return AdManager.Instance != null;
    }

    public void RemoveAds()
    {
        PlayerPrefs.SetInt("removeAds", 1);
    }

    public bool IsRemoveAds()
    {
        return PlayerPrefs.HasKey("removeAds") && PlayerPrefs.GetInt("removeAds") == 1;
    }
}
