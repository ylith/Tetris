using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Ticker ticker = Camera.main.GetComponent<Ticker>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void Delete()
    {
        Ticker ticker = Camera.main.GetComponent<Ticker>();
        StartCoroutine(Blink(ticker.tickDuration / GameManager.instance.CurrentSpeed, 0.2f));
        Destroy(gameObject, ticker.tickDuration / GameManager.instance.CurrentSpeed);
    }

    IEnumerator Blink(float duration, float blinkInterval)
    {
        float nextBlink = 0;
        while (duration > 0)
        {
            duration -= Time.fixedDeltaTime;
            if (nextBlink <= 0)
            {
                GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
                nextBlink = blinkInterval;
            }

            nextBlink -= Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        GetComponent<Renderer>().enabled = true;
    }

    IEnumerator MoveCoroutine(Vector3 dir, float delay)
    {
        if (delay > 0)
        {
            delay -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log(transform.position);
        Move(dir);
    }

    public void MoveWithDirection(Vector3 dir, float delay = 0)
    {
        Vector3 pos = transform.position + dir;
        StartCoroutine(MoveCoroutine(pos, delay));
    }

    public void Move(Vector3 dir)
    {
        Debug.Log("Move");
        transform.position = dir;
    }
}
