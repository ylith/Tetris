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
        GameObject obj = (GameObject)GameObject.Instantiate(GameManager.instance.prefabs[randIndex]);
        obj.SetActive(false);
        obj.GetComponent<Piece>().SetDimensions();
        obj.transform.localScale = new Vector3(GameManager.instance.cubeSize, GameManager.instance.cubeSize, 1.0f);

        return obj;
    }
}
