using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public int sizeX;

    public int sizeY;

    public float tileSize;

    private Vector2 anchorBoard;

    public TileHolder tileHolderPrefab;

    public LevelData currentLevelData;

    public int moves;

    public int remainBlocks;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void InitBoard()
    {
        GenBoard();
    }

    public void GenBoard()
    {
        remainBlocks = 0;
        currentLevelData = GameManager.instance.dataManager.levelConfig.levelDataList[GameManager.instance.levelIndex];
        moves = currentLevelData.moves;
        sizeX = currentLevelData.sizeWidth;
        sizeY = currentLevelData.sizeHeight;
        GameManager.instance.boardController.tileHolderList = new List<TileHolder>();

        anchorBoard = new Vector2(-0.5f * currentLevelData.sizeWidth * tileSize, 0.5f * currentLevelData.sizeHeight * tileSize);

        for (int i = 0; i < currentLevelData.sizeHeight; i++)
        {
            for (int j = 0; j < currentLevelData.sizeWidth; j++)
            {
                TileHolder holder = Instantiate(tileHolderPrefab);
                holder.transform.SetParent(transform);
                holder.transform.localPosition = anchorBoard + new Vector2(0.5f * tileSize, -0.5f * tileSize) + new Vector2(j * tileSize, -i * tileSize);
                holder.posX = j;
                holder.posY = i;
                holder.cellIndex = i * currentLevelData.sizeWidth + j;
                GameManager.instance.boardController.tileHolderList.Add(holder);

                //load tiles
                for (int k = 0; k < currentLevelData.tiles.Count; k++)
                {
                    if (currentLevelData.tiles[k].posX == j && currentLevelData.tiles[k].posY == i)
                    {
                        holder.LoadTileElement(currentLevelData.tiles[k].tileValue);
                        remainBlocks++;
                    }
                       
                }
                //load saws
                for (int k = 0; k < currentLevelData.saws.Count; k++)
                {
                    if (currentLevelData.saws[k].posX == j && currentLevelData.saws[k].posY == i)
                        holder.LoadSaw();
                }

              

            }
        }

        for (int i = 0; i < currentLevelData.sizeHeight; i++)
        {
            for (int j = 0; j < currentLevelData.sizeWidth; j++)
            {
                int cellIndex = i * currentLevelData.sizeWidth + j;
                //load rotators
                for (int k = 0; k < currentLevelData.rotators.Count; k++)
                {
                    if (currentLevelData.rotators[k].posX == j && currentLevelData.rotators[k].posY == i)
                        GameManager.instance.boardController.tileHolderList[cellIndex].LoadRotator(currentLevelData.rotators[k]);
                }


            }
        }

    }

}
