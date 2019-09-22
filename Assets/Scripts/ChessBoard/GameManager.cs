using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
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
        InitChessBoard();
    }

    [SerializeField] private Text MousePos;
    [SerializeField] private Text MousePos2;

    private List<ChessBoardLine> ChessBoardLines = new List<ChessBoardLine>();
    [SerializeField] private BoxCollider ChessBoardCollider;
    [SerializeField] private Transform ChessBoard;
    internal ChessPiece[,] ChessPieceArr;

    internal int GridSize_W = 8;
    internal int GridSize_H = 8;
    internal int CellSize = 100;

    [SerializeField] private Transform PlayersTrans;
    internal List<Player> Players = new List<Player>();
    internal int PlayerNumber = 4;
    [SerializeField] private List<Transform> PlayerPivots = new List<Transform>();
    public List<Color> PlayerColors = new List<Color>();
    internal Player CurrentPlayer => Players[turn];
    internal Color CurrentPlayerColor => PlayerColors[turn];

    [SerializeField] private InputField PlayerNumberInputField;
    [SerializeField] private InputField GridSize_W_InputField;
    [SerializeField] private InputField GridSize_H_InputField;

    private int turn = -1;

    public int Turn
    {
        get { return turn; }
        set
        {
            turn = value;
            if (turn >= PlayerNumber) turn = 0;
            for (int i = 0; i < PlayerNumber; i++)
            {
                Player p = Players[i];
                p.IsMyTurn = i == turn;
            }

            AudioManager.Instance.SoundPlay("OnForbid");
            if (CurrentMouseHoverBox)
            {
                CurrentMouseHoverBox.ChangeColor(CurrentPlayerColor);
            }
        }
    }

    public void InitChessBoard()
    {
        foreach (Player player in Players)
        {
            player.PoolRecycle();
        }

        Players.Clear();

        foreach (ChessBoardLine chessBoardLine in ChessBoardLines)
        {
            chessBoardLine.PoolRecycle();
        }

        ChessBoardLines.Clear();

        if (ChessPieceArr != null)
        {
            foreach (ChessPiece ch in ChessPieceArr)
            {
                if (ch) ch.PoolRecycle();
            }
        }

        if (int.TryParse(PlayerNumberInputField.text, out int num))
        {
            PlayerNumber = Mathf.Min(4, num);
        }

        string[] playerNames = new[] {"Player_A", "Player_B", "Player_C", "Player_D"};
        for (int i = 0; i < PlayerNumber; i++)
        {
            Player p = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<Player>(PlayersTrans);
            Players.Add(p);
            p.transform.position = PlayerPivots[i].position;
            p.Initialize(PlayerColors[i], playerNames[i], i);
        }

        if (int.TryParse(GridSize_W_InputField.text, out int w))
        {
            GridSize_W = Mathf.Min(20, Mathf.Max(2, w));
        }

        if (int.TryParse(GridSize_H_InputField.text, out int h))
        {
            GridSize_H = Mathf.Min(20, Mathf.Max(2, h));
        }

        CellSize = 900 / GridSize_H;
        ChessPieceArr = new ChessPiece[GridSize_W, GridSize_H];
        ChessBoardCollider.size = new Vector2(CellSize * GridSize_W, CellSize * GridSize_H);

        for (int i = -1; i < GridSize_W; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(ChessBoard);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(2, GridSize_H * CellSize);
            line.Rect.anchoredPosition = new Vector2(CellSize * (i - GridSize_W / 2.0f + 1), 0);
        }

        for (int i = -1; i < GridSize_H; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(ChessBoard);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(GridSize_W * CellSize, 2);
            line.Rect.anchoredPosition = new Vector2(0, CellSize * (i - GridSize_H / 2.0f + 1));
        }

        Turn = 0;
    }

    void Update()
    {
        UpdateMouseDown();
        UpdatePutChess();
        UpdatePickupChess();
        UpdateMouseHover();

        MousePos.text = Input.mousePosition.ToString();
        int[] pos = getClickPosition();
        if (pos != null)
        {
            MousePos2.text = "(" + pos[0] + "," + pos[1] + ")";
        }
        else
        {
            MousePos2.text = "";
        }
    }

    int[] lastFocusPos;
    internal MouseHoverBox CurrentMouseHoverBox;

    private void UpdateMouseHover()
    {
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
                CurrentMouseHoverBox = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.MouseHoverBox].AllocateGameObject<MouseHoverBox>(ChessBoard);
                ((RectTransform) (CurrentMouseHoverBox.transform)).anchoredPosition = GetPiecePosByXY(pos);
                CurrentMouseHoverBox.ChangeColor(CurrentPlayerColor);
                ((RectTransform) (CurrentMouseHoverBox.transform)).sizeDelta = new Vector2(CellSize, CellSize);
            }
        }
    }

    private Vector2 GetPiecePosByXY(int[] pos)
    {
        return new Vector2((pos[0] - (GridSize_W + 1) / 2 + 0.5f + (GridSize_W % 2) / 2f) * CellSize, (pos[1] - (GridSize_H + 1) / 2 + 0.5f + (GridSize_H % 2) / 2f) * CellSize);
    }

    private void UpdateMouseDown()
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

    bool mouseLeftDown = false;
    bool mouseRightDown = false;

    private void UpdatePutChess()
    {
        if (mouseLeftDown && Input.GetMouseButtonUp(0))
        {
            int[] pos = getClickPosition();
            if (pos == null) return;
            if (!ChessPieceArr[pos[0], pos[1]])
            {
                PutChess(pos);
            }

            mouseLeftDown = false;
        }
    }

    private void UpdatePickupChess()
    {
        if (mouseRightDown && Input.GetMouseButtonUp(1))
        {
            int[] pos = getClickPosition();
            if (pos == null) return;
            if (ChessPieceArr[pos[0], pos[1]])
            {
                RemoveChess(pos[0], pos[1]);
            }

            mouseLeftDown = false;
        }
    }

    private int[] getClickPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            int x = Mathf.RoundToInt((hit.point.x / CellSize * 100 + ((GridSize_W + 1) % 2) / 2f) / 1.15f);
            int y = Mathf.RoundToInt((hit.point.y / CellSize * 100 + ((GridSize_W + 1) % 2) / 2f) / 1.15f);
            if (x < -GridSize_W / 2 || y < -GridSize_H / 2 || x > GridSize_W / 2 || y > GridSize_H / 2)
            {
                return null;
            }

            x += (GridSize_W - 1) / 2;
            y += (GridSize_H - 1) / 2;
            return new int[] {x, y};
        }

        return null;
    }

    private void PutChess(int[] pos)
    {
        CurrentPlayer.PlaceLeftTimes--;
        ChessPiece chessPiece = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessPiece].AllocateGameObject<ChessPiece>(ChessBoard);
        ChessPieceArr[pos[0], pos[1]] = chessPiece;
        CurrentPlayer.TotalPieces++;
        chessPiece.Initialize(Turn, CurrentPlayerColor, CellSize);

        ((RectTransform) (chessPiece.transform)).anchoredPosition = GetPiecePosByXY(pos);
        chessPiece.Pos = new int[] {pos[0], pos[1]};
        AudioManager.Instance.SoundPlay("OnMove");
    }

    private void RemoveChess(int posX, int posY)
    {
        CurrentPlayer.PickOtherTimes--;
        CurrentPlayer.TotalPickup++;
        ChessPiece cp = ChessPieceArr[posX, posY];
        Players[cp.PlayerIndex].TotalPieces--;
        cp.PoolRecycle();
        ChessPieceArr[posX, posY] = null;
        AudioManager.Instance.SoundPlay("AttackGun");
    }
}