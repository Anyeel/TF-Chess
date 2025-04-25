using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square
{

    public Square(Vector2Int position, GameObject prefab)
    {
        Vector3 position3D = new Vector3(position.x, 0, position.y);
        GameObject.Instantiate(prefab, position3D, Quaternion.identity);
    }
}
