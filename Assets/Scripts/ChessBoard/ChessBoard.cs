using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoSingleton<ChessBoard>
{
    private List<ChessBoardLine> ChessBoardLines = new List<ChessBoardLine>();
    [SerializeField] private BoxCollider Collider;
    public ChessPiece[,] chessPieceArr;

    public int GridSize_W = 8;
    public int GridSize_H = 8;
    internal int CellSize = 100;

    void Awake()
    {
        CellSize = 1000 / GridSize_H;
        chessPieceArr = new ChessPiece[GridSize_W, GridSize_H];
    }

    void Start()
    {
        Collider.size = new Vector2(CellSize * GridSize_W, CellSize * GridSize_H);

        for (int i = -1; i < GridSize_W; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(transform);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(2, GridSize_H * CellSize);
            line.Rect.anchoredPosition = new Vector2(-(GridSize_W - 2) / 2.0f * CellSize + CellSize * i, 0);
        }

        for (int i = -1; i < GridSize_H; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(transform);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(GridSize_W * CellSize, 2);
            line.Rect.anchoredPosition = new Vector2(0, -(GridSize_H - 2) / 2.0f * CellSize + CellSize * i);
        }
    }

    void Update()
    {
        UpdateMouseDown();
        UpdatePutChess();
        UpdatePickupChess();
        UpdateMouseHover();
    }

    int[] lastFocusPos;
    internal MouseHoverBox CurrentMouseHoverBox;

    private void UpdateMouseHover()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.Playing)
        {
            if (CurrentMouseHoverBox) CurrentMouseHoverBox.PoolRecycle();
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
                if (CurrentMouseHoverBox) CurrentMouseHoverBox.PoolRecycle();
                CurrentMouseHoverBox = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.MouseHoverBox].AllocateGameObject<MouseHoverBox>(transform);
                ((RectTransform) (CurrentMouseHoverBox.transform)).anchoredPosition = new Vector3((pos[0] - GridSize_W / 2 + 0.5f) * CellSize, (pos[1] - GridSize_H / 2 + 0.5f) * CellSize);
                CurrentMouseHoverBox.ChangeColor(GamePlayManager.Instance.PlayerColors[GamePlayManager.Instance.Turn]);
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
                if (!chessPieceArr[pos[0], pos[1]])
                {
                    PutChess(pos[0], pos[1]);
                }

                mouseLeftDown = false;
            }
        }
    }

    private void UpdatePickupChess()
    {
        if (GameManager.Instance.GameState == GameManager.GameStates.Playing)
        {
            if (mouseRightDown && Input.GetMouseButtonUp(1))
            {
                int[] pos = getClickPosition();
                if (pos == null) return;
                if (chessPieceArr[pos[0], pos[1]])
                {
                    RemoveChess(pos[0], pos[1]);
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
            int x = Mathf.RoundToInt((hit.point.x / CellSize * 100 + 0.5f) / 1.15f);
            int y = Mathf.RoundToInt((hit.point.y / CellSize * 100 + 0.5f) / 1.15f);
            if (x < -GridSize_W / 2 || y < -GridSize_H / 2 || x > GridSize_W / 2 || y > GridSize_H / 2)
            {
                return null;
            }

            x += GridSize_W / 2 - 1;
            y += GridSize_H / 2 - 1;
            return new int[] {x, y};
        }

        return null;
    }

    private void PutChess(int posX, int posY)
    {
        GamePlayManager.Instance.CurrentPlayer.PlaceLeftTimes--;
        ChessPiece chessPiece = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessPiece].AllocateGameObject<ChessPiece>(transform);
        chessPieceArr[posX, posY] = chessPiece;
        chessPiece.Initialize(GamePlayManager.Instance.PlayerColors[GamePlayManager.Instance.Turn], CellSize);

        ((RectTransform) (chessPiece.transform)).anchoredPosition = new Vector3((posX - GridSize_W / 2 + 0.5f) * CellSize, (posY - GridSize_H / 2 + 0.5f) * CellSize);
        chessPiece.Pos = new int[] {posX, posY};
        AudioManager.Instance.SoundPlay("OnMove");
    }

    private void RemoveChess(int posX, int posY)
    {
        GamePlayManager.Instance.CurrentPlayer.PickOtherTimes--;
        chessPieceArr[posX, posY].PoolRecycle();
        chessPieceArr[posX, posY] = null;
        AudioManager.Instance.SoundPlay("AttackGun");
    }

    public void ResetGameBoard()
    {
        foreach (GameObject player in GameManager.Instance.Players)
        {
            player.SetActive(false);
        }

        GameManager.Instance.Players[0].SetActive(true);

        foreach (ChessPiece ch in chessPieceArr)
        {
            if (ch) ch.PoolRecycle();
        }

        for (int i = 0; i < chessPieceArr.GetLength(0); i++)
        {
            for (int j = 0; j < chessPieceArr.GetLength(1); j++)
            {
                chessPieceArr[i, j] = null;
            }
        }
    }

    private bool CheckGameOver()
    {
        return false;
    }
}