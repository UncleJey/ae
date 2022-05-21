using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Порождаемый на карте объект
/// </summary>
[System.Serializable]
public class EnvType
{
    public string name;

    /// <summary>
    /// Обозначение на карте
    /// </summary>
    public TileBase tile;

    public GameObject prefab;

    /// <summary>
    /// Когда активировать
    /// </summary>
    public ActivateOn activate;
}