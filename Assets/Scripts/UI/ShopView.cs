using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopView : BaseView
{
    public Text freeAdsTimerTxt;

    public GameObject getBtn;

    public GameObject timerTxt;

    public override void InitView()
    {

    }

    public override void Start()
    {

    }

    public override void Update()
    {

    }

    private GameManager.GAME_STATE lastState;

    public override void ShowView()
    {
        base.ShowView();
        lastState = GameManager.instance.currentGameState;
        GameManager.instance.currentGameState = GameManager.GAME_STATE.POP_UP;
    }

    public override void HideView()
    {
        AudioManager.instance.clickSound.Play();
        base.HideView();
        GameManager.instance.currentGameState = lastState;
    }

    public void UpdateFreeAdsTxt()
    {
        // freeAdsValueTxt.text = GameManager.Instance.freeAdsTimer.LivesText;
        if (GameManager.instance.freeAdsTimer.freeAdsNumber > 0)
        {
            getBtn.SetActive(true);
            timerTxt.SetActive(false);
        }
        else
        {
            getBtn.SetActive(false);
            timerTxt.SetActive(true);
        }
    }

    public void UpdateFreeAdsTimerTxt()
    {
        freeAdsTimerTxt.text = GameManager.instance.freeAdsTimer.RemainingTimeString;

    }

    public void FreeCoin()
    {
        AudioManager.instance.clickSound.Play();

        if (GameManager.instance.freeAdsTimer.freeAdsNumber <= 0)
        {
            return;
        }

        if (AdsControl.Instance == null)
        {
            return;
        }

        AdsControl.Instance.ShowRewardAd(EarnFreeCoin, error => Debug.LogWarning("[广告] 激励视频未完成：" + error));
    }

    public void EarnFreeCoin()
    {
        GameManager.instance.freeAdsTimer.ConsumeLife();
        GameManager.instance.AddCoin(150);
        HideView();
    }
    

    public void BuyIAPPackage(Config.IAPPackageID packageID)
    {
        AudioManager.instance.clickSound.Play();

        IAPManager.instance.BuyConsumable(packageID, (string iapID, IAPManager.IAP_CALLBACK_STATE state) =>
        {
            if (state == IAPManager.IAP_CALLBACK_STATE.SUCCESS)
            {

                Debug.Log("SUCCESSSUCCESS " + iapID);

                if (iapID.Equals(Config.IAPPackageID.NoAds.ToString()))
                {
                    Debug.Log("REMOVE ADS");
                    AdsControl.Instance.RemoveAds();
                }
                else
                {
                    BuySuccesss(packageID);
                }
            }
            else
            {
                Debug.Log("Buy Fail!");

            }
        });



    }

    public void BuySuccesss(Config.IAPPackageID packageID)
    {
        switch (packageID)
        {
            case Config.IAPPackageID.started_bundle:
                GameManager.instance.AddCoin(400);
                GameManager.instance.AddBomb(6);
                GameManager.instance.AddHammer(6);
                break;

            case Config.IAPPackageID.advanced_bundle:
                GameManager.instance.AddCoin(800);
                GameManager.instance.AddBomb(15);
                GameManager.instance.AddHammer(15);
                break;

            case Config.IAPPackageID.master_bundle:
                GameManager.instance.AddCoin(1500);
                GameManager.instance.AddBomb(30);
                GameManager.instance.AddHammer(30);
                break;

            case Config.IAPPackageID.ultimate_bundle:
                GameManager.instance.AddCoin(3000);
                GameManager.instance.AddBomb(70);
                GameManager.instance.AddHammer(70);
                break;

            case Config.IAPPackageID.NoAds:
                RemoveAds();
                break;

            
        }

        HideView();
    }

    public void RemoveAds()
    {
        AudioManager.instance.clickSound.Play();
        BuyIAPPackage(Config.IAPPackageID.NoAds);
        Debug.Log("Remove Ads");
    }

    public void Restore()
    {
        AudioManager.instance.clickSound.Play();
        IAPManager.instance.RestorePurchases();
        Debug.Log("Restore");
    }

    public void PurchaseItem(int index)
    {
        BuyIAPPackage((Config.IAPPackageID)index);

    }
}
