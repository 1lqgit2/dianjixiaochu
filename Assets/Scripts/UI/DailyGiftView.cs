using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyGiftView : BaseView
{
    public DailySlot[] slots;

    public RewardModel[] rewardArr;

    public override void InitView()
    {
        if (slots == null || slots.Length == 0) return;

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
        if (dailyKey < 1) dailyKey = 1;

        int total = slots.Length;

        // 以 7 天为一轮：超过一轮后自动进入下一轮，避免所有格子都变成“已过期”
        long cycle = (dailyKey - 1) / total;
        if (PlayerPrefs.GetInt("DailyCycle", 0) != (int)cycle)
        {
            PlayerPrefs.SetInt("DailyCycle", (int)cycle);
            for (int i = 0; i < total; i++)
            {
                PlayerPrefs.SetInt("Daily" + (i + 1).ToString(), 0);
            }
            PlayerPrefs.Save();
        }

        int todayInCycle = (int)((dailyKey - 1) % total) + 1;

        for (int i = 0; i < total; i++)
        {
            if (slots[i] == null) continue;

            slots[i].itemIndex = i;

            slots[i].Refresh();

            if (rewardArr != null && i < rewardArr.Length)
            {
                slots[i].rwModel = rewardArr[i];
            }

            bool claimed = PlayerPrefs.GetInt("Daily" + (i + 1).ToString()) == 1;

            if (claimed)
            {
                // 已领取
                slots[i].FinishItem();
            }
            else if (todayInCycle == (i + 1))
            {
                // 今天可领
                slots[i].EnableItem();
            }
            else if (todayInCycle > (i + 1))
            {
                // 漏签：这天已经过去但没领，仍然要把格子显示出来（置灰）
                slots[i].ShowMissed();
            }
            else
            {
                // 未来某天
                slots[i].DisableItem();
            }
        }
    }

    /// <summary>每次打开都重新算一次状态，领完当天奖励后不用重启游戏也能解锁下一天。</summary>
    public override void ShowView()
    {
        InitView();
        base.ShowView();
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public override void HideView()
    {
        AudioManager.instance.clickSound.Play();
        base.HideView();
    }


}
