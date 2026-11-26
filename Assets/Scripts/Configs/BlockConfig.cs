using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockConfigSO", menuName = "GameData/BlockConfigSO")]
public class BlockConfig : ScriptableObject
{
    public List<BlockSet> blockSetList;
}

[Serializable]
public class BlockSet
{
    public List<Sprite> directionSprite;
}