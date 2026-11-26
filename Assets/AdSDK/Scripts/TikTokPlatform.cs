using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//#if TickTok
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;

/*
 * 抖音平台广告
 */
public class TikTokPlatform : IAdPlatform
{
    /// <summary>
    /// 是否向第三方（ecpm.xxhygames.com）上报 openid/机型等数据做 ECPM 与转化回传。
    /// 默认关闭：上面报会把用户信息发到外部服务器，抖音审核和隐私政策里必须申报，
    /// 确认是自己的后台、且已在上架材料里写明后再改成 true。
    /// </summary>
    public static bool EnableRemoteTracking = false;

    // 小游戏ID
    private readonly string miniAppid;

    // 激励视频
    private TTRewardedVideoAd rewardedVideoAd = null;
    private string videoAdId = "";
    private Action onRewardSuccess;
    private Action<string> onRewardFail;
    private float prevTimeScale = 1f;

    // 插屏广告
    private TTInterstitialAd interAdIns;
    private string interAdId = "";
    private float moduleStartTime;
    private float lastShowTime;

    // 抖音开放平台频控（官方文档值，SDK 注释里的 15s/30s 是旧值）
    private const float InterStartupCooldown = 30f;   // 启动后 30 秒内不展示插屏
    private const float InterShowInterval = 60f;      // 两次插屏 / 激励视频后转插屏，间隔 60 秒

    /// <summary>TT.InitSDK 回调后置 true，此前不允许创建广告实例。</summary>
    private bool sdkReady;
    private float platformCreateTime;

    /// <summary>插屏广告位已判定不可用（appid 不匹配 / 类型错 / 已关闭），本次运行不再重试。</summary>
    private bool interAdBroken;
    private int interAdFailCount;
    private bool interAdLoading;
    private float interAdLoadStartTime;
    private bool interPendingShow;
    private float lastRewardCloseTime = -60f;

    // 埋点
    private readonly string ecpmURL = "https://ecpm.xxhygames.com/openid/openlog/insertlog";  // ECPM后台地址
    private readonly string conversionURL = "https://analytics.oceanengine.com/api/v2/conversion";   // 巨量引擎API接口

    private TTSystemInfo systemInfo;

    public TikTokPlatform(string appId, string videoAdId, string interAdId)
    {
        this.miniAppid = appId ?? "";
        this.videoAdId = videoAdId;
        this.interAdId = interAdId;
        if (string.IsNullOrEmpty(this.miniAppid))
        {
            // AppID 只用于 ECPM/转化回传，广告本身按广告位 ID 播放，不影响收益流程。
            Debug.Log("[抖音广告] 未填写小游戏 AppID，广告可正常播放，仅埋点上报不可用");
        }
        else if (EnableRemoteTracking && !this.miniAppid.EndsWith("07"))
        {
            // 只有开启回传时才需要 AppID 格式校验，否则这条警告纯属噪音
            Debug.LogWarning($"[抖音广告] AppID「{this.miniAppid}」格式可能不匹配回传接口，广告播放不受影响");
        }

        if (EnableRemoteTracking)
        {
            try
            {
                this.Login();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[抖音广告] 埋点初始化失败（不影响广告）：{e.Message}");
            }
        }

        // 注意：TT.InitSDK 是异步的，构造函数执行时抖音广告模块还没起来。
        // 以前在这里直接 CreateInterstitialAd()，会稳定拿到 createInterstitialAd:fail（code=0、errMsg 为空），
        // 随后 Load() 又抛 Uncaught (in promise) API_ERROR。
        // 现在广告实例一律等 AdManager 回调 OnSdkReady() 之后再创建。
        this.platformCreateTime = Time.realtimeSinceStartup;
        this.moduleStartTime = this.platformCreateTime;
        this.lastShowTime = -InterShowInterval;
    }

    /// <summary>
    /// 由 AdManager 在 TT.InitSDK 回调（或 3 秒兜底）后调用：此时才能安全创建广告实例。
    /// </summary>
    public void OnSdkReady()
    {
        if (this.sdkReady)
        {
            return;
        }

        this.sdkReady = true;
        // 频控从「广告模块真正启动」算起，而不是从构造函数算起
        this.moduleStartTime = Time.realtimeSinceStartup;
        Debug.Log("[抖音广告] SDK 就绪，开始预创建广告实例");

        try
        {
            if (this.rewardedVideoAd == null)
            {
                this.CreateRewardedVideoAd();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 预创建激励视频失败（点击时会重建）：{e.Message}");
        }

        try
        {
            this.CreateInterstitialAd();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 预创建插屏失败（下次打开设置时重试）：{e.Message}");
        }
    }

    /// <summary>
    /// 广告模块是否可用了。宿主不回调 InitSDK 时 5 秒后惰性放行，避免广告永久不可用。
    /// </summary>
    private bool IsSdkUsable()
    {
        if (Application.isEditor)
        {
            this.sdkReady = true;
            return true;
        }

        if (this.sdkReady)
        {
            return true;
        }

        if (Time.realtimeSinceStartup - this.platformCreateTime < 5f)
        {
            return false;
        }

        this.sdkReady = true;
        this.moduleStartTime = Time.realtimeSinceStartup;
        return true;
    }


    #region 插屏广告

    /// 插屏广告。
    /// 频控以开放平台文档为准：启动后 30 秒内不展示、两次插屏间隔 60 秒、激励视频后 60 秒内不展示插屏。
    /// 实例只能展示一次，关闭/失败后由平台销毁，下次展示时按需重建。
    /// 错误码参考 https://microapp.bytedance.com/docs/zh-CN/mini-game/develop/open-capacity/ads/tt-create-interstitial-ad
    public override void ShowInterstitialAd(string AdId)
    {
        // 广告位变化时丢弃旧实例，避免继续使用旧广告位
        if (!string.IsNullOrEmpty(AdId) && this.interAdId != AdId)
        {
            this.interAdId = AdId;
            this.DropInterstitialRef();
            this.interAdBroken = false;
            this.interAdFailCount = 0;
        }

        if (this.interAdBroken)
        {
            Debug.Log("[插屏] 广告位已判定不可用，本次运行跳过（原因见启动时的错误日志）");
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now - this.moduleStartTime < InterStartupCooldown)
        {
            Debug.Log($"[插屏] 启动 30 秒内不展示插屏，还需 {InterStartupCooldown - (now - this.moduleStartTime):F0} 秒");
            return;
        }

        if (now - this.lastShowTime < InterShowInterval)
        {
            Debug.Log($"[插屏] 两次广告间隔不足 60 秒，还需 {InterShowInterval - (now - this.lastShowTime):F0} 秒");
            return;
        }

        if (now - this.lastRewardCloseTime < InterShowInterval)
        {
            Debug.Log("[插屏] 激励视频结束后 60 秒内不展示插屏");
            return;
        }

        if (!this.IsSdkUsable())
        {
            Debug.Log("[插屏] 广告模块尚未初始化完成，本次跳过");
            return;
        }

        if (this.interAdIns == null)
        {
            // 首次或上次展示后没有实例：创建 + 加载，加载完成再自动展示
            this.interPendingShow = true;
            this.CreateInterstitialAd();
            return;
        }

        bool loaded = false;
        try
        {
            loaded = this.interAdIns.IsLoaded();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 查询插屏状态异常：{e.Message}");
        }

        if (!loaded)
        {
            if (this.interAdLoading && now - this.interAdLoadStartTime > 10f)
            {
                Debug.LogWarning("[插屏] 加载超时，重建一次");
                this.DropInterstitialRef();
                this.interPendingShow = true;
                this.CreateInterstitialAd();
            }
            else
            {
                // 还在加载：等 OnLoad 回来后自动展示，避免“点了没反应”
                this.interPendingShow = true;
                Debug.Log("[插屏] 正在加载，加载完成后自动展示");
            }

            return;
        }

        this.PresentInterstitialAd(this.interAdIns);
    }

    private void PresentInterstitialAd(TTInterstitialAd ad)
    {
        if (ad == null)
        {
            return;
        }

        try
        {
            ad.Show();
            this.lastShowTime = Time.realtimeSinceStartup;
            Debug.Log("[插屏] 已展示");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 展示插屏异常：{e.Message}");
            this.DropInterstitialRef();
        }
    }

    private void CreateInterstitialAd()
    {
        if (this.interAdIns != null || this.interAdBroken)
        {
            return;
        }

        if (string.IsNullOrEmpty(this.interAdId))
        {
            this.interAdBroken = true;
            Debug.LogError("[插屏] 广告位 ID 为空，已跳过");
            return;
        }

        if (!this.IsSdkUsable())
        {
            return;
        }

        try
        {
            TTInterstitialAd ad = TT.CreateInterstitialAd(new CreateInterstitialAdParam { InterstitialAdId = this.interAdId });
            this.interAdIns = ad;
            this.interAdLoading = true;
            this.interAdLoadStartTime = Time.realtimeSinceStartup;

            ad.OnLoad += () =>
            {
                // 换广告位重建后，旧实例的回调不能影响新实例
                if (this.interAdIns != ad)
                {
                    return;
                }

                this.interAdLoading = false;
                Debug.Log("[插屏] 加载完成");

                if (!this.interPendingShow)
                {
                    return;
                }

                this.interPendingShow = false;
                this.PresentInterstitialAd(ad);
            };

            ad.OnError += (code, message) =>
            {
                if (this.interAdIns != ad)
                {
                    return;
                }

                this.interAdLoading = false;
                // 只清引用：平台会自行销毁，这里再 Destroy 会和还没结束的 load Promise 撞出
                // Uncaught (in promise) API_ERROR
                this.interAdIns = null;
                this.interPendingShow = false;
                this.interAdFailCount++;

                // 158902 广告位不存在/已关闭、158987 宿主不支持、1002 广告位不属于当前小游戏；
                // code=0 且 errMsg 只有 createInterstitialAd:fail 基本都是 appid 与广告位不匹配
                if (code == 158902 || code == 158987 || code == 1002 || this.interAdFailCount >= 2)
                {
                    this.interAdBroken = true;
                }

                Debug.LogError($"[插屏] 创建/加载失败 code={code} msg={message} 广告位={this.interAdId}。"
                               + "请核对：① 广告位属于当前小游戏 appid；② 类型是「插屏」而不是「激励视频」；③ 后台已启用。");
            };

            ad.OnClose += () =>
            {
                if (this.interAdIns != ad)
                {
                    return;
                }

                // 插屏实例只能用一次，关闭后平台会销毁，这里只清引用，下次展示时再创建
                this.interAdIns = null;
                this.interAdLoading = false;
                Debug.Log("[插屏] 已关闭");
                this.SendGameAddiction();
            };

            ad.Load();
        }
        catch (Exception e)
        {
            this.interAdIns = null;
            this.interAdLoading = false;
            this.interAdBroken = true;
            Debug.LogError($"[插屏] 创建实例失败：{e.Message}");
        }
    }

    /// <summary>只断开引用，不主动 Destroy（Destroy 会撞上未结束的 load Promise）。</summary>
    private void DropInterstitialRef()
    {
        this.interAdIns = null;
        this.interAdLoading = false;
        this.interPendingShow = false;
    }

    #endregion


    #region 侧边栏

    public override void NavigateToScene(Action onSuccess, Action<string> onFail)
    {
        var data = new JsonData { ["scene"] = "sidebar" };
        TT.NavigateToScene(data,
            () =>
            {
                Debug.Log("侧边栏唤起成功");
                onSuccess?.Invoke();
            },
            () => { },
            (errCode, errMsg) =>
            {
                onFail?.Invoke(errMsg);
                Debug.LogError($"侧边栏显示异常:{errCode}, {errMsg}");
            });
    }

    #endregion

    #region 激励视频
    public override void ShowRewardedVideoAd(string videoAdId, Action onSuccess, Action<string> onFail)
    {
        Debug.Log("播放激励视频广告");
        this.prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0; // 看广告需要暂停游戏

        // 广告位发生变化时必须重建实例，否则会继续播旧的广告位
        bool needRecreate = rewardedVideoAd == null || this.videoAdId != videoAdId;
        this.videoAdId = videoAdId;
        onRewardSuccess = onSuccess;
        onRewardFail = onFail;

        if (needRecreate)
        {
            this.DelRewardedVideoAd();
            try
            {
                this.CreateRewardedVideoAd();
            }
            catch (Exception e)
            {
                Debug.LogError($"[抖音广告] 创建激励视频异常：{e.Message}");
            }
        }

        if (rewardedVideoAd == null)
        {
            // 创建失败必须恢复暂停，否则游戏会一直卡在 timeScale=0
            var fail = onRewardFail;
            this.RestoreTimeScale();
            fail?.Invoke("激励视频创建失败");
            return;
        }

        try
        {
            rewardedVideoAd.Show();
        }
        catch (Exception e)
        {
            Debug.LogError($"[抖音广告] 展示激励视频异常：{e.Message}");
            var fail = onRewardFail;
            this.RestoreTimeScale();
            fail?.Invoke("激励视频展示失败");
        }
    }

    private void RestoreTimeScale()
    {
        onRewardSuccess = null;
        onRewardFail = null;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
    }

    private void CreateRewardedVideoAd()
    {
        rewardedVideoAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam() { AdUnitId = videoAdId });
        rewardedVideoAd.OnClose += (ended, count) =>
        {
            // 激励视频结束后 60 秒内不允许再展示插屏（开放平台频控）
            this.lastRewardCloseTime = Time.realtimeSinceStartup;
            Debug.Log($"是否看完广告:{ended} 次数:{count}");
            var success = onRewardSuccess;
            var fail = onRewardFail;
            this.RestoreTimeScale();
            if (ended)
            {
                success?.Invoke();
                this.SendGameAddiction();
            }
            else
            {
                Debug.Log("广告未看完，不发放奖励");
                fail?.Invoke("广告未看完");
            }

            // 关闭广告后,创建新的实例（失败也不影响下次点击，下次会自动重建）
            this.DelRewardedVideoAd();
            try { this.CreateRewardedVideoAd(); }
            catch (Exception e) { Debug.LogWarning($"[抖音广告] 预创建激励视频失败：{e.Message}"); }
        };
        rewardedVideoAd.OnError += (errorCode, errorMessage) =>
        {
            this.DelRewardedVideoAd();
            var fail = onRewardFail;
            this.RestoreTimeScale();
            Debug.LogError($"激励视频错误 errorCode: {errorCode}, message: {errorMessage}");
            fail?.Invoke(string.IsNullOrEmpty(errorMessage) ? $"广告加载失败（{errorCode}）" : errorMessage);
            // 不在这里重建实例，避免广告位无效时反复创建；下次点击时 needRecreate 会自动重建
        };
    }

    private void DelRewardedVideoAd()
    {
        rewardedVideoAd?.Destroy();
        rewardedVideoAd = null;
    }

    #endregion


    #region 网络请求兜底清理

    /// <summary>
    /// 编辑器里脚本热重载时，正在进行的 UnityWebRequest 会被打断但不会被释放，
    /// 等它在新的 AppDomain 里完成时，原生层去释放旧域的 GCHandle 就会报
    /// 「Release of invalid GC handle. The handle is from a previous domain」。
    /// 这里在重载前主动 Abort + Dispose，把这个残留清干净。
    /// </summary>
#if UNITY_EDITOR
    private static readonly List<UnityWebRequest> activeRequests = new List<UnityWebRequest>();

    [UnityEditor.InitializeOnLoadMethod]
    private static void RegisterReloadCleanup()
    {
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= AbortActiveRequests;
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += AbortActiveRequests;
    }

    private static void AbortActiveRequests()
    {
        if (activeRequests.Count == 0)
        {
            return;
        }

        for (int i = activeRequests.Count - 1; i >= 0; i--)
        {
            UnityWebRequest request = activeRequests[i];
            try { request?.Abort(); } catch { }
            try { request?.Dispose(); } catch { }
        }

        activeRequests.Clear();
    }
#endif

    private static void TrackRequest(UnityWebRequest request)
    {
#if UNITY_EDITOR
        if (request != null)
        {
            activeRequests.Add(request);
        }
#endif
    }

    private static void UntrackRequest(UnityWebRequest request)
    {
#if UNITY_EDITOR
        if (request != null)
        {
            activeRequests.Remove(request);
        }
#endif
    }

    #endregion

    #region 埋点

    private void Login()
    {
        // 1.获取系统信息
        systemInfo = TT.GetSystemInfo();
        if (systemInfo?.system == null)
            return;

        // 2.登录-获取临时登录凭证 
        TT.Login(this.onLoginSuccess, (errMsg) =>
        {
            Debug.LogError("登录失败:" + errMsg);
        }, true);
        // 3.初始化埋点
        this.InitMaidian();
    }

    private void onLoginSuccess(string code, string anonymousCode, bool isLogin)
    {
        // 2.1获取用户的基本信息
        TT.GetUserInfo((ref TTUserInfo scUserInfo) => this.onGetUserInfo(ref scUserInfo, code, anonymousCode),
            (msg) =>
            {
                Debug.LogError($"用户信息获取失败: {msg}");
            });
    }

    private void onGetUserInfo(ref TTUserInfo scUserInfo, string code, string anonymousCode)
    {
        // 2.2向ECPM后台上传设备信息
        Debug.Log(scUserInfo);
        AdManager.Instance.StartCoroutine(this.SendOpenIdLog(
            miniAppid, scUserInfo.nickName, code, anonymousCode, systemInfo, (success, res) =>
            {
                if (success && res.d != null && res.d.user != null && !string.IsNullOrEmpty(res.d.user.openid))
                {
                    string useropenid = res.d.user.openid;
                    Debug.Log("openidlog==succ== " + res);
                    string currentDate = DateTime.UtcNow.ToString("o");
                    PlayerPrefs.SetString(miniAppid + TT.GetSystemInfo().model, currentDate);
                }
                else
                {
                    Debug.Log("openidlog==fail== " + res);
                }
            }));
    }

    private IEnumerator SendOpenIdLog(string appid, string nickName, string code, string anonymousCode, TTSystemInfo sysinfores, Action<bool, OpenIdResponse> callback)
    {
#if UNITY_EDITOR
        // 编辑器里没有真实登录态，上报没有意义；而且请求一旦跨脚本热重载就会留下无效 GCHandle
        Debug.Log("[抖音埋点] 编辑器环境跳过 openid 上报");
        callback?.Invoke(false, null);
        yield break;
#else
        var query = $"?appid={Uri.EscapeDataString(appid)}" +
                    $"&nickName={Uri.EscapeDataString(nickName)}" +
                    $"&code={Uri.EscapeDataString(code)}" +
                    $"&anonymousCode={Uri.EscapeDataString(anonymousCode)}" +
                    $"&system={Uri.EscapeDataString(sysinfores.system)}" +
                    $"&platform={Uri.EscapeDataString(sysinfores.platform)}" +
                    $"&brand={Uri.EscapeDataString(sysinfores.brand)}" +
                    $"&modelx={Uri.EscapeDataString(sysinfores.model)}" +
                    $"&version={Uri.EscapeDataString(sysinfores.ttVersion)}" +
                    $"&SDKVersion={Uri.EscapeDataString(sysinfores.sdkVersion)}" +
                    $"&appName={Uri.EscapeDataString(sysinfores.hostName)}";

        var www = UnityWebRequest.Get(this.ecpmURL + query);
        TrackRequest(www);
        try
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("openidlog==fail== " + www.error);
                callback?.Invoke(false, null);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<OpenIdResponse>(www.downloadHandler.text);
                    callback?.Invoke(true, response);
                }
                catch (Exception ex)
                {
                    Debug.LogError("SendOpenIdLog ERROR：" + ex.Message);
                    callback?.Invoke(false, null);
                }
            }
        }
        finally
        {
            UntrackRequest(www);
            www.Dispose();
        }
#endif
    }

    private void InitMaidian()
    {
        // 3.1获取小游戏启动时的参数
        var launchOptions = TT.GetLaunchOptionsSync();
        Debug.Log($"小游戏启动参数: {launchOptions}");
        if (launchOptions == null || launchOptions.Query == null || !launchOptions.Query.ContainsKey("ad_params"))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没添加埋点参数");
            return;
        }

        string decodedString = Uri.UnescapeDataString(launchOptions.Query["ad_params"]);
        AdParamsData jsonval = JsonUtility.FromJson<AdParamsData>(decodedString);
        string clickid = jsonval?.log_extra?.clickid;
        if (string.IsNullOrEmpty(clickid))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没点击事件埋点");
            return;
        }

        Debug.Log($"clickid: {clickid}");
        PlayerPrefs.SetString(miniAppid + "ClickId", clickid);
        if (!CanClaimReward(miniAppid + "Active"))
        {
            Debug.LogWarning($"小游戏:{miniAppid}奖励已领取，无法重复领取");
            return;
        }

        // 3.2 API上报激活数据
        long timenow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        AdManager.Instance.StartCoroutine(SendConversionRequest(
            "active", clickid, timenow,
            (success, response) =>
            {
                Debug.Log($"激活埋点事件==={(success ? "success" : "fail")}");
                Debug.Log($"激活结果={response}");
            }
        ));
        string currentDate = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(miniAppid + "Active", currentDate);
    }

    /*是否允许领取奖励*/
    private bool CanClaimReward(string key)
    {
        string lastClaimDateStr = PlayerPrefs.GetString(miniAppid + key);
        if (string.IsNullOrEmpty(lastClaimDateStr))
        {
            return true;
        }

        if (!DateTime.TryParse(lastClaimDateStr, out DateTime lastClaimDate))
        {
            return true;
        }
        DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
        return lastClaimDate < oneMonthAgo;
    }

    /// <summary>
    /// API上报关键行为
    /// </summary>
    private void SendGameAddiction()
    {
        string clickid = PlayerPrefs.GetString(miniAppid + "ClickId");
        if (string.IsNullOrEmpty(clickid))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没点击事件埋点");
            return;
        }

        long timenow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        AdManager.Instance.StartCoroutine(SendConversionRequest(
                "game_addiction", clickid, timenow,
                (success, response) =>
                {
                    Debug.Log($"埋点事件===={(success ? "success" : "fail")}");
                    Debug.Log($"关键行为结果={response}");
                }
            ));
        Debug.Log("已触发激活埋点");
    }

    /// <summary>
    /// 转化回传 API
    /// </summary>
    /// <param name="eventType">回传的事件：</param>
    /// <param name="clickid">clickid 参数</param>
    /// <param name="timestamp">事件发生的毫秒级时间戳</param>
    /// <param name="callback"></param>
    /// https://event-manager.oceanengine.com/docs/8650/app_api_docs
    /// <returns></returns>
    public IEnumerator SendConversionRequest(string eventType, string clickid, long timestamp, Action<bool, string> callback)
    {
#if UNITY_EDITOR
        // 编辑器里 clickid 是假的，回传本身没有意义；
        // 更重要的是 UnityWebRequest + UploadHandlerRaw 一旦跨脚本热重载，
        // 原生层释放旧域的 GCHandle 就会报 "Release of invalid GC handle"
        Debug.Log($"[抖音埋点] 编辑器环境跳过转化回传：{eventType}");
        callback?.Invoke(false, "editor 环境跳过");
        yield break;
#else
        var data = new ConversionData
        {
            event_type = eventType,
            context = new Context { ad = new Ad { callback = clickid } },
            timestamp = timestamp
        };

        string json = JsonUtility.ToJson(data);
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(json);

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");

        var www = new UnityWebRequest(conversionURL, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        foreach (var header in headers)
        {
            www.SetRequestHeader(header.Key, header.Value);
        }

        // 注册到统一表里：编辑器热重载前会被 Abort + Dispose，
        // 避免 UploadHandlerRaw 持有的 byte[] GCHandle 在旧域失效后还被释放
        TrackRequest(www);
        try
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                callback?.Invoke(false, www.error);
            }
            else
            {
                callback?.Invoke(true, www.downloadHandler.text);
            }
        }
        finally
        {
            UntrackRequest(www);
            www.Dispose();
        }
#endif
    }
    public override void ShareGame(Action<bool> callBack)
    {
        // 通用分享
        JsonData shareJson = new JsonData();
        shareJson["channel"] = "video";
        shareJson["title"] = "Some Title";
        shareJson["extra"] = new JsonData();
        shareJson["extra"]["videoPath"] = "/xxx/xxx.mp4";//录屏分享的话，路径是 OnRecordComplete 拿到的路径
        JsonData videoTopics = new JsonData();
        videoTopics.SetJsonType(JsonType.Array);
        videoTopics.Add("Some Topic1");
        videoTopics.Add("Some Topic2");
        shareJson["extra"]["videoTopics"] = videoTopics;
        shareJson["extra"]["hashtag_list"] = videoTopics;

        Debug.Log($"ShareAppMessageBtnClicked jsonData: {videoTopics.ToJson()}");
        TT.ShareAppMessage(videoTopics, (data) =>
        {
            callBack?.Invoke(true);
            Debug.Log($"ShareAppMessage success: {videoTopics.ToJson()}");//
        },
            (errMsg) =>
            {
                callBack?.Invoke(false);
                Debug.Log($"ShareAppMessage failed: {errMsg}");
            },
            () =>
            {
                callBack?.Invoke(false);
                Debug.Log($"ShareAppMessage cancel");
            });
    }

    #endregion
}

//#endif