using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    private bool _isMoving = true;
    private int height = 0;
    private int width = 0;

    public delegate void PieceLand();
    public static event PieceLand OnPieceLanded;


    public int Height
    {
        get
        {
            return height;
        }

        set
        {
            height = value;
        }
    }

    public int Width
    {
        get
        {
            return width;
        }

        set
        {
            width = value;
        }
    }

    void OnEnable()
    {
        Ticker.OnTick += OnTick;
    }


    void OnDisable()
    {
        Ticker.OnTick -= OnTick;
    }

    void OnTick()
    {
        Vector3 moveDirection = new Vector3(0.0f, -1.0f, 0.0f);
        Move(moveDirection);
    }

    // Update is called once per frame
    void Update()
    {
        ListenForKeys();
    }

    void ListenForKeys()
    {
        if (_isMoving)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) && CanMoveInDirection(new Vector3(-1, 0, 0)))
            {
                Vector3 moveDirection = new Vector3(-1.0f, 0.0f, 0.0f);
                Move(moveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && CanMoveInDirection(new Vector3(1, 0, 0)))
            {
                Vector3 moveDirection = new Vector3(1.0f, 0.0f, 0.0f);
                Move(moveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                RotateClockwise();
                if (!CanMoveInDirection(Vector3.zero))
                {
                    RotateCounterClockwise();
                }
            }
        }
    }

    void Move(Vector3 moveDirection)
    {
        moveDirection = transform.TransformDirection(moveDirection);
        Vector3 pos = gameObject.transform.position;
        pos += moveDirection;

        if (! CanMoveInDirection(moveDirection))
        {
            _isMoving = false;
        }

        if (! _isMoving)
        {
            if (OnPieceLanded != null)
                OnPieceLanded();
            GameManager.instance.BoardManager.AddObject(gameObject);
            transform.DetachChildren();
            Destroy(gameObject);
        } else
        {
            gameObject.transform.position = pos;
        }
    }

    public void RotateClockwise()
    {
        if (height == width)
        {
            return;
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Vector3 temp = new Vector3(gameObject.transform.GetChild(i).localPosition.y, -gameObject.transform.GetChild(i).localPosition.x, gameObject.transform.GetChild(i).localPosition.z);
            gameObject.transform.GetChild(i).localPosition = temp;
        }
    }

    public void RotateCounterClockwise()
    {
        if (height == width)
        {
            return;
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Vector3 temp = new Vector3(-gameObject.transform.GetChild(i).localPosition.y, gameObject.transform.GetChild(i).localPosition.x, gameObject.transform.GetChild(i).localPosition.z);
            gameObject.transform.GetChild(i).localPosition = temp;
        }
    }

    public bool CanMoveInDirection(Vector3 direction)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            int x = Mathf.FloorToInt(gameObject.transform.GetChild(i).position.x + direction.x);
            int y = Mathf.FloorToInt(gameObject.transform.GetChild(i).position.y + direction.y);
            if (GameManager.instance.BoardManager.IsFull(x, y))
            {
                return false;
            }
        }

        return true;
    }

    public void Deactivate()
    {
        enabled = false;
    }
}
