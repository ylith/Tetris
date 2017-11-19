using UnityEngine;

public class UserInput : MonoBehaviour {

    Ticker ticker;

    private void Start()
    {
        ticker = gameObject.GetComponent<Ticker>();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ticker.tickDuration = Mathf.Max(ticker.minTickDuration, ticker.tickDuration - ticker.tickStep);
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ticker.tickDuration = Mathf.Min(ticker.maxTickDuration, ticker.tickDuration + ticker.tickStep);
        }
    }
}
