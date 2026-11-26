using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public FreeAdsTimer freeAdsTimer;

    public DataManager dataManager;

    public BoardGenerator boardGenerator;

    public BoardController boardController;

    public UIManager uiManager;

    public bool isTestLevelMode;

    public int levelTestIndex;

    //[HideInInspector]
    public int levelIndex;

    [HideInInspector]
    public int blockSkinIndex;

    [HideInInspector]
    public int currentCoin;

    [HideInInspector]
    public int currentHammerBooster;

    [HideInInspector]
    public int currentBombBooster;

    public enum GAME_STATE
    {
        IN_HOME,
        IN_GAME,
        POP_UP,
        BOMB_BOOSTER,
        HAMMER_BOOSTER,
        BOOSTER_WORKING,
        LEVEL_CLEAR,
        LEVEL_FAIL
    }

    public GAME_STATE currentGameState;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void LoadGame()
    {
        LoadGameData();
        ShowHome();
    }

    public void ShowHome()
    {
        currentGameState = GAME_STATE.IN_HOME;
        uiManager.currencyView.RefreshView();
        uiManager.homeView.InitView();
        uiManager.homeView.ShowView();
        uiManager.skinView.InitView();
        uiManager.dailyGiftView.InitView();
    }

    public void InitGame()
    {
        boardGenerator.InitBoard();
    }

    public void PlayGame()
    {
        currentGameState = GAME_STATE.IN_GAME;
        uiManager.homeView.HideView();
        InitGame();
        uiManager.gameView.InitView();
        uiManager.gameView.ShowView();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            AddCoin(200);
        }
    }

    public void UseMoves()
    {
        if (boardGenerator.moves > 0)
        {
            boardGenerator.moves--;
            uiManager.gameView.UpdateMovesTxt();
        }

        if (boardGenerator.moves == 0)
        {
            StartCoroutine(CheckOutOfMovesIE());
        }
    }

    public void CheckRemainBlocks()
    {
        if (GameManager.instance.currentGameState == GAME_STATE.LEVEL_FAIL)
            return;

        boardGenerator.remainBlocks--;
        if (boardGenerator.remainBlocks == 0)
        {

            ShowWin();
        }
    }

    private void ShowWin()
    {
        currentGameState = GAME_STATE.LEVEL_CLEAR;
        uiManager.winView.InitView();
        uiManager.winView.ShowView();
    }

    IEnumerator CheckOutOfMovesIE()
    {
        yield return new WaitForSeconds(2.0f);

        if (boardGenerator.moves == 0 && boardGenerator.remainBlocks > 0)
        {
            ShowOutOfMoves();
        }
            
    }

    public void ShowOutOfMoves()
    {
        currentGameState = GAME_STATE.LEVEL_FAIL;
        uiManager.failView.ShowView();
    }

    public void Replay()
    {
        currentGameState = GAME_STATE.IN_GAME;
        CleanBoard();
        InitGame();
        uiManager.gameView.InitView();
    }

    public void ResumeGame()
    {
        currentGameState = GAME_STATE.IN_GAME;
        uiManager.gameView.InitView();
    }

    public void BackToHome()
    {
        CleanBoard();
        ShowHome();
    }

    private void CleanBoard()
    {
        for(int i = 0; i < boardController.tileHolderList.Count; i++)
        {
            if (boardController.tileHolderList[i].tile != null)
                Destroy(boardController.tileHolderList[i].tile.gameObject);
            if (boardController.tileHolderList[i].saw != null)
                Destroy(boardController.tileHolderList[i].saw.gameObject);
            if (boardController.tileHolderList[i].rotator != null)
                Destroy(boardController.tileHolderList[i].rotator.gameObject);
            Destroy(boardController.tileHolderList[i].gameObject);
        }

        boardController.tileHolderList.Clear();
    }

    private void LoadGameData()
    {
        if(isTestLevelMode)
        {
            levelIndex = levelTestIndex;
            currentCoin = 50000;
            currentBombBooster = 2;
            currentHammerBooster = 2;
            blockSkinIndex = PlayerPrefs.GetInt("BlockSkin");
            PlayerPrefs.SetInt("Coin", currentCoin);
            PlayerPrefs.SetInt("Hammer", currentHammerBooster);
            PlayerPrefs.SetInt("Bomb", currentBombBooster);
        }
        else
        {
           
            blockSkinIndex = PlayerPrefs.GetInt("BlockSkin");
            levelIndex = PlayerPrefs.GetInt("LevelIndex");

            if(PlayerPrefs.GetInt("InitData") == 0)
            {
                currentCoin = 100;
                currentBombBooster = 3;
                currentHammerBooster = 3;
                PlayerPrefs.SetInt("Coin", currentCoin);
                PlayerPrefs.SetInt("Shop0", 1);
                PlayerPrefs.SetInt("InitData", 1);
                PlayerPrefs.SetInt("Hammer", currentHammerBooster);
                PlayerPrefs.SetInt("Bomb", currentBombBooster);
            }
            

            currentCoin = PlayerPrefs.GetInt("Coin");
            currentHammerBooster = PlayerPrefs.GetInt("Hammer");
            currentBombBooster = PlayerPrefs.GetInt("Bomb");
        }


    }

    public void AddCoin(int moreCoin)
    {
        StartCoroutine(AddCoinIE(moreCoin));
    }

    public void SubCoin(int subCoin)
    {
        currentCoin -= subCoin;
        PlayerPrefs.SetInt("Coin", currentCoin);
        uiManager.currencyView.RefreshView();
    }

    public void AddHammer(int moreHammer)
    {
        currentHammerBooster += moreHammer;
        PlayerPrefs.SetInt("Hammer", currentHammerBooster);
        uiManager.gameView.hammerBtn.Refresh();
    }

    public void SubHammer()
    {
        currentHammerBooster--;
        PlayerPrefs.SetInt("Hammer", currentHammerBooster);
        uiManager.gameView.hammerBtn.Refresh();
    }


    public void AddBomb(int moreBomb)
    {
        currentBombBooster += moreBomb;
        PlayerPrefs.SetInt("Bomb", currentBombBooster);
        uiManager.gameView.bombBtn.Refresh();
    }

    public void SubBomb()
    {
        currentBombBooster--;
        PlayerPrefs.SetInt("Bomb", currentBombBooster);
        uiManager.gameView.bombBtn.Refresh();
    }

    IEnumerator AddCoinIE(int moreCoin)
    {
        //AudioManager.instance.coinFlySound.Play();
        uiManager.currencyView.SpawnCoin(Vector3.zero);
        yield return new WaitForSeconds(0.75f);
        //AudioManager.instance.coinFlySound.Stop();
        //AudioManager.instance.coinCollectSound.Play();
        currentCoin += moreCoin;
        AudioManager.instance.coinSound.Play();
        PlayerPrefs.SetInt("Coin", currentCoin);
        uiManager.currencyView.RefreshView();
    }
}
