using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SawElement : MonoBehaviour
{
    public Transform sawRoot;

    public ParticleSystem bloodVfx;

    public TileHolder tileHolder;

    // Start is called before the first frame update
    void Start()
    {
        if (sawRoot != null)
            sawRoot.DOLocalRotate(new Vector3(0, 0, 360), 2f, RotateMode.Fast).SetRelative(true).SetLoops(-1).SetEase(Ease.Linear);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int directionInRotator;

    public int handIndexInRotator;

    public int nodeIndexInRotator;

    public RotatorElement rotatorParent;

    public void RotateTile(int step, int nodeIndex)
    {
        switch (directionInRotator)
        {
            case 0:
                RotateTileUp(step, nodeIndex);
                break;
            case 1:
                RotateTileLeft(step, nodeIndex);
                break;
            case 2:
                RotateTileDown(step, nodeIndex);
                break;
            case 3:
                RotateTileRight(step, nodeIndex);
                break;
        }
    }

    public void RotateTileUp(int step, int nodeIndex)
    {
        //GetComponent<Collider>().enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                directionInRotator = 3;
            });

        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            nextCellIndex2 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    directionInRotator = 2;
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                directionInRotator = 1;
            });
        }

    }


    public void RotateTileLeft(int step, int nodeIndex)
    {
        // GetComponent<Collider>().enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 0;
            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            nextCellIndex2 = cellIndex + nodeIndex * 2;
            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {

                    directionInRotator = 3;
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 2;
            });
        }

    }



    public void RotateTileDown(int step, int nodeIndex)
    {
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 1;
            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            nextCellIndex2 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;

            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {

                    directionInRotator = 0;
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 3;
            });
        }

    }



    public void RotateTileRight(int step, int nodeIndex)
    {
        //GetComponent<Collider>().enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;

            if (tileHolder.saw == this)
                tileHolder.saw = null;

            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 2;
            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            nextCellIndex2 = cellIndex - nodeIndex * 2;
            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {

                    directionInRotator = 1;
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.saw == this)
                tileHolder.saw = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (tileHolder.tile != null)
            {
                if (tileHolder.tile.rotatorParent != null)
                {
                    if (tileHolder.tile.rotatorParent != rotatorParent)
                    {
                        tileHolder.tile.nextSaw = this;
                        tileHolder.tile.KillTile();
                        tileHolder.tile = null;
                    }
                }
                else
                {
                    tileHolder.tile.nextSaw = this;
                    tileHolder.tile.KillTile();
                    tileHolder.tile = null;
                }

            }

            tileHolder.saw = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {

                directionInRotator = 0;
            });
        }

    }


}
