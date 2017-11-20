using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public delegate void PieceSpawned(GameObject me);
    public static event PieceSpawned OnPieceSpawn;
    public delegate void GameOver();
    public static event GameOver OnGameOver;
    public Object[] prefabs;

    private bool _isRunning = true;
    public BoardManager BoardManager;
    public int cubeSize = 1;
    public Vector2Int boardSize = new Vector2Int(10, 18);
    private Spawner spawner;
    public int speed = 1;

    public static GameManager instance = null; //Singleton
    
    void OnLevelWasLoaded(int index)
    {
        InitGame();
    }

    void OnEnable()
    {
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

        DontDestroyOnLoad(gameObject);
        InitGame();
    }

    // Use this for initialization
    void Start()
    {
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
        _isRunning = true;
        prefabs = Resources.LoadAll("Prefabs");
        BoardManager = new BoardManager(boardSize);
        AIBehaviour ai = new AIBehaviour();
        SpawnPrefab a = new SpawnPrefab();
        SetSpawner(a);
        SetupCamera();
        SetupBoard();
        SpawnPiece();
    }

    public void SetSpawner(Spawner spawn)
    {
        spawner = spawn;
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
        obj.transform.position = new Vector3(Mathf.Round(boardSize.x / 2) + (0.5f * cubeSize), boardSize.y - (height + 0.5f) * cubeSize, -cubeSize);
        obj.GetComponent<Piece>().height = Mathf.RoundToInt(height);
        obj.GetComponent<Piece>().width = Mathf.RoundToInt(width);

        if (!obj.GetComponent<Piece>().CanMoveInDirection(Vector3.zero))
        {
            obj.GetComponent<Piece>().Deactivate();
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        _isRunning = false;
        if (OnGameOver != null)
            OnGameOver();
    }
}
