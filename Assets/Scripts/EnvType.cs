using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ActivateOn : byte
{
    None = 0,
    OnStart = 1,
    OnStepIn = 2,
    OnStepOut = 3,
    Hero = 4
}

[System.Serializable]
public class EnvType
{
    public string name;
    public TileBase tile;
    public GameObject prefab;
    public ActivateOn activate;
    public GameObject effect;
    public float effectTime;

}
