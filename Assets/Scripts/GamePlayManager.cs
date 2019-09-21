using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoSingleton<GamePlayManager>
{
    private List<Player> Players = new List<Player>();
    internal Player CurrentPlayer => Players[turn];
    public List<Color> PlayerColors = new List<Color>();
    [SerializeField] private List<Transform> PlayerPivots = new List<Transform>();

    public int PlayerNumber = 4;

    void Start()
    {
        string[] playerNames = new[] {"Player_A", "Player_B", "Player_C", "Player_D"};
        for (int i = 0; i < PlayerNumber; i++)
        {
            Player p = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<Player>(transform);
            Players.Add(p);
            p.transform.position = PlayerPivots[i].position;
            p.Initialize(PlayerColors[i], playerNames[i], i);
        }

        Turn = 0;
    }

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
            if (ChessBoard.Instance && ChessBoard.Instance.CurrentMouseHoverBox)
            {
                ChessBoard.Instance.CurrentMouseHoverBox.ChangeColor(PlayerColors[Turn]);
            }
        }
    }
}