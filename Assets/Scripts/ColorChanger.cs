using System.Collections;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{

    SpriteRenderer ChessBoardSR;
    [SerializeField] private Color MainColor;
    private Color nextColor;
    private Color lastColor;

    public float Change;
    void Awake()
    {
        ChessBoardSR = gameObject.GetComponent<SpriteRenderer>();
        nextColor = new Color(MainColor.r + Random.Range(-Change, Change), MainColor.g + Random.Range(-Change, Change), MainColor.b + Random.Range(-Change, Change), 1);
        lastColor = nextColor;
        changeInteval = Random.Range(2f, 4f);
    }

    void Start()
    {
        StartCoroutine(produceNextColor());
        StartCoroutine(changeColor());
    }

    void Update()
    {

    }

    float changeInteval = 3f;
    IEnumerator produceNextColor()
    {
        while (true)
        {
            lastColor = nextColor;
            nextColor = new Color(MainColor.r + Random.Range(-Change, Change), MainColor.g + Random.Range(-Change, Change), MainColor.b + Random.Range(-Change, Change), 1);
            changeTicker = 0;
            yield return new WaitForSeconds(changeInteval);
        }
    }

    float showInteval = 0.05f;
    float changeTicker = 0;
    IEnumerator changeColor()
    {
        while (true)
        {
            changeTicker += showInteval;
            float ratio = changeTicker / changeInteval;
            ChessBoardSR.color = new Color(ratio * (nextColor.r - lastColor.r) + lastColor.r, ratio * (nextColor.g - lastColor.g) + lastColor.g, ratio * (nextColor.b - lastColor.b) + lastColor.b, ratio * (nextColor.a - lastColor.a) + lastColor.a);
            yield return new WaitForSeconds(showInteval);
        }
    }
}