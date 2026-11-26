using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterView :BaseView
{
    public enum BoosterType
    {
        BOMB,
        HAMMER
    }

    public GameObject bombTxt;

    public GameObject hammerTxt;

    public override void InitView()
    {
       
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
       
    }

    public void ShowView(BoosterType type)
    {
        base.ShowView();

        if(type == BoosterType.BOMB)
        {
            bombTxt.SetActive(true);
            hammerTxt.SetActive(false);
        }
        else
        {
            bombTxt.SetActive(false);
            hammerTxt.SetActive(true);
        }

    }

    public override void HideView()
    {
        AudioManager.instance.clickSound.Play();
        base.HideView();
        bombTxt.SetActive(false);
        hammerTxt.SetActive(false);
    }
}
