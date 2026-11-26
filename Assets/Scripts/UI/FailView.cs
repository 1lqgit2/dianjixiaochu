using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailView : BaseView
{
    public GameObject vfx;

    public override void InitView()
    {
       
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
        vfx.SetActive(true);
    }

    public override void HideView()
    {
        base.HideView();
        vfx.SetActive(false);
    }

    public void Lose()
    {
        AudioManager.instance.clickSound.Play();
        HideView();
        GameManager.instance.Replay();
    }

    public void AddMovesByCoins()
    {
        AudioManager.instance.clickSound.Play();

        if(GameManager.instance.currentCoin >= 250)
        {
            GameManager.instance.SubCoin(250);
            GameManager.instance.boardGenerator.moves += 10;
            HideView();
            GameManager.instance.ResumeGame();
        }
        else
        {
            HideView();
            GameManager.instance.uiManager.moreCoinView.ShowView();
        }
       
    }

    public void AddMovesByAds()
    {
        AudioManager.instance.clickSound.Play();
        ShowRewardAds();
    }

    public void ShowRewardAds()
    {
        AudioManager.instance.clickSound.Play();

        if (AdsControl.Instance == null)
        {
            return;
        }

        AdsControl.Instance.ShowRewardAd(EarnReward, error => Debug.LogWarning("[广告] 激励视频未完成：" + error));
    }

    public void EarnReward()
    {
        GameManager.instance.boardGenerator.moves += 5;
        HideView();
        GameManager.instance.ResumeGame();
    }

}
