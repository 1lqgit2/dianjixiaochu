using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoreBoosterView : BaseView
{
    public enum POPUP_TYPE
    {
        BOMB,
        HAMMER
    }

    public POPUP_TYPE currentType;

    public GameObject hammerIcon;

    public GameObject bombIcon;

    public Text boosterTitle;

    public Text boosterDes;

    public Text moreBoosterValue;

    public Text priceTxt;

    public override void InitView()
    {
       
    }

    public void InitView(POPUP_TYPE type)
    {
        currentType = type;
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public void BuyBooster()
    {
        AudioManager.instance.clickSound.Play();

        if (currentType == POPUP_TYPE.BOMB)
        {
            if (GameManager.instance.currentCoin >= 350)
            {
                GameManager.instance.SubCoin(350);
                GameManager.instance.AddBomb(3);
                GameManager.instance.uiManager.gameView.bombBtn.SetUnLock();
                HideView();
            }
            else
                MoreOffer();
        }
        else if (currentType == POPUP_TYPE.HAMMER)
        {
            if (GameManager.instance.currentCoin >= 200)
            {
                GameManager.instance.SubCoin(200);
                GameManager.instance.AddHammer(3);
                GameManager.instance.uiManager.gameView.hammerBtn.SetUnLock();
                HideView();
            }
            else
                MoreOffer();
        }
    }

    public void MoreOffer()
    {
        AudioManager.instance.clickSound.Play();
        HideView();
        GameManager.instance.uiManager.moreCoinView.ShowView();
    }

    private GameManager.GAME_STATE lastState;

    public override void ShowView()
    {
        base.ShowView();
        lastState = GameManager.instance.currentGameState;
        GameManager.instance.currentGameState = GameManager.GAME_STATE.POP_UP;

        if (currentType == POPUP_TYPE.BOMB)
        {
            boosterTitle.text = "炸弹";
            boosterDes.text = "消除周围的若干方块";
            moreBoosterValue.text = "3";
            priceTxt.text = "350";
            bombIcon.SetActive(true);
            hammerIcon.SetActive(false);
        }
        else if (currentType == POPUP_TYPE.HAMMER)
        {
            boosterTitle.text = "锤子";
            boosterDes.text = "敲碎地图上的一个方块";
            moreBoosterValue.text = "3";
            priceTxt.text = "200";
            bombIcon.SetActive(false);
            hammerIcon.SetActive(true);
        }
    }

    public override void HideView()
    {
        AudioManager.instance.clickSound.Play();
        base.HideView();
        GameManager.instance.currentGameState = lastState;
    }
}
