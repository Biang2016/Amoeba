using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseHoverBox : PoolObject
{
    void Start()
    {
        ((RectTransform) transform).sizeDelta = new Vector2(ChessBoard.Instance.CellSize * 1.5f, ChessBoard.Instance.CellSize * 1.5f);
    }

    public Image[] Images;

    public void ChangeColor(Color color)
    {
        foreach (Image image in Images)
        {
            image.color = color;
        }
    }
}