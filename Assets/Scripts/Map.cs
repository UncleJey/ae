﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    private static Map Instance;

    [SerializeField] private TileBase[] walkable;

    private void Start()
    {
        Instance = this;
    }

    public static bool GetCell(Vector3 pos, out TileBase tb, out Vector3Int cell)
    {
        cell = Instance.grid.WorldToCell(pos);
        tb = Instance.tilemap.GetTile(cell);
        return tb != null;
    }

    /// <summary>
    /// Тайлы, сквозь которые можно проходить
    /// </summary>
    public static bool Walkable(TileBase tb, bool nullIsTrue = true)
    {
        if (tb == null)
            return nullIsTrue;

        for (int i = Instance.walkable.Length - 1; i >= 0; i--)
            if (Instance.walkable[i].Equals(tb))
                return true;
        return false;
    }

    public static bool Walkable(Vector3 pPoint)
    {
        GetCell(pPoint, out TileBase tb, out Vector3Int cell);
        return Walkable(tb);
    }

    public static Vector3 CellToWorld(Vector3Int pPoint)
    {
        return Instance.grid.CellToWorld(pPoint);
    }

    public static Vector3Int WorldToCell(Vector3 pPoint)
    {
        return Instance.grid.WorldToCell(pPoint);
    }

    public static Vector3 Trim(Vector3 pPoint)
    {
        return CellToWorld(WorldToCell(pPoint));
    }

    /// <summary>
    /// размер ячейки
    /// </summary>
    public static Vector3 CellSize => Instance.grid.cellSize;

    public static string Serialize()
    {
        string data = "";
        Vector3Int v = Vector3Int.zero;
        for (int i = -20; i < 20; i++)
        {
            v.x = i;
            string line = "";
            bool was = false;
            for (int j = -20; j < 20; j++)
            {
                v.y = j;
                TileBase tb = Instance.tilemap.GetTile(v);
                if (tb != null)
                {
                    line += tb.name;
                    was = true;
                }

                line += ",";
            }
            if (was)
                data += $"{i}:{line}\r\n";
        }

        return data;
    }
    
}