using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager {

    private Vector2Int boardSize;
    public GameObject[,] _board;

    public BoardManager(Vector2Int size)
    {
        boardSize = size;
        _board = new GameObject[boardSize.x, boardSize.y];
    }

    public void AddObject(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            int x = Mathf.FloorToInt(child.position.x);
            int y = Mathf.FloorToInt(child.position.y);
            _board[x, y] = child.gameObject;
            //Debug.Log(child.position.x + 5 + " " + child.position.y);
            //Debug.Log("Added on " + x + " " + y);
        }

        CheckLines();
    }

    public bool IsFull(int x, int y)
    {
        if (x >= boardSize.x || x < 0 || y >= boardSize.y || y < 0)
        {
            return true;
        }

        return  _board[x, y] != null;
    }

    public void CheckLines()
    {
        List<int> linesToClear = new List<int>();
        for (int i = 0; i < boardSize.x; i++)
        {
            if (Enumerable.Range(0, _board.GetLength(0)).Count(k => _board[k, i] != null) == 10)
            {
                DeleteLine(i);
                linesToClear.Add(i);
            }
        }

        if (linesToClear.Count() > 0)
        {
            //GameObject[,] tempArray = new GameObject[boardSize.x, boardSize.y];
            int timesShifted = 0;
            foreach (var line in linesToClear)
            {
                for (int j = line + 1 - timesShifted; j < boardSize.y; j++)
                {
                    for (int i = 0; i < boardSize.x; i++)
                    {
                        if (_board[i, j] != null)
                        {
                            _board[i, j].transform.position += new Vector3(0, -1, 0);
                            _board[i, j - 1] = _board[i, j];
                            _board[i, j] = null;
                        }
                    }
                }

                timesShifted += 1;
            }
            //_board = tempArray;
        }
    }

    public void DeleteLine(int line)
    {
        for (int i = 0; i < boardSize.x; i++)
        {
            Debug.Log("Destroying " + i + " " + line);
            Object.Destroy(_board[i, line]);
            _board[i, line] = null;
        }
    }
}
