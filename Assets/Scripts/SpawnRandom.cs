using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnRandom : Spawner {
    
    private Dictionary<string, Color> possibleColors = new Dictionary<string, Color>();
    private List<Color> goodColors = new List<Color> { Color.red, Color.blue, Color.green, Color.magenta, Color.yellow, Color.cyan, new Color(1.0f, 0.65f, 0.0f) };

    public override GameObject Spawn()
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
            cube.transform.localPosition = new Vector3(piece.x, piece.y, -GameManager.instance.cubeSize);
            cube.transform.parent = generatedPiece.transform.transform;
        }

        generatedPiece.SetActive(false);
        generatedPiece.AddComponent<Piece>();
        generatedPiece.GetComponent<Piece>().SetDimensions();
        AssignColor(generatedPiece);
        generatedPiece.SetActive(true);
        return generatedPiece;
    }

    private void AssignColor(GameObject obj)
    {
        int tries = 3;
        string[] signatures = new string[tries];
        var objScript = obj.GetComponent<Piece>();
        objScript.Normalize();
        GameObject temp = GameObject.Instantiate(obj);
        var tempScript = temp.GetComponent<Piece>();
        string signature = objScript.GetSignature();

        if (possibleColors.Count() == 0)
        {
            int index = Random.Range(0, goodColors.Count - 1);
            Color chosenColor = goodColors[index];
            goodColors.RemoveAt(index);
            possibleColors[signature] = chosenColor;
            objScript.SetColor(chosenColor);
        }
        else if (possibleColors.ContainsKey(signature))
        {
            objScript.SetColor(possibleColors[signature]);
            return;
        }
        else
        {
            for (int i = 0; i < tries; i++)
            {
                tempScript.RotateClockwise();
                tempScript.Normalize();
                signature = tempScript.GetSignature();
                signatures[i] = signature;
                if (possibleColors.ContainsKey(signature))
                {
                    objScript.SetColor(possibleColors[signature]);
                    return;
                }
            }

            int index = Random.Range(0, goodColors.Count - 1);
            Color chosenColor = goodColors[index];
            goodColors.RemoveAt(index);
            for (int i = 0; i < signatures.Length; i++)
            {
                possibleColors[signatures[i]] = chosenColor;
            }

            objScript.SetColor(chosenColor);
        }

        GameObject.Destroy(temp);
    }
}
