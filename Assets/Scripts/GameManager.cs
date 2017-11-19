using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

    Object[] prefabs;
    private bool _isRunning = true;
    private Dictionary<string, Color> possibleColors;
    public BoardManager BoardManager;
    public int cubeSize = 1;
    public Vector2Int boardSize = new Vector2Int(10, 18);
    public GameObject spawner;
    public int speed = 1;

    public static GameManager instance = null; //Singleton

    void OnEnable()
    {
        Ticker.OnTick += OnTick;
        Piece.OnPieceLanded += SpawnPiece;
    }

    void OnDisable()
    {
        Ticker.OnTick -= OnTick;
        Piece.OnPieceLanded -= SpawnPiece;
    }

    void OnTick()
    {
        Debug.Log("Tic Tac");
    }

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);

        InitGame();
    }

    // Use this for initialization
    void Start()
    {
        //InvokeRepeating("spawnPiece", 0, 1.0f);
        //_board = new GameObject[boardSize.x][boardSize.x];
        SpawnPiece();
    }

    void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            speed = 5;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            speed = 1;
        }
    }

    void InitGame()
    {
        prefabs = Resources.LoadAll("Prefabs");
        BoardManager = new BoardManager(boardSize);
        possibleColors = new Dictionary<string, Color>();
    }

    public void SpawnPiece()
    {
        SpawnPiece(generatePiece());
    }

    void SpawnPiece(GameObject obj)
    {        
        float height, width, smallestY, largestY, smallestX, largestX;
        height = width = smallestY = largestY = smallestX = largestX = 0;

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            float localY = obj.transform.GetChild(i).localPosition.y;
            float localX = obj.transform.GetChild(i).localPosition.x;
            smallestY = Mathf.Min(localY, smallestY);
            largestY = Mathf.Max(localY, largestY);
            smallestX = Mathf.Min(localX, smallestX);
            largestX = Mathf.Max(localX, largestX);
        }
        height = (Mathf.Abs(smallestY - largestY) + 1) * cubeSize;
        width = (Mathf.Abs(smallestX - largestX) + 1) * cubeSize;
        obj.transform.position = new Vector3(Mathf.Round(boardSize.x / 2) + (0.5f * cubeSize), boardSize.y - (largestY + 0.5f) * cubeSize, -cubeSize);
        obj.GetComponent<Piece>().Height = Mathf.RoundToInt(height);
        obj.GetComponent<Piece>().Width = Mathf.RoundToInt(width);

        if (!obj.GetComponent<Piece>().CanMoveInDirection(Vector3.zero))
        {
            obj.GetComponent<Piece>().Deactivate();
            GameOver();
            return;
        }
        //Debug.Log(height);
        //Debug.Log("spawn" + new Vector3(Mathf.Round(boardSize.x / 2), boardSize.y - height / 2, -cubeSize));
    }

    GameObject getRandomFromPrefab()
    {
        if (prefabs.Length == 0)
        {
            return new GameObject();
        }

        int randIndex = Random.Range(0, prefabs.Length - 1);
        GameObject obj = (GameObject)Instantiate(prefabs[randIndex], new Vector3(boardSize.x + 10, boardSize.y + 1, -cubeSize), Quaternion.identity);

        return obj;
    }

    GameObject generatePiece()
    {
        Vector2Int[] direction = { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };
        Vector2Int[] pieces = new Vector2Int[4];
        pieces[0] = new Vector2Int(0, 0);
        int i = 0;

        while (i < 3)
        {
            List<Vector2Int> possible = direction.Select(vector => vector + pieces[i]).ToList();
            for (int j = 0; j < i + 1; j++)
            {
                possible.Remove(pieces[j]);
            }
            int chosenDirection = Random.Range(0, possible.Count());
            i++;
            pieces[i] = possible[chosenDirection];
        }

        GameObject generatedPiece = new GameObject();
        foreach (var piece in pieces)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localPosition = new Vector3(piece.x, piece.y, -cubeSize);
            cube.transform.parent = generatedPiece.transform.transform;
        }

        generatedPiece.SetActive(false);
        generatedPiece.AddComponent<Piece>();
        AssignColor(generatedPiece);
        generatedPiece.SetActive(true);
        return generatedPiece;
    }

    private void GameOver()
    {
        _isRunning = false;
    }

    private string GetSignature(GameObject obj)
    {
        List<Vector3> coordinates = new List<Vector3>();
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            coordinates.Add(obj.transform.GetChild(i).localPosition);
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

    private void AssignColor(GameObject obj)
    {
        int tries = 3;
        for (int i = 0; i < obj.transform.childCount; i++)
        Normalize(obj);
        string[] signatures = new string[tries];
        GameObject temp = Instantiate(obj);
        string signature = GetSignature(obj);

        if (possibleColors.Count() == 0)
        {
            Color chosenColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            possibleColors[signature] = chosenColor;
            SetColor(obj, chosenColor);
        } else if (possibleColors.ContainsKey(signature))
        {
            SetColor(obj, possibleColors[signature]);
            return;
        } else
        {
            for (int i = 0; i < tries; i++)
            {
                Normalize(temp);
                signature = GetSignature(temp);
                signatures[i] = signature;
                if (possibleColors.ContainsKey(signature))
                {
                    SetColor(obj, possibleColors[signature]);
                    return;
                }
            }

            Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            for (int i = 0; i < signatures.Length; i++)
            {
                possibleColors[signatures[i]] = color;
            }

            SetColor(obj, color);
        }

        Destroy(temp);
    }

    private void SetColor(GameObject obj, Color color)
    {
        Renderer[] renders = obj.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renders)
        {
            renderer.material.color = color;
        }
    }

    private bool CompareCoordinates(GameObject first, GameObject second)
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

    private void Normalize(GameObject obj)
    {
        float smallestX, smallestY;
        smallestX = smallestY = 0;

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            smallestX = Mathf.Min(obj.transform.GetChild(i).localPosition.x, smallestX);
            smallestY = Mathf.Min(obj.transform.GetChild(i).localPosition.y, smallestY);
        }
        smallestX = Mathf.Abs(smallestX);
        smallestY = Mathf.Abs(smallestY);
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).transform.localPosition = new Vector3(
                obj.transform.GetChild(i).localPosition.x + smallestX,
                obj.transform.GetChild(i).localPosition.y + smallestY,
                0);
        }
    }
}
