using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AIBehaviour {

    private bool _isRunning = false;
    BoardManager boardManager;
    GameObject currentPiece;
    GameObject currentPieceCopy;
    Vector3 direction;

    Dictionary<Vector3, List<Vector4>> directions = new Dictionary<Vector3, List<Vector4>>(); // keep best values for all directions

    public AIBehaviour()
    {
        _isRunning = true;
        GameManager.OnPieceSpawn += OnPieceSpawn;
        Ticker.OnTick += MoveWithDirection;
        boardManager = new BoardManager(GameManager.instance.boardSize); // new custom board
        boardManager.check = false; // do not treat AI pieces as regular pieces
        
        GameManager.OnGameOver += Stop;
    }

    void Stop()
    {
        _isRunning = false;
        GameManager.OnPieceSpawn -= OnPieceSpawn;
        Ticker.OnTick -= MoveWithDirection;
        boardManager = null;
    }

    void OnPieceSpawn(GameObject obj)
    {
        if ( ! _isRunning)
        {
            return;
        }
        if (! obj.GetComponent<Piece>().CanMoveInDirection(Vector3.zero)) //is valid position
        {
            return;
        }
        boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,]; // deep copy of board
        currentPiece = obj;
        AttemptMove();
        GameObject.Destroy(currentPieceCopy);
    }

    // move with chosen direction on each tick
    void MoveWithDirection()
    {
        if (direction.x != 0 && _isRunning)
        {
            float x = direction.x / Mathf.Abs(direction.x);
            if (currentPiece.GetComponent<Piece>().CanMoveInDirection(new Vector3(x, 0.0f, 0.0f)))
            {
                currentPiece.GetComponent<Piece>().Move(new Vector3(x, 0.0f, 0.0f));
                direction = new Vector3(direction.x - x, direction.y, 0.0f);
            } else
            {
                direction = Vector3.zero;
            }
        }
    }

    void AttemptMove()
    {
        direction = Vector3.zero;
        Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 down = new Vector3(0.0f, -1.0f, 0.0f);
        directions = new Dictionary<Vector3, List<Vector4>>();
        currentPieceCopy = GameObject.Instantiate(currentPiece); // get a clone of current piece
        currentPieceCopy.SetActive(false);
        currentPieceCopy.GetComponent<Piece>().Interactable = false;
        Piece script = currentPieceCopy.GetComponent<Piece>();// make it non interactable so it does not fire events

        for (int j = 0; j < 4; j++) // number of rotations
        {
            CheckMove(left, (int)Mathf.Abs(script.SmallestX), (int)currentPiece.transform.position.x, j);
            CheckMove(right, (int)currentPiece.transform.position.x + 1, (int)(GameManager.instance.boardSize.x - script.LargestX), j);
            script.RotateClockwise();
        }

        // order directions by best potential to clear, then by least holes on the board, then by lowest column height
        //directions = directions.OrderBy(x => x.Value.x).ThenBy(y => y.Value.y).ToDictionary(x => x.Key, x => x.Value);
        Vector4 test = new Vector4(0, 1000, 1000, 0);
        foreach (KeyValuePair<Vector3, List<Vector4>> dir in directions)
        {
            foreach (var item in dir.Value)
            {
                if (item.x > test.x)
                {
                    test = item;
                    direction = dir.Key;
                } else if (item.x == test.x && item.y < test.y)
                {
                    test = item;
                    direction = dir.Key;
                } else if (item.x == test.x && item.y == test.y && item.z < test.z)
                {
                    test = item;
                    direction = dir.Key;
                }
            }
        }
        
        for (int i = 0; i < test.w; i++)
        {
            currentPiece.GetComponent<Piece>().RotateClockwise();
            if (! currentPiece.GetComponent<Piece>().CanMoveInDirection(Vector3.zero))
            {
                Stop();
            }
        }
    }

    private void CheckMove(Vector3 dir, int start, int end, int timesRotated = 0)
    {
        Vector3 down = new Vector3(0.0f, -1.0f, 0.0f);
        int times = 0;

        for (int i = start; i <= end; i++)
        {
            GameObject temp = GameObject.Instantiate(currentPieceCopy); // get a clone of current piece
            temp.SetActive(false);
            temp.GetComponent<Piece>().Interactable = false;
            Piece script = temp.GetComponent<Piece>();
            script.Move(dir * times);

            if (! script.CanMoveInDirection(Vector3.zero))
            {
                break;
            }

            while (script.CanMoveInDirection(down, boardManager))
            {
                script.Move(down); // go down and try to collide
            }

            boardManager.AddObject(temp);
            int holes = boardManager.getHolesByColumn().Sum();
            int highestY = boardManager.getHeightByColumn().Max();
            int willClear = boardManager.getFullLines();

            // keep best values for direction
            if (!directions.ContainsKey(dir * times))
            {
                directions[dir * times] = new List<Vector4> { new Vector4(willClear, holes, highestY, timesRotated) };
            } else
            {
                directions[dir * times].Add(new Vector4(willClear, holes, highestY, timesRotated));
            }

            boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,];
            times++;
            GameObject.Destroy(temp);
        }
    }
}
