using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorHand : MonoBehaviour
{
    public RotatorElement parentElement;

    public List<Transform> handsList;

    public List<Transform> childList;

    public GameObject handPrefab;

    public int handLenght;

    public int handDirection;

    public float handDistance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitHand(int lenght, int direction)
    {
        handLenght = lenght;
        handDirection = direction;

        for (int i = 0; i < handLenght; i++)
        {
            GameObject handObj = Instantiate(handPrefab);
            handObj.transform.SetParent(transform);
            handObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            handObj.transform.localPosition = new Vector3(-handDistance * i, 0, 0);
            /*
            if (handDirection == 0)
                handObj.transform.localPosition = new Vector3(-handDistance * i, 0, 0);
            else if (handDirection == 2)
                handObj.transform.localPosition = new Vector3(-handDistance * i, 0, 0);
            else if (handDirection == 1)
                handObj.transform.localPosition = new Vector3(-handDistance * i, 0, 0);
            else if (handDirection == 3)
                handObj.transform.localPosition = new Vector3(-handDistance * i, 0, 0);
            */
            handsList.Add(handObj.transform);
        }

        AttachChild();
    }

    public void AttachChild()
    {
        if (handDirection == 0)
        {
            for (int i = 1; i < handLenght + 1; i++)
            {
                int holderIndex = parentElement.tileHolder.cellIndex;
                if (GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.rotatorParent = parentElement;
                }

                else if (GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.rotatorParent = parentElement;
                }
            }
        }

        if (handDirection == 1)
        {
            for (int i = 1; i < handLenght + 1; i++)
            {
                int holderIndex = parentElement.tileHolder.cellIndex;
                if (GameManager.instance.boardController.tileHolderList[holderIndex - i].tile != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex - i].tile.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].tile.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].tile.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].tile.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].tile.rotatorParent = parentElement;
                }

                else if (GameManager.instance.boardController.tileHolderList[holderIndex - i].saw != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex - i].saw.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].saw.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].saw.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].saw.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex - i].saw.rotatorParent = parentElement;
                }
            }
        }

        if (handDirection == 2)
        {
            for (int i = 1; i < handLenght + 1; i++)
            {
                int holderIndex = parentElement.tileHolder.cellIndex;
                //Debug.Log("HOLDER INDX " + (holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth));
                if (GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile.rotatorParent = parentElement;
                }

                else if (GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw.rotatorParent = parentElement;
                }
            }
        }

        if (handDirection == 3)
        {
            for (int i = 1; i < handLenght + 1; i++)
            {
                int holderIndex = parentElement.tileHolder.cellIndex;

                if (GameManager.instance.boardController.tileHolderList[holderIndex + i].tile != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex + i].tile.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].tile.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].tile.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].tile.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].tile.rotatorParent = parentElement;
                }

                else if (GameManager.instance.boardController.tileHolderList[holderIndex + i].saw != null)
                {
                    childList.Add(GameManager.instance.boardController.tileHolderList[holderIndex + i].saw.transform);
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].saw.directionInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].saw.handIndexInRotator = handDirection;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].saw.nodeIndexInRotator = i;
                    GameManager.instance.boardController.tileHolderList[holderIndex + i].saw.rotatorParent = parentElement;
                }
            }
        }
    }

    public void ReleaseTileNode(int nodeIndex)
    {
        childList.RemoveAt(nodeIndex - 1);
        childList.Insert(nodeIndex - 1, null);
        /*
        if (nodeIndex == handsList.Count)
        {
            for (int i = 0; i < handsList.Count; i++)
                handsList[i].gameObject.SetActive(false);
        }
        */

        int removeNodeIndex = 0;
        for (int i = childList.Count - 1; i >= 0; i--)
        {
            if (childList[i] == null)
                removeNodeIndex++;
            else
                break;
        }

        removeNodeIndex = childList.Count - removeNodeIndex;

        for (int i = handsList.Count - 1; i >= removeNodeIndex; i--)
        {
            handsList[i].gameObject.SetActive(false);
        }

    }
}
