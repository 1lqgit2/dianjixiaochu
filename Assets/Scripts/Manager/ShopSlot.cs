using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public GameObject selectBg;

    public Image icon;

    public GameObject checkMark;

    public GameObject quesMark;

    public GameObject buyButton;

    public int itemIndex;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLock(bool state)
    {
        if (state)
        {
            selectBg.SetActive(false);
            icon.gameObject.SetActive(false);
            checkMark.SetActive(false);
            quesMark.SetActive(true);
            buyButton.SetActive(true);
        }
        else
        {
            selectBg.SetActive(true);
            icon.gameObject.SetActive(true);
            checkMark.SetActive(true);
            quesMark.SetActive(false);
            buyButton.SetActive(false);
        }
    }


    public void Select(bool state)
    {
        if (state)
        {
            selectBg.SetActive(true);
            icon.gameObject.SetActive(true);
            checkMark.SetActive(true);
            quesMark.SetActive(true);
            buyButton.SetActive(false);
        }
        else
        {
            selectBg.SetActive(false);
            icon.gameObject.SetActive(true);
            checkMark.SetActive(false);
            quesMark.SetActive(false);
            buyButton.SetActive(false);
        }
    }

    public void Unlock()
    {
        ShowRewardAds();
    }

    public void Choose()
    {
        if (PlayerPrefs.GetInt("Shop" + itemIndex) == 0)
            return;

        GameManager.instance.uiManager.skinView.Select(this);
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
        AudioManager.instance.coinSound.Play();
        PlayerPrefs.SetInt("Shop" + itemIndex, 1);
        SetLock(false);
        GameManager.instance.uiManager.skinView.Refresh();
    }
}
