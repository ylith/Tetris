using UnityEngine;

public class SpawnPrefab : Spawner {

    // spawn a piece from prefab directory
    public override GameObject Spawn()
    {
        if (GameManager.instance.prefabs.Length == 0)
        {
            return new GameObject();
        }

        int randIndex = Random.Range(0, GameManager.instance.prefabs.Length - 1);
        GameObject obj = (GameObject)GameObject.Instantiate(GameManager.instance.prefabs[randIndex],
            new Vector3(GameManager.instance.boardSize.x + 10, GameManager.instance.boardSize.y + 1, -GameManager.instance.cubeSize), Quaternion.identity);
        obj.transform.localScale = new Vector3(GameManager.instance.cubeSize, GameManager.instance.cubeSize, 0);

        return obj;
    }
}
