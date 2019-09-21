using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : PoolObject
{
    [SerializeField] private Text Text;
    [SerializeField] private Image MyTurnBG;

    [SerializeField] private Text PlaceLeftTimes_Text;
    [SerializeField] private Text MoveTimes_Text;
    [SerializeField] private Text PickOtherTimes_Text;

    public void Initialize(Color playerColor, string playerName, int playerIndex)
    {
        Text.color = playerColor;
        Text.text = playerName;
        PlaceLeftTimes_Text.text = "";
        MoveTimes_Text.text = "";
        PickOtherTimes_Text.text = "";
        PlayerIndex = playerIndex;
    }

    public int PlayerIndex;

    private bool isMyTurn;

    public bool IsMyTurn
    {
        get { return isMyTurn; }
        set
        {
            isMyTurn = value;
            MyTurnBG.enabled = value;
        }
    }

    public void OnDiceButtonClick()
    {
        if ((GamePlayManager.Instance.CurrentPlayer.PlayerIndex == GamePlayManager.Instance.PlayerNumber - 1 && PlayerIndex == 0)
            || GamePlayManager.Instance.CurrentPlayer.PlayerIndex == PlayerIndex - 1)
        {
            GamePlayManager.Instance.Turn++;
            return;
        }
        else if (GamePlayManager.Instance.CurrentPlayer == this)
        {
            PlaceLeftTimes = Random.Range(0, 5);
            MoveTimes = Random.Range(0, 5);
            PickOtherTimes = Random.Range(0, 5);
            AudioManager.Instance.SoundPlay("Dice");
        }
    }

    private int placeLeftTimes;

    public int PlaceLeftTimes
    {
        get { return placeLeftTimes; }
        set
        {
            placeLeftTimes = value;
            PlaceLeftTimes_Text.text = placeLeftTimes.ToString();
        }
    }

    private int moveTimes;

    public int MoveTimes
    {
        get { return moveTimes; }
        set
        {
            moveTimes = value;
            MoveTimes_Text.text = moveTimes.ToString();
        }
    }

    private int pickOtherTimes;

    public int PickOtherTimes
    {
        get { return pickOtherTimes; }
        set
        {
            pickOtherTimes = value;
            PickOtherTimes_Text.text = pickOtherTimes.ToString();
        }
    }
}