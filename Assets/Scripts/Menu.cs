using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public GameObject menu;
    public GameObject gameOver;
    public Dropdown pieceGeneration;

    private void OnEnable()
    {
        GameManager.OnGameOver += ShowMenu;
        GameManager.OnGameOver += ShowGameOver;

        DontDestroyOnLoad(gameObject);
        Init();
    }

    void OnLevelWasLoaded(int index)
    {
        Init();
    }

    void Init()
    {
        gameOver.GetComponent<Text>().enabled = false;
        menu.SetActive(false);
        pieceGeneration.onValueChanged.AddListener(
            delegate
            {
                SwitchPieceGeneration();
            });
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ShowGameOver()
    {
        gameOver.GetComponent<Text>().enabled = true;
    }

    void ShowMenu()
    {
        menu.SetActive(true);
    }

    void ToggleMenu()
    {
        if (menu.gameObject.activeSelf)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
        }
    }

    public void SwitchPieceGeneration()
    {
        switch (pieceGeneration.value)
        {
            case 0:
                GameManager.instance.SetSpawner(new SpawnPrefab());
                break;
            case 1:
                GameManager.instance.SetSpawner(new SpawnRandom());
                break;
        }
    }
}
