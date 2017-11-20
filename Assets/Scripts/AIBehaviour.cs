using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AIBehaviour {

    BoardManager boardManager;
    GameObject currentPiece;
    GameObject currentPieceCopy;
    Vector3 direction;

    Dictionary<Vector3, Vector3Int> directions = new Dictionary<Vector3, Vector3Int>(); // keep best values for all directions
    Dictionary<Vector3, int> rotations = new Dictionary<Vector3, int>(); // number of rotations for a direction

    public AIBehaviour()
    {
        GameManager.OnPieceSpawn += OnPieceSpawn;
        Ticker.OnTick += MoveWithDirection;
        boardManager = new BoardManager(GameManager.instance.boardSize); // new custom board
        boardManager.check = false; // do not treat AI pieces as regular pieces
        
        GameManager.OnGameOver += Stop;
    }

    void Stop()
    {
        GameManager.OnPieceSpawn -= OnPieceSpawn;
    }

    void OnPieceSpawn(GameObject obj)
    {
        boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,]; // deep copy of board
        currentPiece = obj;
        currentPieceCopy = GameObject.Instantiate(obj); // get a clone of current piece
        currentPieceCopy.SetActive(false);
        currentPieceCopy.GetComponent<Piece>().Interactable = false; // make it non interactable so it does not fire events
        AttemptMove();
        GameObject.Destroy(currentPieceCopy);
    }

    // move with chosen direction on each tick
    void MoveWithDirection()
    {
        if (direction.x != 0)
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
        Vector3 origPos = currentPieceCopy.transform.position;
        Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        directions = new Dictionary<Vector3, Vector3Int>();
        rotations = new Dictionary<Vector3, int>();

        currentPieceCopy.transform.position = currentPiece.transform.position;

        CheckMove(Vector3.zero); // check current position and all possible rotations
        CheckMove(left); // check left position and all possible rotations
        currentPieceCopy.transform.position = origPos; // reset to middle
        CheckMove(right); // check right position and all possible rotations

        // order directions by best potential to clear, then by least holes on the board, then by lowest column height
        directions = directions.OrderByDescending(z => z.Value.z).ThenBy(x => x.Value.x).ThenBy(y => y.Value.y).ToDictionary(x => x.Key, x => x.Value);
        direction = directions.First().Key;

        for (int i = 0; i < rotations[direction]; i++)
        {
            currentPiece.GetComponent<Piece>().RotateClockwise();
        }
    }

    private void CheckMove(Vector3 dir)
    {
        Vector3 down = new Vector3(0.0f, -1.0f, 0.0f);
        int times = 0;
        Piece script = currentPieceCopy.GetComponent<Piece>();

        while (script.CanMoveInDirection(dir, boardManager))
        {
            script.Move(dir);
            times++;
            GameObject tempPiece = GameObject.Instantiate(currentPieceCopy); // make copy of current to test rotations
            Piece tempScript = tempPiece.GetComponent<Piece>();
            tempPiece.GetComponent<Piece>().Interactable = false;
            tempPiece.name = "temp";
            tempPiece.SetActive(false);
            rotations[dir * times] = 0;
            int rotate = tempScript.height == tempScript.width ? 1 : 4; // do not rotate if square
            for (int i = 0; i < rotate; i++)
            {
                tempPiece.transform.position = currentPieceCopy.transform.position;
                if (i != 0) // rotate only after current position is checked
                {
                    tempScript.RotateClockwise();
                    while (! tempScript.CanMoveInDirection(Vector3.zero, boardManager) || ! tempScript.CanMoveInDirection(down, boardManager))
                    {
                        tempScript.RotateCounterClockwise();
                        tempScript.Move(down);
                        tempScript.RotateCounterClockwise(); //rotate back if move is invalid
                    }
                }
                while (tempScript.CanMoveInDirection(down, boardManager))
                {
                    tempScript.Move(down); // go down and try to collide
                }
                
                boardManager.AddObject(tempPiece);
                int holes = boardManager.getHolesByColumn().Sum();
                int highestY = boardManager.getHeightByColumn().Max();
                int willClear = boardManager.getFullLines();

                // keep best values for direction
                if (!directions.ContainsKey(dir * times))
                {
                    directions[dir * times] = new Vector3Int(holes, highestY, willClear);
                    rotations[dir * times] = i;
                } else if (willClear > directions[dir * times].z)
                {
                    directions[dir * times] = new Vector3Int(holes, highestY, willClear);
                    rotations[dir * times] = i;
                }
                else if (holes < directions[dir * times].x)
                {
                    directions[dir * times] = new Vector3Int(holes, highestY, willClear);
                    rotations[dir * times] = i;
                }
                else if (highestY < directions[dir * times].y)
                {
                    directions[dir * times] = new Vector3Int(holes, highestY, willClear);
                    rotations[dir * times] = i;
                }
                
                boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,];
            }

            GameObject.Destroy(tempPiece);

            if (dir == Vector3.zero)
            {
                break;
            }
        }
    }
}
