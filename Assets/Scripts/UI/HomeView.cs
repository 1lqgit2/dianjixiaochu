using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeView : BaseView
{
    public Text levelTxt;

    public override void InitView()
    {
        levelTxt.text = (GameManager.instance.levelIndex + 1).ToString();
    }

    public override void Start()
    {
       
    }

    public override void Update()
    {
        
    }

    public override void ShowView()
    {
        base.ShowView();
    }

    public override void HideView()
    {
        base.HideView();
    }

    public void PlayGame()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.PlayGame();
    }

    public void OpenShop()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.shopView.ShowView();
    }

    public void OpenSkin()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.skinView.ShowView();
    }

    public void OpenDaily()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.dailyGiftView.ShowView();
    }

    public void OpenSetting()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.settingView.ShowView();
    }

    public void RemoveAds()
    {
        AudioManager.instance.clickSound.Play();
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.shopView.BuyIAPPackage(Config.IAPPackageID.NoAds);
    }

    // ---- 两侧新增的两个广告按钮 ----

    /// <summary>「领金币」看一次激励视频给多少金币，想改数值改这里。</summary>
    private const int RewardCoinsPerAd = 100;

    /// <summary>cebianBtn「福利」：点击拉一次插屏。抖音侧有 15s 冷启动与 30s 间隔限制，不满足会被跳过。</summary>
    public void ShowSideAd()
    {
        AudioManager.instance.clickSound.Play();

        if (AdsControl.Instance == null)
        {
            Debug.LogWarning("[广告] AdsControl 未初始化，福利按钮不可用");
            return;
        }

        AdsControl.Instance.ShowInterstital();
    }

    /// <summary>jiliBtn「领金币」：看完激励视频加金币，中途关闭或加载失败不加。</summary>
    public void WatchAdForCoin()
    {
        AudioManager.instance.clickSound.Play();

        if (AdsControl.Instance == null)
        {
            Debug.LogWarning("[广告] AdsControl 未初始化，激励按钮不可用");
            return;
        }

        if (!AdsControl.Instance.IsRewardedVideoAvailable())
        {
            Debug.LogWarning("[广告] 激励视频暂不可用");
            return;
        }

        AdsControl.Instance.ShowRewardAd(
            () => GameManager.instance.AddCoin(RewardCoinsPerAd),
            error => Debug.LogWarning("[广告] 激励视频未完成：" + error));
    }
}
