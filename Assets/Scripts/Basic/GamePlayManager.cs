using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GamePlayManager : MonoSingleton<GamePlayManager>
{
    public ChessBoard ChessBoard;

    public Chess[,] chessArr;

    public int GridSize_W = 8;
    public int GridSize_H = 8;
    public int CellSize = 100;

    void Awake()
    {
        CellSize = 800 / GridSize_H;
        chessArr = new Chess[GridSize_W, GridSize_H];
    }

    void Start()
    {
    }

    int turn = 0;

    void Update()
    {
        UpdateMouseDown();
        UpdatePutChess();
        UpdateMouseHover();
    }

    int[] lastFocusPos;
    MouseHoverBox currentMouseHoverBox;

    private void UpdateMouseHover()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.Playing)
        {
            if (currentMouseHoverBox) currentMouseHoverBox.PoolRecycle();
            lastFocusPos = null;
            return;
        }

        int[] pos = getClickPosition();
        if (lastFocusPos != null && pos != null && lastFocusPos[0] == pos[0] && lastFocusPos[1] == pos[1])
        {
            return;
        }
        else
        {
            if (pos != null)
            {
                lastFocusPos = pos;
                if (currentMouseHoverBox) currentMouseHoverBox.PoolRecycle();
                currentMouseHoverBox = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.MouseHoverBox].AllocateGameObject<MouseHoverBox>(ChessBoard.transform);
                ((RectTransform) (currentMouseHoverBox.transform)).anchoredPosition = new Vector3((pos[0] - GridSize_W / 2 + 0.5f) * CellSize, (pos[1] - GridSize_H / 2 + 0.5f) * CellSize);
            }
        }
    }

    private void UpdateMouseDown()
    {
        if (GameManager.Instance.GameState == GameManager.GameStates.Playing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseLeftDown = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                mouseRightDown = true;
            }
        }
    }

    bool mouseLeftDown = false;
    bool mouseRightDown = false;

    private void UpdatePutChess()
    {
        if (GameManager.Instance.GameState == GameManager.GameStates.Playing)
        {
            if (mouseLeftDown && Input.GetMouseButtonUp(0))
            {
                int[] pos = getClickPosition();
                if (pos == null) return;
                if (!chessArr[pos[0], pos[1]])
                {
                    PutChess(pos[0], pos[1]);
                }

                mouseLeftDown = false;
            }
        }
    }

    private int[] getClickPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.point);
            int x = Mathf.RoundToInt((hit.point.x / CellSize * 100 + 0.5f) / 1.15f);
            int y = Mathf.RoundToInt((hit.point.y / CellSize * 100 + 0.5f) / 1.15f);
            if (x < -GridSize_W / 2 || y < -GridSize_H / 2 || x > GridSize_W / 2 || y > GridSize_H / 2)
            {
                return null;
            }

            x += GridSize_W / 2 - 1;
            y += GridSize_H / 2 - 1;
            Debug.Log(x + "," + y);
            return new int[] {x, y};
        }

        return null;
    }

    private void PutChess(int posX, int posY)
    {
        Chess chess = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.ChessColors[turn]].AllocateGameObject<Chess>(ChessBoard.transform);
        chessArr[posX, posY] = chess;

        ((RectTransform) (chess.transform)).anchoredPosition = new Vector3((posX - GridSize_W / 2 + 0.5f) * CellSize, (posY - GridSize_H / 2 + 0.5f) * CellSize);
        chess.Pos = new int[] {posX, posY};
        AudioManager.Instance.SoundPlay("OnMove");

        bool isOver = CheckGameOver();

        if (!isOver)
        {
            turn++;
            if (turn > 3) turn = 0;
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                GameManager.Instance.Players[i].SetActive(turn == i);
            }
        }
    }

    private bool CheckGameOver()
    {
        for (int i = 0; i < chessArr.GetLength(0); i++)
        {
            for (int j = 0; j < chessArr.GetLength(1); j++)
            {
                Chess ch = chessArr[i, j];
                if (ch == null)
                {
                    continue;
                }
                else
                {
                    if (CheckAllDirections(ch))
                    {
                        GameManager.Instance.GameOver(turn == 0);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        LeftDown,
        RightDown,
        LeftUp,
        RightUp,
    }

    private bool CheckAllDirections(Chess ch)
    {
        if (CheckChessLine(ch, Direction.Up))
            return true;

        if (CheckChessLine(ch, Direction.Down))
            return true;

        if (CheckChessLine(ch, Direction.Left))
            return true;

        if (CheckChessLine(ch, Direction.Right))
            return true;

        if (CheckChessLine(ch, Direction.LeftDown))
            return true;

        if (CheckChessLine(ch, Direction.RightDown))
            return true;

        if (CheckChessLine(ch, Direction.LeftUp))
            return true;

        if (CheckChessLine(ch, Direction.RightUp))
            return true;

        return false;
    }

    private bool CheckChessLine(Chess chess, Direction direction, List<Chess> temp_ChessGroup = null)
    {
        if (temp_ChessGroup == null)
        {
            temp_ChessGroup = new List<Chess>();
        }

        if (chess == null) return false;
        temp_ChessGroup.Add(chess);
        if (temp_ChessGroup.Count >= GameManager.Instance.GameWinChessNum) return true;
        int[] pos = chess.Pos;
        Chess nextChess = null;
        switch (direction)
        {
            case Direction.Up:
                if (pos[1] > 0) nextChess = chessArr[pos[0], pos[1] - 1];
                break;
            case Direction.Down:
                if (pos[1] < chessArr.GetLength(1) - 1) nextChess = chessArr[pos[0], pos[1] + 1];
                break;
            case Direction.Left:
                if (pos[0] > 0) nextChess = chessArr[pos[0] - 1, pos[1]];
                break;
            case Direction.Right:
                if (pos[0] < chessArr.GetLength(0) - 1) nextChess = chessArr[pos[0] + 1, pos[1]];
                break;
            case Direction.LeftDown:
                if (pos[0] > 0 && pos[1] < chessArr.GetLength(1) - 1) nextChess = chessArr[pos[0] - 1, pos[1] + 1];
                break;
            case Direction.RightDown:
                if (pos[0] < chessArr.GetLength(0) - 1 && pos[1] < chessArr.GetLength(1) - 1) nextChess = chessArr[pos[0] + 1, pos[1] + 1];
                break;
            case Direction.LeftUp:
                if (pos[0] > 0 && pos[1] > 0) nextChess = chessArr[pos[0] - 1, pos[1] - 1];
                break;
            case Direction.RightUp:
                if (pos[0] < chessArr.GetLength(0) - 1 && pos[1] > 0) nextChess = chessArr[pos[0] + 1, pos[1] - 1];
                break;
        }

        if (nextChess != null)
        {
            return CheckChessLine(nextChess, direction, temp_ChessGroup);
        }

        return false;
    }

    public void ResetGameBoard()
    {
        foreach (GameObject player in GameManager.Instance.Players)
        {
            player.SetActive(false);
        }

        GameManager.Instance.Players[0].SetActive(true);

        foreach (Chess ch in chessArr)
        {
            if (ch) ch.PoolRecycle();
        }

        for (int i = 0; i < chessArr.GetLength(0); i++)
        {
            for (int j = 0; j < chessArr.GetLength(1); j++)
            {
                chessArr[i, j] = null;
            }
        }
    }
}