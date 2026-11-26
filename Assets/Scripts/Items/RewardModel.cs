using System;
using UnityEngine;

[Serializable]
public class RewardModel 
{
    public enum Reward_Type
    {
        COIN,
        BOMB,
        HAMMER
    }

    public Reward_Type type;

    public int total;

    public Sprite icon;
}
