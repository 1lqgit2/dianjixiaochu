using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinView : BaseView
{
    public ShopSlot[] shopSlots;

    public Sprite[] iconList;

    public ShopSlot currentSelectSlot;

    public override void InitView()
    {
        Refresh();
    }

    public override void Start()
    {

    }

    public override void Update()
    {

    }

    public void Refresh()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            int lockState = PlayerPrefs.GetInt("Shop" + i.ToString());
            shopSlots[i].itemIndex = i;

            if (lockState == 0)
            {
                shopSlots[i].SetLock(true);
            }
            else
            {
                shopSlots[i].icon.sprite = iconList[i];

                shopSlots[i].SetLock(false);

                if (i == GameManager.instance.blockSkinIndex)
                {
                    shopSlots[i].Select(true);
                    currentSelectSlot = shopSlots[i];
                }
                    
                else
                    shopSlots[i].Select(false);
            }
        }
    }

    public void Select(ShopSlot slot)
    {
        AudioManager.instance.clickSound.Play();
        currentSelectSlot.Select(false);
        slot.Select(true);
        currentSelectSlot = slot;
        GameManager.instance.blockSkinIndex = slot.itemIndex;
        PlayerPrefs.SetInt("BlockSkin", slot.itemIndex);
        Refresh();
    }
}
