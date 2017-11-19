using UnityEngine;

public class Ticker : MonoBehaviour {

    public delegate void Tick();
    public static event Tick OnTick;

    public float minTickDuration = 0.05f;
    public float maxTickDuration = 0.5f;
    public float tickStep = 0.05f;
    public float tickDuration = 0.5f;
    private double _timeSinceUpdate = 0;

    // Update is called once per frame
    void Update()
    {
        _timeSinceUpdate += Time.deltaTime;

        if (_timeSinceUpdate >= (tickDuration / GameManager.instance.speed))
        {
            if (OnTick != null)
                OnTick();
            _timeSinceUpdate -= (tickDuration / GameManager.instance.speed);
        }
    }
}
