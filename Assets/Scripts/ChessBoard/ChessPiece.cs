using UnityEngine;
using UnityEngine.UI;

public class ChessPiece : PoolObject
{
    internal int[] Pos;
    public int PlayerIndex;
    [SerializeField] private Image Image;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
        Pos = new int[] {0, 0};
    }

    public void Initialize(int playerIndex,Color color, float size)
    {
        PlayerIndex = playerIndex;
        Image.color = color;
        ((RectTransform) transform).sizeDelta = new Vector2(size, size);
    }

    void Start()
    {
    }

    void Update()
    {
    }
}