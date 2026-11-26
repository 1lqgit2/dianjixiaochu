using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class GameView : BaseView
{
    public Text levelTxt;

    public Text movesTxt;

    public BoosterButton bombBtn;

    public BoosterButton hammerBtn;

    public GameObject tutObj;

    public override void InitView()
    {
        if (GameManager.instance.levelIndex == 0)
        {
            tutObj.SetActive(true);
        }

        movesTxt.text = GameManager.instance.boardGenerator.moves.ToString() + " 步";
        levelTxt.text = (GameManager.instance.levelIndex + 1).ToString();

        if(GameManager.instance.levelIndex >= 2)
        {
            if (GameManager.instance.currentHammerBooster > 0)
                hammerBtn.SetUnLock();
            else
                hammerBtn.SetAdd();
        }
        else
        {
            hammerBtn.SetLock();
        }

        if (GameManager.instance.levelIndex >= 3)
        {
            if (GameManager.instance.currentBombBooster > 0)
                bombBtn.SetUnLock();
            else
                bombBtn.SetAdd();
        }
        else
        {
            bombBtn.SetLock();
        }
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public void HideTut()
    {
        if (GameManager.instance.levelIndex == 0)
        {
            tutObj.SetActive(false);
        }
    }

    public void UpdateMovesTxt()
    {
        movesTxt.text = GameManager.instance.boardGenerator.moves.ToString() + " 步";
    }

    public void BackToHome()
    {
        AudioManager.instance.clickSound.Play();
        HideView();
        GameManager.instance.BackToHome();
    }

    public void Replay()
    {
        AudioManager.instance.clickSound.Play();
        GameManager.instance.Replay();
    }

    public void ShowBoomBooster()
    {
        AudioManager.instance.clickSound.Play();

        if (bombBtn.currentState == BoosterButton.BOOSTER_STATE.LOCK)
            return;
        if (bombBtn.currentState == BoosterButton.BOOSTER_STATE.UNLOCK)
        {
            HideView();
            GameManager.instance.uiManager.boosterView.ShowView(BoosterView.BoosterType.BOMB);
            GameManager.instance.currentGameState = GAME_STATE.BOMB_BOOSTER;
            GameManager.instance.SubBomb();

            if (GameManager.instance.currentBombBooster == 0)
                bombBtn.SetAdd();
        }
        else if (bombBtn.currentState == BoosterButton.BOOSTER_STATE.ADD)
        {
            GameManager.instance.uiManager.moreBoosterView.InitView(MoreBoosterView.POPUP_TYPE.BOMB);
            GameManager.instance.uiManager.moreBoosterView.ShowView();
        }
    }

    public void ShowHammerBooster()
    {
        AudioManager.instance.clickSound.Play();

        if (hammerBtn.currentState == BoosterButton.BOOSTER_STATE.LOCK)
            return;

        if (hammerBtn.currentState == BoosterButton.BOOSTER_STATE.UNLOCK)
        {
            HideView();
            GameManager.instance.uiManager.boosterView.ShowView(BoosterView.BoosterType.HAMMER);
            GameManager.instance.currentGameState = GAME_STATE.HAMMER_BOOSTER;
            GameManager.instance.SubHammer();

            if (GameManager.instance.currentHammerBooster == 0)
                hammerBtn.SetAdd();
        }
        else if(hammerBtn.currentState == BoosterButton.BOOSTER_STATE.ADD)
        {
            GameManager.instance.uiManager.moreBoosterView.InitView(MoreBoosterView.POPUP_TYPE.HAMMER);
            GameManager.instance.uiManager.moreBoosterView.ShowView();
        }
           
    }

 
}
