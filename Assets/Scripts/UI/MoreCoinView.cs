using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreCoinView : BaseView
{
    public override void InitView()
    {

    }

    public override void Start()
    {

    }

    public override void Update()
    {

    }

    public void ShowStore()
    {
        AudioManager.instance.clickSound.Play();
        HideView();
        GameManager.instance.uiManager.shopView.ShowView();
    }

    private GameManager.GAME_STATE lastState;

    public override void ShowView()
    {
        AudioManager.instance.clickSound.Play();
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
}
