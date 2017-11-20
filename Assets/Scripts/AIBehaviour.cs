using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AIBehaviour {

    BoardManager boardManager;
    GameObject currentPiece;
    GameObject currentPieceCopy;
    Vector3 direction;

    Dictionary<Vector3, Vector2Int> directions = new Dictionary<Vector3, Vector2Int>();
    Dictionary<Vector3, int> rotations = new Dictionary<Vector3, int>();

    public AIBehaviour()
    {
        GameManager.OnPieceSpawn += OnPieceSpawn;
        Ticker.OnTick += MoveWithDirection;
        boardManager = new BoardManager(GameManager.instance.boardSize);
        boardManager.check = false;
    }

    void OnPieceSpawn(GameObject obj)
    {
        boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,];
        currentPiece = obj;
        currentPieceCopy = GameObject.Instantiate(obj);
        currentPieceCopy.name = "copy";
        currentPieceCopy.SetActive(false);
        currentPieceCopy.GetComponent<Piece>().Interactable = false;
        AttemptMove();
        GameObject.Destroy(currentPieceCopy);
    }

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
        directions = new Dictionary<Vector3, Vector2Int>();
        rotations = new Dictionary<Vector3, int>();

        currentPieceCopy.transform.position = currentPiece.transform.position;

        CheckMove(Vector3.zero);
        CheckMove(left);
        CheckMove(right);

        Debug.Log(directions);
        directions = directions.OrderBy(x => x.Value.x).ThenBy(y => y.Value.y).ToDictionary(x => x.Key, x => x.Value);
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
            for (int k = 0; k < currentPieceCopy.transform.childCount; k++)
            {
                float localY = currentPieceCopy.transform.GetChild(k).position.y;
                float localX = currentPieceCopy.transform.GetChild(k).position.x;
                Debug.Log(localX + " " + localY);
            }
            GameObject tempPiece = GameObject.Instantiate(currentPieceCopy);
            Piece tempScript = tempPiece.GetComponent<Piece>();
            tempPiece.GetComponent<Piece>().Interactable = false;
            tempPiece.name = "temp";
            tempPiece.SetActive(false);
            rotations[dir * times] = 0;
            for (int i = 0; i < 4; i++)
            {
                tempPiece.transform.position = currentPieceCopy.transform.position;
                while (tempScript.CanMoveInDirection(down, boardManager))
                {
                    tempScript.Move(down);
                }

                //Debug.Log("Adding " + tempPiece.transform.position);
                boardManager.AddObject(tempPiece);
                int holes = boardManager.getHolesByColumn().Sum();
                int highestY = boardManager.getHeightByColumn().Max();
                if (!directions.ContainsKey(dir * times))
                {
                    directions[dir * times] = new Vector2Int(holes, highestY);
                }
                else if (holes < directions[dir * times].x)
                {
                    directions[dir * times] = new Vector2Int(holes, highestY);
                    rotations[dir * times] = i;
                }
                else if (highestY < directions[dir * times].y)
                {
                    directions[dir * times] = new Vector2Int(holes, highestY);
                    rotations[dir * times] = i;
                }

                //Debug.Log("Rotate");
                //tempScript.RotateClockwise();
                boardManager._board = GameManager.instance.BoardManager._board.Clone() as GameObject[,];
            }

            GameObject.Destroy(tempPiece);
        }
    }
}
