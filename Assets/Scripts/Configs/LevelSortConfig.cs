using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSortConfig", menuName = "GameData/LevelSortConfig")]
public class LevelSortConfig : ScriptableObject
{
    public LevelData[] levelDataList;
}
