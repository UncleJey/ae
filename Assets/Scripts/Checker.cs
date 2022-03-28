using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Checker : MonoBehaviour
{
    private Hero hero;

    void Awake()
    {
        hero = GetComponent<Hero>();
    }

    void Update()
    {
        if (Map.GetCell(hero.CurrentCenter, out TileBase tb, out Vector3Int cell))
        {
            Debug.Log(tb.name);
        }
    }
}
