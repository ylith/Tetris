using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager {

    private Vector2Int boardSize;
    public GameObject[,] _board;
    public bool check = true;

    public BoardManager(Vector2Int size)
    {
        boardSize = size;
        _board = new GameObject[boardSize.x, boardSize.y];
        Piece.OnPieceLanded += AddObject;
    }

    ~BoardManager()
    {
        Piece.OnPieceLanded -= AddObject;
    }

    public void AddObject(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            int x = Mathf.FloorToInt(child.position.x);
            int y = Mathf.FloorToInt(child.position.y);
            
            _board[x, y] = child.gameObject;
        }

        if(check) //skip check for AI
        {
            CheckLines();
        }

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
            if (Enumerable.Range(0, _board.GetLength(0)).Count(k => _board[k, i] != null) == boardSize.x) //is row full
            {
                DeleteLine(i);
                linesToClear.Add(i);
            }
        }

        if (linesToClear.Count() > 0)
        {
            int timesShifted = 0; //shift rows for each clered row
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

    public int[] getHolesByColumn()
    {
        int[] holes = new int[boardSize.x]; //number of holes on the board

        for (int i = 0; i < boardSize.x; i++)
        {
            int largestTakenY = 0;
            for (int j = 0; j < boardSize.y; j++)
            {
                if (_board[i, j] != null)
                {
                    largestTakenY = j;
                }
            }
            for (int j = 0; j < largestTakenY; j++)
            {
                if (_board[i, j] == null)
                {
                    holes[i] += 1;
                }
            }
        }

        return holes;
    }

    public int[] getHeightByColumn()
    {
        int[] columns = new int[boardSize.x];

        for (int i = 0; i < boardSize.x; i++)
        {
            int largestTakenY = 0;
            for (int j = 0; j < boardSize.y; j++)
            {
                if (_board[i, j] != null)
                {
                    largestTakenY = j;
                }
            }
            columns[i] = largestTakenY;
        }

        return columns;
    }

    public int getFullLines()
    {
        int fullLines = 0;
        for (int i = 0; i < boardSize.x; i++)
        {
            if (Enumerable.Range(0, _board.GetLength(0)).Count(k => _board[k, i] != null) == 10)
            {
                fullLines++;
            }
        }

        return fullLines;
    }
}
