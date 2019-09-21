using System.Linq;
using Boo.Lang;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    private List<ChessBoardLine> ChessBoardLines = new List<ChessBoardLine>();
    [SerializeField] private BoxCollider Collider;

    void Start()
    {
        Collider.size = new Vector2(GamePlayManager.Instance.CellSize * GamePlayManager.Instance.GridSize_W, GamePlayManager.Instance.CellSize * GamePlayManager.Instance.GridSize_H);

        for (int i = -1; i < GamePlayManager.Instance.GridSize_W; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(transform);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(2, GamePlayManager.Instance.GridSize_H * GamePlayManager.Instance.CellSize);
            line.Rect.anchoredPosition = new Vector2(-(GamePlayManager.Instance.GridSize_W - 2) / 2.0f * GamePlayManager.Instance.CellSize + GamePlayManager.Instance.CellSize * i, 0);
        }

        for (int i = -1; i < GamePlayManager.Instance.GridSize_H; i++)
        {
            ChessBoardLine line = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChessBoardLine].AllocateGameObject<ChessBoardLine>(transform);
            ChessBoardLines.Add(line);
            line.Rect.sizeDelta = new Vector2(GamePlayManager.Instance.GridSize_W * GamePlayManager.Instance.CellSize, 2);
            line.Rect.anchoredPosition = new Vector2(0, -(GamePlayManager.Instance.GridSize_H - 2) / 2.0f * GamePlayManager.Instance.CellSize + GamePlayManager.Instance.CellSize * i);
        }
    }
}