using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

public class LevelParser : MonoBehaviour
{
    public string fileName = "Assets/Resources/LevelDataJson/board_0017.json";

    LevelData levelData;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            for(int i = 1; i <= 200; i++)
            {
                ParseData(i);
            }
        }
    }

    public void ParseData(int levelIndex)
    {
        levelData = new LevelData();

       // if (levelIndex <= 9)
            fileName = "Assets/Resources/LevelDataJson/MT/board_MT_" + levelIndex.ToString() + ".json";
        //else if (levelIndex >= 10 && levelIndex <= 99)
       //     fileName = "Assets/Resources/LevelDataJson/board_MT_00" + levelIndex.ToString() + ".json";
       // else
        //    fileName = "Assets/Resources/LevelDataJson/board_MT_0" + levelIndex.ToString() + ".json";

        string jsonString = File.ReadAllText(fileName);
        JSONNode jsonNode = SimpleJSON.JSON.Parse(jsonString);

        levelData.sizeWidth = jsonNode["size"]["width"];
        levelData.sizeHeight = jsonNode["size"]["height"];

        //parse solution
        JSONNode solutionNode = jsonNode["solution"]["steps"];

        levelData.solution = new List<NodeComponent>();

        for (int i = 0; i < solutionNode.Count; i++)
        {
            NodeComponent nodeComponent = new NodeComponent();
            nodeComponent.posX = solutionNode[i]["x"];
            nodeComponent.posY = solutionNode[i]["y"];
            levelData.solution.Add(nodeComponent);
        }



        //parse tiles

        JSONNode tilesNode = jsonNode["tiles"];

        levelData.tiles = new List<TileComponent>();

        for (int i = 0; i < tilesNode.Count; i++)
        {
            TileComponent tileComponent = new TileComponent();
            tileComponent.posX = tilesNode[i]["position"]["x"];
            tileComponent.posY = tilesNode[i]["position"]["y"];
            tileComponent.tileValue = tilesNode[i]["direction"];

            levelData.tiles.Add(tileComponent);
        }

        //parse saw

        JSONNode sawNode = jsonNode["saws"];
        levelData.saws = new List<NodeComponent>();

        for (int i = 0; i < sawNode.Count; i++)
        {
            NodeComponent sawComponent = new NodeComponent();
            sawComponent.posX = sawNode[i]["position"]["x"];
            sawComponent.posY = sawNode[i]["position"]["y"];

            levelData.saws.Add(sawComponent);
        }

        //parse rotator

        JSONNode rotatorNode = jsonNode["rotators"];

        levelData.rotators = new List<RotatorComponent>();

        for (int i = 0; i < rotatorNode.Count; i++)
        {
            RotatorComponent rotComponent = new RotatorComponent();
            rotComponent.posX = rotatorNode[i]["position"]["x"];
            rotComponent.posY = rotatorNode[i]["position"]["y"];

            JSONNode handNode = rotatorNode[i]["hands"];

            rotComponent.hands = new List<int>();
            for(int j = 0; j < handNode.Count; j++)
            {
                rotComponent.hands.Add(handNode[j]);
            }

            rotComponent.direction = rotatorNode[i]["direction"];

            levelData.rotators.Add(rotComponent);
        }

        levelData.moves = jsonNode["moves"];
        CreateLevelSO(levelIndex);
    }

    public void CreateLevelSO(int levelIndex)
    {

       // UnityEditor.AssetDatabase.CreateAsset(levelData, "Assets/Resources/LevelDataSO/MT/LevelSO" + levelIndex.ToString() + ".asset");

    }
}
