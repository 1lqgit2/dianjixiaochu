using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;
using static GameManager;

public class BoardController : MonoBehaviour
{
    public List<TileHolder> tileHolderList;

    public GameObject hammerObj;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void InitBoard()
    {

    }

    private TileElement currentBoosterTile;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.currentGameState == GAME_STATE.POP_UP)
            return;

        if (GameManager.instance.currentGameState == GAME_STATE.BOOSTER_WORKING)
            return;

        if (GameManager.instance.currentGameState == GAME_STATE.HAMMER_BOOSTER)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

                if (rayHit.transform != null)
                {
                    if (rayHit.transform.tag == "Tile")
                    {
                        GameManager.instance.currentGameState = GAME_STATE.BOOSTER_WORKING;
                        currentBoosterTile = rayHit.transform.GetComponent<TileElement>();
                        StartCoroutine(ProcessingHammer());
                    }
                }
            }


        }

        if (GameManager.instance.currentGameState == GAME_STATE.BOMB_BOOSTER)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

                if (rayHit.transform != null)
                {
                    if (rayHit.transform.tag == "Tile")
                    {
                        GameManager.instance.currentGameState = GAME_STATE.BOMB_BOOSTER;
                        currentBoosterTile = rayHit.transform.GetComponent<TileElement>();
                        StartCoroutine(ProcessingBoom());
                    }
                }
            }


        }

        if (GameManager.instance.currentGameState != GAME_STATE.IN_GAME)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.instance.boardGenerator.moves == 0)
            {
                return;
            }
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (rayHit.transform != null)
            {
                if (rayHit.transform.tag == "Tile")
                {
                    if (AudioManager.instance.hapticState == 1)
                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);

                    TileElement tile = rayHit.transform.GetComponent<TileElement>();
                    tile.MoveTile();
                    GameManager.instance.UseMoves();
                    AudioManager.instance.PlayComboSound();
                }

                if (rayHit.transform.tag == "Rotator")
                {
                    if (AudioManager.instance.hapticState == 1)
                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);

                    RotatorElement rot = rayHit.transform.GetComponent<RotatorElement>();
                    rot.Rotate();
                    GameManager.instance.UseMoves();
                    AudioManager.instance.rotateSound.Play();
                }

                if (GameManager.instance.levelIndex == 0)
                {
                    GameManager.instance.uiManager.gameView.HideTut();
                }
            }


        }
    }

    IEnumerator ProcessingHammer()
    {

        if (currentBoosterTile != null)
        {
            hammerObj.SetActive(true);
            hammerObj.transform.position = currentBoosterTile.tileHolder.transform.position;
        }
        yield return new WaitForSeconds(1.0f);

        if (currentBoosterTile != null)
        {
            currentBoosterTile.BreakTile();
            AudioManager.instance.hammerSound.Play();
            AudioManager.instance.PlayComboSound();
            currentBoosterTile = null;
            hammerObj.SetActive(false);
            GameManager.instance.currentGameState = GAME_STATE.IN_GAME;
            GameManager.instance.uiManager.boosterView.HideView();
            GameManager.instance.uiManager.gameView.ShowView();
        }

    }

    IEnumerator ProcessingBoom()
    {
        AudioManager.instance.bombSound.Play();

        if (currentBoosterTile != null)
        {
            currentBoosterTile.BreakComboTiles();
            AudioManager.instance.PlayComboSound();
        }


        yield return new WaitForSeconds(1.0f);

        GameManager.instance.currentGameState = GAME_STATE.IN_GAME;
        GameManager.instance.uiManager.boosterView.HideView();
        GameManager.instance.uiManager.gameView.ShowView();
        currentBoosterTile = null;


    }
}
