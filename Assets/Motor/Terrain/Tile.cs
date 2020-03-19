﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    NONE,
    GROUND,
    WATER
}

public class Tile : MonoBehaviour
{
    [SerializeField]
    private TerrainType type;

    public TerrainType Type
    {
        get { return type; }
        set { return; }
    }
}
