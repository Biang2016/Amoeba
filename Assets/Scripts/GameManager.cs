using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    public int GameWinChessNum = 5;

    void Awake()
    {
        PrefabManager.ClearPrefabDict();
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/");
        foreach (GameObject prefab in prefabs)
        {
            PrefabManager.AddPrefabRes(prefab.name, prefab);
        }
    }

    void Start()
    {
        initializeGameStatesCanvas();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameState == GameStates.Playing)
                Pause();
            else if (GameState == GameStates.Pause)
                Resume();
        }
    }

    #region GameStates And Canvas

    public GameObject GameOverCanvas;
    public Text GameOverText;
    public GameObject GameStartCanvas;
    public GameObject GamePauseCanvas;
    public GameObject ChessBoardCanvas;

    public List<GameObject> Players;

    public enum GameStates
    {
        BeforeStart,
        Playing,
        GameOver,
        Pause
    }

    public GameStates GameState = GameStates.BeforeStart;

    private void initializeGameStatesCanvas()
    {
        ChessBoardCanvas.SetActive(false);
        GameStartCanvas.SetActive(true);
        GameOverCanvas.SetActive(false);
        GamePauseCanvas.SetActive(false);
    }

    public void GameOver(bool PlayerAWin)
    {
        GameState = GameStates.GameOver;
        GameOverCanvas.SetActive(true);
        //if (PlayerAWin)
        //{
        //    GameOverText.text = "Player A Wins!";
        //}
        //else
        //{
        //    GameOverText.text = "Player B Wins!";
        //}
    }

    public void NewGame()
    {
        GameState = GameStates.Playing;
        GameStartCanvas.SetActive(false);
        ChessBoardCanvas.SetActive(true);
        foreach (GameObject player in Players)
        {
            player.SetActive(true);
        }
    }

    public void ClearGame()
    {
        ChessBoard.Instance.ResetGameBoard();
    }

    public void Replay()
    {
        GamePauseCanvas.SetActive(false);
        GameOverCanvas.SetActive(false);
        ClearGame();
        NewGame();
    }

    private void Pause()
    {
        GameState = GameStates.Pause;
        GamePauseCanvas.SetActive(true);
    }

    public void Resume()
    {
        GameState = GameStates.Playing;
        GamePauseCanvas.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion
}