using System;
using UnityEngine;

/// <summary>
/// 支付入口占位实现。
/// 原实现基于 Unity IAP（Google Play / App Store），本工程只保留抖音渠道，
/// 已移除 com.unity.purchasing。这里保留全部对外 API 与回调签名，
/// 保证 ShopView / SettingView 等处编译与运行不崩，等接入抖音虚拟支付时
/// 在 BuyProductID 内部替换成 TT 的支付调用即可。
/// </summary>
public class IAPManager : MonoBehaviour
{
    public static IAPManager instance;

    public static string kProductIDConsumable = "consumable";
    public static string kProductIDNonConsumable = "nonconsumable";
    public static string kProductIDSubscription = "subscription";

    public enum IAP_CALLBACK_STATE
    {
        SUCCESS,
        FAIL
    }

    private Action<string, IAP_CALLBACK_STATE> PurchaserManager_Callback =
        delegate (string _iapID, IAP_CALLBACK_STATE _callBackState) { };

    public static Action InitializeSucceeded;
    public static event Action RestoreCompleted;
    public static event Action RestoreFailed;

    /// <summary>抖音虚拟支付是否已就绪。接入前恒为 false。</summary>
    private bool initialized = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (initialized)
        {
            return;
        }

        // TODO: 接入抖音虚拟支付时，在这里调用 TT 的支付初始化（打开/校验支付环境），
        //       成功后把 initialized 置 true 并触发 InitializeSucceeded。
        initialized = false;

        Debug.LogWarning("[支付] 当前为抖音渠道占位实现，未接入抖音虚拟支付，内购暂不可用。");
        InitializeSucceeded?.Invoke();
    }

    public bool IsInitialized()
    {
        return initialized;
    }

    public void BuyConsumable(Config.IAPPackageID iapID, Action<string, IAP_CALLBACK_STATE> _purchaserManager_Callback)
    {
        PurchaserManager_Callback = _purchaserManager_Callback;
        BuyProductID(iapID.ToString());
    }

    public void BuyNonConsumable(Config.IAPPackageID iapID, Action<string, IAP_CALLBACK_STATE> _purchaserManager_Callback)
    {
        PurchaserManager_Callback = _purchaserManager_Callback;
        BuyProductID(iapID.ToString());
    }

    public void BuyNonConsumable()
    {
        BuyProductID(kProductIDNonConsumable);
    }

    public void BuySubscription()
    {
        BuyProductID(kProductIDSubscription);
    }

    private void BuyProductID(string productId)
    {
        // TODO: 换成抖音虚拟支付的「下单 + 拉起支付」流程，
        //       支付成功回调里用 IAP_CALLBACK_STATE.SUCCESS 触发 PurchaserManager_Callback。
        Debug.LogWarning("[支付] 未接入抖音虚拟支付，跳过购买流程：" + productId);
        PurchaserManager_Callback.Invoke(productId, IAP_CALLBACK_STATE.FAIL);
    }

    public void RestorePurchases()
    {
        // 小游戏渠道没有「恢复购买」概念，这里只回报结果，不做任何事。
        Debug.Log("[支付] 抖音渠道无需恢复购买。");
        RestoreFailed?.Invoke();
    }

    /// <summary>商品价格展示文案。接入抖音虚拟支付后应替换为后台下发的价格。</summary>
    public string GetLocalizedPriceString(string iapID)
    {
        return "0.01$";
    }
}
