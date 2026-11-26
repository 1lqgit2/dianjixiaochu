using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDataSO", menuName = "GameData/LevelDataSO")]
public class LevelData : ScriptableObject
{
    public int sizeWidth;

    public int sizeHeight;

    public List<NodeComponent> solution;

    public List<TileComponent> tiles;

    public List<NodeComponent> saws;

    public List<NodeComponent> walls;

    public List<NodeComponent> bombs;

    public List<TileComponent> locks;

    public List<RotatorComponent> rotators;

    public float offsetX;

    public float offsetY;

    public int moves;
}
