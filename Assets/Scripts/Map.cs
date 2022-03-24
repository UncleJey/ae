using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    private static Map Instance;

    [SerializeField] private TileBase[]  walkable;

    private void Start()
    {
        Instance = this;
    }

    public static TileBase GetCell(Vector3 pos)
    {
        Vector3Int cell = Instance.grid.WorldToCell(pos);
        TileBase tb = Instance.tilemap.GetTile(cell);

        //Instance.tilemap.SetTile(cell, null);
        return tb;
    }

    /// <summary>
    /// Тайлы, сквозь которые можно проходить
    /// </summary>
    public static bool Walkable(TileBase tb, bool nullIsTrue = true)
    {
        if (tb == null)
            return nullIsTrue;
        
        for (int i=Instance.walkable.Length-1; i>=0; i--)
            if (Instance.walkable[i].Equals(tb))
                return true;
        return false;
    }

    public static bool Walkable(Vector3 pPoint)
    {
        return Walkable(GetCell(pPoint));
    }
}