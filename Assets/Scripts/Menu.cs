using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    private GameObject menu;
    public Dropdown pieceGeneration;

    private void OnEnable()
    {
        GameManager.OnGameOver += ShowMenu;
        GameManager.OnGameOver += ShowGameOver;
    }

    // Use this for initialization
    void Start () {
        menu = GameObject.Find("Menu");
        menu.SetActive(false);
        GameObject.Find("GameOver").GetComponent<Text>().enabled = false;
        pieceGeneration.onValueChanged.AddListener(
            delegate
            {
                SwitchPieceGeneration();
            });
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
        GameObject.Find("GameOver").GetComponent<Text>().enabled = true;
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
