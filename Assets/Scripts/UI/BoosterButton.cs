using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    public enum BOOSTER_STATE
    {
        LOCK,
        ADD,
        UNLOCK
    }

    public BOOSTER_STATE currentState;

    public enum BOOSTER_TYPE
    {
        BOMB,
        HAMMER
    }

    public BOOSTER_TYPE boosterType;

    public GameObject addObj;

    public GameObject lockObj;

    public Text countTxt;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLock()
    {
        currentState = BOOSTER_STATE.LOCK;
        lockObj.SetActive(true);
        addObj.SetActive(false);
        countTxt.gameObject.SetActive(false);
    }

    public void SetUnLock()
    {
        currentState = BOOSTER_STATE.UNLOCK;
        lockObj.SetActive(false);
        addObj.SetActive(false);
        countTxt.gameObject.SetActive(true);

        int countTotal = 0;
        if (boosterType == BOOSTER_TYPE.BOMB)
            countTotal = PlayerPrefs.GetInt("Bomb");
        else
             if (boosterType == BOOSTER_TYPE.HAMMER)
            countTotal = PlayerPrefs.GetInt("Hammer");

        countTxt.text = countTotal.ToString();
    }

    public void SetAdd()
    {
        currentState = BOOSTER_STATE.ADD;
        lockObj.SetActive(false);
        addObj.SetActive(true);
        countTxt.gameObject.SetActive(false);
    }

    public void Refresh()
    {
        int countTotal = 0;

        if (boosterType == BOOSTER_TYPE.BOMB)
            countTotal = PlayerPrefs.GetInt("Bomb");
        else
             if (boosterType == BOOSTER_TYPE.HAMMER)
            countTotal = PlayerPrefs.GetInt("Hammer");

        if (countTotal == 0)
            SetLock();
        else
            SetUnLock();

        countTxt.text = countTotal.ToString();
    }
}
