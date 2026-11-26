using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class RotatorElement : MonoBehaviour
{
    public TileHolder tileHolder;

    public GameObject rotWrong;

    public Transform mainHandRoot;

    public List<RotatorHand> rootHandList;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitRotator(RotatorComponent rotComponent)
    {
        for (int i = 0; i < rotComponent.hands.Count / 2; i++)
        {
            rootHandList[rotComponent.hands[i * 2]].InitHand(rotComponent.hands[i * 2 + 1], rotComponent.hands[i * 2]);
            // Debug.Log("HAND " + rotComponent.hands[i * 2] + " LENGTH : " + rotComponent.hands[i * 2 + 1]);
        }
    }

    public void Rotate()
    {
       
        int stepRotate = GetRotateStep();
        GetComponent<Collider2D>().enabled = false;
        PlayPressAnim();
        if(stepRotate > 0)
        {
            Debug.Log("ROT STEP " + stepRotate);
            RotateHand(stepRotate);

            if(stepRotate != 3)
            {
                mainHandRoot.DOLocalRotate(new Vector3(0, 0, -90 * stepRotate), 0.125f * stepRotate).SetRelative(true).SetEase(Ease.Linear).OnComplete(() =>
                {
                    GetComponent<Collider2D>().enabled = true;

                });
            }

            else
            {
                mainHandRoot.DOLocalRotate(new Vector3(0, 0, 90), 0.125f).SetRelative(true).SetEase(Ease.Linear).OnComplete(() =>
                {
                    GetComponent<Collider2D>().enabled = true;

                });
            }
           
        }
        else
        {
            GetComponent<Collider2D>().enabled = true;
            Debug.Log("CAN NOT ROTATE");
        }
    }

    private void RotateHand(int step)
    {
        for (int i = 0; i < rootHandList.Count; i++)
        {
            for (int j = 0; j < rootHandList[i].childList.Count; j++)
            {
                if (rootHandList[i].childList[j] != null)
                {
                    if (rootHandList[i].childList[j].tag == "Tile")
                    {
                        TileElement tile = rootHandList[i].childList[j].GetComponent<TileElement>();
                        tile.RotateTile(step, tile.nodeIndexInRotator);
                    }

                    if (rootHandList[i].childList[j].tag == "Saw")
                    {
                        SawElement saw = rootHandList[i].childList[j].GetComponent<SawElement>();
                        saw.RotateTile(step, saw.nodeIndexInRotator);
                        //Debug.Log("SAW NODE" + (j+1).ToString());
                    }
                }
              
            }
        }
    }

    private bool CanRotateHand(int step)
    {
        bool canRotate = true;
        for (int i = 0; i < rootHandList.Count; i++)
        {
            for (int j = 0; j < rootHandList[i].childList.Count; j++)
            {
                if (rootHandList[i].childList[j] != null)
                {
                    if (rootHandList[i].childList[j].tag == "Tile")
                    {
                        TileElement tile = rootHandList[i].childList[j].GetComponent<TileElement>();
                        if(!tile.CanRotate(step, j + 1))
                        {
                            Debug.Log("CAN NOT ROTATE AT " + " HAND " + i + "  NODE " + j);
                            canRotate = false;
                            return canRotate;
                        }

                    }
                }

            }
        }

        return canRotate;
    }


    private int GetRotateStep()
    {
        int stepRotate = 0;

        for(int i = 1; i < 4; i++)
        {
            if (CanRotateHand(i))
            {
                stepRotate++;
                return stepRotate;
            }
            else
            {
                stepRotate++;
            }

        }

        if (stepRotate == 3)
            stepRotate = 0;

        return stepRotate;
    }

    private void PlayPressAnim()
    {
        StartCoroutine(PlayPressAnimIE());
    }

    IEnumerator PlayPressAnimIE()
    {
        rotWrong.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        rotWrong.SetActive(false);
    }
}
