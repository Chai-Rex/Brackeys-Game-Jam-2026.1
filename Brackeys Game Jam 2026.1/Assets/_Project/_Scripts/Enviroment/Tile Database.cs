using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public Dictionary<string, TileData> tileDatabase;
    [SerializeField]private List<TileData> tileList;

    private void Awake()
    {
        tileDatabase = new Dictionary<string, TileData>();
        foreach (TileData tileData in tileList)
        {
            tileDatabase.Add(tileData.tileName, tileData);
        }
    }
}
