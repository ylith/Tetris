using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour {
    private bool _isMoving = true;
    private bool interactable = true;
    private float _largestY;
    private float _largestX;
    private float _smallestX;
    private float _smallestY;
    public int height = 0;
    public int width = 0;

    public bool Interactable
    {
        get { return interactable; }
        set{ interactable = value; }
    }

    public float LargestY
    {
        get { return _largestY; }
        private set { _largestY = value; }
    }

    public float LargestX
    {
        get { return _largestX; }
        private set { _largestX = value; }
    }

    public float SmallestX
    {
        get { return _smallestX; }
        private set { _smallestX = value; }
    }
    
    public float SmallestY
    {
        get { return _smallestY; }
        private set { _smallestY = value; }
    }

    public delegate void PieceLand(GameObject me);
    public static event PieceLand OnPieceLanded;

    void OnEnable()
    {
        Ticker.OnTick += OnTick;
        SetDimensions();
    }

    void OnDisable()
    {
        Ticker.OnTick -= OnTick;
    }

    void OnTick()
    {
        if (interactable)
        {
            Vector3 moveDirection = new Vector3(0.0f, -GameManager.instance.cubeSize, 0.0f);
            Move(moveDirection); //attempt to move down
        }
    }

    // Update is called once per frame
    void Update()
    {
        ListenForKeys();
    }

    void ListenForKeys()
    {
        if (_isMoving && GameManager.instance.KeysEnabled)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) && CanMoveInDirection(new Vector3(-GameManager.instance.cubeSize, 0, 0))) //left
            {
                Vector3 moveDirection = new Vector3(-GameManager.instance.cubeSize, 0.0f, 0.0f);
                Move(moveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && CanMoveInDirection(new Vector3(GameManager.instance.cubeSize, 0, 0))) //right
            {
                Vector3 moveDirection = new Vector3(GameManager.instance.cubeSize, 0.0f, 0.0f);
                Move(moveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) //up
            {
                RotateClockwise();
                if (!CanMoveInDirection(Vector3.zero)) //is valid rotation
                {
                    RotateCounterClockwise();
                }
            }
        }
    }

    public void Move(Vector3 moveDirection)
    {
        moveDirection = transform.TransformDirection(moveDirection);
        Vector3 pos = gameObject.transform.position;
        pos += moveDirection;

        if (! CanMoveInDirection(moveDirection))
        {
            _isMoving = false;
        }

        if (! _isMoving && interactable) 
        {
            if (OnPieceLanded != null)
                OnPieceLanded(gameObject);
            //GameManager.instance.BoardManager.AddObject(gameObject);
            transform.DetachChildren();
            Destroy(gameObject);

        } else
        {
            gameObject.transform.position = pos;
        }
    }

    public void SetDimensions()
    {
        float height, width, smallestY, largestY, smallestX, largestX;
        height = width = smallestY = largestY = smallestX = largestX = 0;

        for (int k = 0; k < gameObject.transform.childCount; k++)
        {
            float localY = gameObject.transform.GetChild(k).localPosition.y;
            float localX = gameObject.transform.GetChild(k).localPosition.x;
            smallestY = Mathf.Min(localY, smallestY);
            largestY = Mathf.Max(localY, largestY);
            smallestX = Mathf.Min(localX, smallestX);
            largestX = Mathf.Max(localX, largestX);
        }

        height = (Mathf.Abs(smallestY - largestY) + 1) * GameManager.instance.cubeSize;
        width = (Mathf.Abs(smallestX - largestX) + 1) * GameManager.instance.cubeSize;
        this.height = Mathf.RoundToInt(height);
        this.width = Mathf.RoundToInt(width);
        _largestY = largestY;
        _largestX = largestX;
        _smallestX = smallestX;
        _smallestY = smallestY;

    }

    public void Rotate(Vector3 coords)
    {

        if (height == width)
        {
            return;
        }

        for (int i = 0; i < gameObject.transform.childCount; i++) // swap coordinates x -> y, y -> -x
        {
            Vector3 temp = new Vector3(
                coords.x * gameObject.transform.GetChild(i).localPosition.y,
                coords.y * gameObject.transform.GetChild(i).localPosition.x,
                coords.z * gameObject.transform.GetChild(i).localPosition.z);
            gameObject.transform.GetChild(i).localPosition = temp;
        }

        SetDimensions();
        if (gameObject.transform.position.y + _largestY > GameManager.instance.boardSize.y)
        {
            Move(new Vector3(0, -_largestY, 0));
        }
    }

    public void RotateClockwise()
    {
        Rotate(new Vector3(1, -1, 1));
    }

    public void RotateCounterClockwise()
    {
        Rotate(new Vector3(-1, 1, 1));
    }

    // check if  movement is valid for all children
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

    // check if  movement is valid for all children for custom board
    public bool CanMoveInDirection(Vector3 direction, BoardManager boardManager)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            int x = Mathf.FloorToInt(gameObject.transform.GetChild(i).position.x + direction.x);
            int y = Mathf.FloorToInt(gameObject.transform.GetChild(i).position.y + direction.y);
            if (boardManager.IsFull(x, y))
            {
                return false;
            }
        }

        return true;
    }

    // get unique concatenated coordinates of all children sorted by x, then y
    public string GetSignature()
    {
        List<Vector3> coordinates = new List<Vector3>();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            coordinates.Add(gameObject.transform.GetChild(i).localPosition);
        }

        coordinates = coordinates.OrderBy(o => o.x).ThenBy(c => c.y).ToList();
        string signature = "";
        foreach (var coord in coordinates)
        {
            signature += coord.x.ToString();
            signature += coord.y.ToString();
        }

        return signature;
    }

    public void SetColor(Color color)
    {
        Renderer[] renders = gameObject.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renders)
        {
            renderer.material.color = color;
        }
    }

    public bool CompareCoordinates(GameObject first, GameObject second)
    {
        for (int i = 0; i < first.transform.childCount; i++)
        {
            for (int j = 0; j < second.transform.childCount; j++)
            {
                if (first.transform.GetChild(i).transform.localPosition.x != second.transform.GetChild(i).localPosition.x ||
                    first.transform.GetChild(i).transform.localPosition.y != second.transform.GetChild(i).localPosition.y)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // normalize local position for all children, so they are all positive
    public void Normalize()
    {
        float smallestX, smallestY;
        smallestX = smallestY = 0;

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            smallestX = Mathf.Min(gameObject.transform.GetChild(i).localPosition.x, smallestX);
            smallestY = Mathf.Min(gameObject.transform.GetChild(i).localPosition.y, smallestY);
        }
        smallestX = Mathf.Abs(smallestX);
        smallestY = Mathf.Abs(smallestY);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).transform.localPosition = new Vector3(
                gameObject.transform.GetChild(i).localPosition.x + smallestX,
                gameObject.transform.GetChild(i).localPosition.y + smallestY,
                0);
        }
    }

    public void Deactivate()
    {
        enabled = false;
    }
}
