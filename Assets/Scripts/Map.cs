using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    private static Map Instance;

    public static Transform MapTransform => Instance.transform; 
    
    /// <summary>
    /// Дистанция для взаимодействия с объектом
    /// </summary>
    private float checkDist = 0.12f;

    /// <summary>
    /// размер ячейки
    /// </summary>
    public static Vector3 CellSize => Instance.grid.cellSize;

    /// <summary>
    /// тайлы сквозь которые можно ходить / лазать
    /// </summary>
    [SerializeField] private TileBase[] walkable;

    /// <summary>
    /// Типы окружения
    /// </summary>
    [SerializeField] private EnvType[] types;

    /// <summary>
    /// Управляемые сущности
    /// </summary>
    [SerializeField] private List<GameObject> env = new List<GameObject>();

    private void Start()
    {
        Instance = this;
        MakeAlive();
    }

    #region map

    /// <summary>
    /// тип тайла
    /// </summary>
    private bool GetType(TileBase tileBase, out EnvType et)
    {
        et = null;

        if (tileBase == null)
            return false;

        for (int i = types.Length - 1; i >= 0; i--)
        {
            et = types[i];
            if (et.tile.Equals(tileBase))
                return true;
        }

        et = null;
        return false;
    }

    /// <summary>
    /// Оживление сущностей на карте
    /// </summary>
    private void MakeAlive()
    {
        GameController.Quads = 0;
        Vector3Int v = Vector3Int.zero;
        for (int i = -20; i < 20; i++)
        {
            v.x = i;
            for (int j = -20; j < 20; j++)
            {
                v.y = j;
                if (GetType(tilemap.GetTile(v), out EnvType type))
                {
                    if (type.activate == ActivateOn.Hero)
                    {
                        // set position 
                        tilemap.SetTile(v, null);
                        GameController.Hero.position = CellToWorld(v);
                    }
                    else if (type.activate == ActivateOn.OnStart)
                    {
                        tilemap.SetTile(v, null);
                        Debug.Log("activate " + type.name);

                        GameObject go = Instantiate(type.prefab, transform, true);
                        go.transform.position = CellToWorld(v);
                        env.Add(go);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Уничтожить объект на карте
    /// </summary>
    /// <param name="go"></param>
    public static void DestoryObj(GameObject go)
    {
        Instance.env.Remove(go);
        GameObject.Destroy(go);
    }

    /// <summary>
    /// Получить адрес ячейки и саму ячейку по позиции в пространстве
    /// </summary>
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


    #endregion map

    #region props

    /// <summary>
    /// Можно ли пройти по ячейке
    /// </summary>
    public static bool Walkable(Vector3 pPoint)
    {
        GetCell(pPoint, out TileBase tb, out Vector3Int cell);
        return Walkable(tb);
    }

    /// <summary>
    /// Адрес ячейки в пространственные координаты
    /// </summary>
    public static Vector3 CellToWorld(Vector3Int pPoint)
    {
        return Instance.grid.CellToWorld(pPoint);
    }

    /// <summary>
    /// Пространственные координаты в адрес ячейки
    /// </summary>
    public static Vector3Int WorldToCell(Vector3 pPoint)
    {
        return Instance.grid.WorldToCell(pPoint);
    }

    /// <summary>
    /// Округление пространственных координаты до ширины ячейки
    /// </summary>
    public static Vector3 Trim(Vector3 pPoint)
    {
        return CellToWorld(WorldToCell(pPoint));
    }

    #endregion props

    #region events

    private void LateUpdate()
    {
        if (GameController.Hero.Flipping)
            return;

        for (int i = env.Count - 1; i >= 0; i--)
        {
            GameObject go = env[i];
            if (go != null && go.activeSelf)
            {
                if ((go.transform.position - GameController.Hero.transform.position).sqrMagnitude < checkDist)
                {
                    ITouchReceiver touch = go.GetComponent<ITouchReceiver>();
                    touch?.TouchAction(true);
                }
            }
        }
    }

    #endregion events


    #region Serialize

#if UNITY_EDITOR
    /// <summary>
    /// Сохранение карты
    /// </summary>
    /// <returns></returns>
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
#endif

    #endregion serialize
}