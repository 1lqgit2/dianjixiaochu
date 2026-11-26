using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinView : BaseView
{
    public GameObject winVfx;

    public CanvasGroup finalTxt;

    public CanvasGroup levelReward;

    public CanvasGroup bonusBar;

    public CanvasGroup doubleGetBtn;

    public CanvasGroup normalGetBtn;

    public Text resultRewardTxt;

    public Text normalRewardTxt;

    public Text multiRewardTxt;

    private int coinReward;

    private int multiCoin;

    /// <summary>上一次写进 multiRewardTxt 的数值，避免每帧重复赋值触发 UI 重建。</summary>
    private int lastRewardShown = -1;

    public RectTransform rwPicker;

    private void Awake()
    {
        rwPicker.DOLocalMoveX(500.0f, 1.0f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    public override void InitView()
    {
        AudioManager.instance.winSound.Play();
        coinReward = (GameManager.instance.levelIndex / 10 + 1) * 5;
        winVfx.SetActive(true);
        resultRewardTxt.text = "+" + coinReward.ToString();
        normalRewardTxt.text = coinReward.ToString();
        // 面板刚弹出时先按滑块当前位置算一次倍率，否则这里会显示 0
        this.lastRewardShown = -1;
        this.RefreshMultiReward();
    }

    public override void ShowView()
    {
        base.ShowView();
        finalTxt.alpha = 0.0f;
        levelReward.alpha = 0.0f;
        bonusBar.alpha = 0.0f;
        doubleGetBtn.alpha = 0.0f;
        doubleGetBtn.alpha = 0.0f;
        normalGetBtn.alpha = 0.0f;


        finalTxt.DOFade(1.0f, 0.25f).SetDelay(2.0f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {


        levelReward.DOFade(1.0f, 0.25f).SetEase(Ease.Linear)
          .OnComplete(() =>
          {

        bonusBar.DOFade(1.0f, 0.25f).SetEase(Ease.Linear)
         .OnComplete(() =>
         {

        doubleGetBtn.DOFade(1.0f, 0.5f).SetEase(Ease.Linear)
        .OnComplete(() =>
        {

       normalGetBtn.DOFade(1.0f, 0.5f).SetEase(Ease.Linear)
      .OnComplete(() =>
      {
          GameManager.instance.levelIndex++;
          PlayerPrefs.SetInt("LevelIndex", GameManager.instance.levelIndex);

      });

        });

         });

          });

           });

    }

    public override void Start()
    {

    }

    public override void Update()
    {
        // 注意：BaseView 隐藏面板只是把 CanvasGroup.alpha 拉到 0，GameObject 一直是激活的，
        // 所以 Update 整局游戏每帧都在跑。这里必须先判断 isShow，
        // 否则会（1）在后台每帧刷奖励文本，（2）刷出满屏 “MUL : 0” 的日志。
        if (!isShow)
        {
            return;
        }

        this.RefreshMultiReward();
    }

    /// <summary>
    /// 按滑块位置换算倍率并刷新文本。值没变就不碰 UI，避免每帧触发 UGUI 重建。
    /// </summary>
    private void RefreshMultiReward()
    {
        if (rwPicker == null || multiRewardTxt == null)
        {
            return;
        }

        float offset = Mathf.Abs(rwPicker.localPosition.x) / 50f;
        if (offset < 1f)
        {
            multiCoin = 5;
        }
        else if (offset < 3f)
        {
            multiCoin = 3;
        }
        else if (offset < 5f)
        {
            multiCoin = 2;
        }
        else
        {
            // 滑块停在两侧空白区：保持上一次的倍率，第一次没有值时按最低档算
            multiCoin = multiCoin > 0 ? multiCoin : 2;
        }

        int total = multiCoin * coinReward;
        if (total == this.lastRewardShown)
        {
            return;
        }

        this.lastRewardShown = total;
        multiRewardTxt.text = total.ToString();
    }

    public override void HideView()
    {
        winVfx.SetActive(false);
        base.HideView();
    }

    public void BackToHome()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.gameView.HideView();
        HideView();
        GameManager.instance.AddCoin(coinReward);
        GameManager.instance.BackToHome();
    }

    public void MultiCoin()
    {
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
        AudioManager.instance.clickSound.Play();
        GameManager.instance.uiManager.gameView.HideView();
        HideView();
        GameManager.instance.AddCoin(coinReward * multiCoin);
        GameManager.instance.BackToHome();
    }
}
