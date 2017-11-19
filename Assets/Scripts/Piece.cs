using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    private Vector3 _moveDirection = Vector3.zero;
    private bool _isMoving = true;
    private int height = 0;
    private int width = 0;
    double timeSinceUpdate = 0;
    bool tick = false;

    private int speed = 1;

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

    // Update is called once per frame
    void Update()
    {
        tick = false;
        timeSinceUpdate += Time.deltaTime;

        if (timeSinceUpdate >= (GameManager.instance.tickDuration / speed))
        {
            tick = true;
            timeSinceUpdate -= (GameManager.instance.tickDuration / speed);
        }

        if (_isMoving)
        {
            Move();
        }
    }

    void Move()
    {
        _moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && CanMoveInDirection(new Vector3(-1, 0, 0)))
        {
            _moveDirection = new Vector3(-1.0f, 0.0f, 0.0f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && CanMoveInDirection(new Vector3(1, 0, 0)))
        {
            _moveDirection = new Vector3(1.0f, 0.0f, 0.0f);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            speed = 5;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateClockwise();
            if (! CanMoveInDirection(Vector3.zero))
            {
                RotateCounterClockwise();
            }
            //transform.Rotate(new Vector3(0, 0, 90));
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            speed = 1;
        }

        _moveDirection = transform.TransformDirection(_moveDirection);
        //_moveDirection.y -= 3.0f * Time.deltaTime;
        if (tick)
        {
            _moveDirection.y -= 1.0f;
        }

        Vector3 pos = gameObject.transform.position;
        pos += _moveDirection;

        if (! CanMoveInDirection(_moveDirection))
        {
            _isMoving = false;
        }

        if (! _isMoving)
        {
            GameManager.instance.BoardManager.AddObject(gameObject);
            transform.DetachChildren();
            Destroy(gameObject);
            GameManager.instance.SpawnPiece();
            //Debug.Log(pos);
        } else
        {
            gameObject.transform.position = pos;
        }
        //rb.MovePosition(transform.position + _moveDirection);
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
            //Debug.Log(gameObject.transform.GetChild(i).position);
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
