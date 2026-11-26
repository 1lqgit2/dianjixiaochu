using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailySlot : MonoBehaviour
{
    public GameObject layoutFuture;

    public GameObject layoutClaim;

    public GameObject layoutComplete;

    public Image giftIcon;

    public Text dayTxt;

    public Text rewardValueTxt;

    public int itemIndex;

    public RewardModel rwModel;

    private Color futureCardColor = Color.white;

    private Color dayTextColor = Color.white;

    private bool inited;

    private void Awake()
    {
        CacheOriginColor();
    }

    private void CacheOriginColor()
    {
        if (inited) return;
        inited = true;

        if (layoutFuture != null)
        {
            Image card = layoutFuture.GetComponent<Image>();
            if (card != null) futureCardColor = card.color;
        }

        if (dayTxt != null) dayTextColor = dayTxt.color;
    }

    public void Refresh()
    {
        //Debug.Log(itemIndex);
        if (dayTxt != null)
        {
            dayTxt.text = "第" + (itemIndex + 1).ToString() + "天";
        }
    }

    public void Claim()
    {
        AudioManager.instance.coinSound.Play();

        long firstDay = 0;

        if (PlayerPrefs.GetInt("CheckFirstDay") == 0)
        {
            firstDay = (long)(DateTime.Today.Subtract(new DateTime(2019, 1, 1))).TotalSeconds;
            PlayerPrefs.SetString("FirstDay", firstDay.ToString());
            PlayerPrefs.SetInt("CheckFirstDay", 1);
        }
        else
        {
            string firstDayStr = PlayerPrefs.GetString("FirstDay");
            long.TryParse(firstDayStr, out firstDay);
        }
        long dailyKey = ((long)(DateTime.Today.Subtract(new DateTime(2019, 1, 1))).TotalSeconds - firstDay) / (24 * 60 * 60) + 1;
        PlayerPrefs.SetInt("Daily" + (itemIndex + 1).ToString(), 1);
        GetReward();
        FinishItem();
    }

    private void GetReward()
    {
        if(rwModel.type == RewardModel.Reward_Type.COIN)
        {
            GameManager.instance.AddCoin(rwModel.total);
        }
        else if (rwModel.type == RewardModel.Reward_Type.HAMMER)
        {
            GameManager.instance.AddHammer(rwModel.total);
        }
        else if (rwModel.type == RewardModel.Reward_Type.BOMB)
        {
            GameManager.instance.AddBomb(rwModel.total);
        }
    }

    /// <summary>恢复格子原本的配色（避免漏签置灰状态残留到其它状态）。</summary>
    private void RestoreNormalColor()
    {
        CacheOriginColor();

        if (layoutFuture != null)
        {
            Image card = layoutFuture.GetComponent<Image>();
            if (card != null) card.color = futureCardColor;
        }

        if (dayTxt != null) dayTxt.color = dayTextColor;
    }

    /// <summary>可领取：显示领取按钮。</summary>
    public void EnableItem()
    {
        RestoreNormalColor();
        if (layoutFuture != null) layoutFuture.SetActive(false);
        if (layoutClaim != null) layoutClaim.SetActive(true);
        if (layoutComplete != null) layoutComplete.SetActive(false);
    }

    /// <summary>未到：显示紫色礼物卡（正常未解锁样子）。</summary>
    public void DisableItem()
    {
        RestoreNormalColor();
        if (layoutFuture != null) layoutFuture.SetActive(true);
        if (layoutClaim != null) layoutClaim.SetActive(false);
        if (layoutComplete != null) layoutComplete.SetActive(false);
    }

    /// <summary>漏签：当天已过但没领，仍然要显示格子（置灰），否则整格是空白。</summary>
    public void ShowMissed()
    {
        CacheOriginColor();

        if (layoutFuture != null) layoutFuture.SetActive(true);
        if (layoutClaim != null) layoutClaim.SetActive(false);
        if (layoutComplete != null) layoutComplete.SetActive(false);

        if (layoutFuture != null)
        {
            Image card = layoutFuture.GetComponent<Image>();
            if (card != null)
            {
                card.color = new Color(futureCardColor.r * 0.72f,
                                       futureCardColor.g * 0.72f,
                                       futureCardColor.b * 0.72f,
                                       futureCardColor.a);
            }
        }

        if (dayTxt != null)
        {
            dayTxt.color = new Color(1f, 1f, 1f, 0.55f);
        }
    }

    /// <summary>已领取：显示奖励图标、数量与对勾。</summary>
    public void FinishItem()
    {
        RestoreNormalColor();
        if (layoutFuture != null) layoutFuture.SetActive(false);
        if (layoutClaim != null) layoutClaim.SetActive(false);
        if (layoutComplete != null) layoutComplete.SetActive(true);

        if (rwModel != null)
        {
            if (giftIcon != null && rwModel.icon != null) giftIcon.sprite = rwModel.icon;
            if (rewardValueTxt != null) rewardValueTxt.text = "X" + rwModel.total;
        }
    }

}
