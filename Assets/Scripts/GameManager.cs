using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public delegate void PieceSpawned(GameObject me);
    public static event PieceSpawned OnPieceSpawn;
    public delegate void GameOver();
    public static event GameOver OnGameOver;

    public Object[] prefabs;
    public bool isAi = false;
    public BoardManager BoardManager;
    public int cubeSize = 1;
    public Vector2Int boardSize = new Vector2Int(10, 18);
    public int speed = 1;

    private bool _isRunning = true;
    private int currentSpeed = 1;
    private Spawner spawner;
    private bool keysEnabled = true;
    private AIBehaviour ai;

    public static GameManager instance = null; //Singleton

    public bool KeysEnabled
    {
        get {return keysEnabled; }

        set{ keysEnabled = value; }
    }

    public int CurrentSpeed
    {
        get { return currentSpeed; }

        set { currentSpeed = value; }
    }

    void OnLevelWasLoaded(int index)
    {
        InitGame();
    }

    void OnEnable()
    {
        // Attach listeners
        Ticker.OnTick += OnTick;
        Piece.OnPieceLanded += OnPieceLanded;
    }

    void OnDisable()
    {
        Ticker.OnTick -= OnTick;
        Piece.OnPieceLanded -= OnPieceLanded;
    }

    void OnTick()
    {
        //Debug.Log("Tic Tac");
    }

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject); //do not destroy when changing scene
        InitGame();
    }

    void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && keysEnabled)
        {
            CurrentSpeed = 5;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && keysEnabled)
        {
            CurrentSpeed = 1;
        }
    }

    void InitGame()
    {
        speed = 1;
        currentSpeed = 1;
        _isRunning = true;
        prefabs = Resources.LoadAll("Prefabs");
        BoardManager = new BoardManager(boardSize);
        if (isAi)
        {
            ai = new AIBehaviour();
            keysEnabled = false;
        }
        SpawnPrefab a = new SpawnPrefab();
        SetSpawner(a);
        SetupCamera();
        SetupBoard();
        SpawnPiece();
    }

    public void SetSpawner(Spawner spawn)
    {
        spawner = spawn; // set spawn piece algorithm
    }

    void SetupCamera()
    {
        Camera.main.orthographicSize = (float)Mathf.Max(boardSize.x, boardSize.y) / 2;
        Camera.main.transform.position = new Vector3((float)boardSize.x / 2, (float)boardSize.y / 2, -Camera.main.orthographicSize * 2);
    }

    void SetupBoard()
    {
        GameObject back = GameObject.Find("Back");
        back.transform.localScale = new Vector3((float)boardSize.x, (float)boardSize.y, back.transform.localScale.z);
        back.transform.position = new Vector3((float)boardSize.x / 2, (float)boardSize.y / 2, back.transform.position.z);
    }

    void OnPieceLanded(GameObject sender)
    {
        SpawnPiece();
    }

    void SpawnPiece()
    {
        if(! _isRunning)
        {
            return;
        }
        GameObject piece = spawner.Spawn();
        SpawnPiece(piece);

        if (OnPieceSpawn != null)
            OnPieceSpawn(piece);
    }

    void SpawnPiece(GameObject obj)
    {
        int height = obj.GetComponent<Piece>().height;
        int width = obj.GetComponent<Piece>().width;

        obj.transform.position = new Vector3(Mathf.Round(boardSize.x / 2) + (0.5f * cubeSize), boardSize.y - obj.GetComponent<Piece>().LargestY * cubeSize -0.5f, -cubeSize);
        obj.GetComponent<Piece>().height = Mathf.RoundToInt(height);
        obj.GetComponent<Piece>().width = Mathf.RoundToInt(width);

        if (!obj.GetComponent<Piece>().CanMoveInDirection(Vector3.zero)) //is valid position
        {
            obj.GetComponent<Piece>().Deactivate();
            TriggerGameOver();
            return;
        }

        obj.SetActive(true);
    }

    private void TriggerGameOver()
    {
        _isRunning = false;
        ai = null;

        if (OnGameOver != null)
            OnGameOver();

    }
}
