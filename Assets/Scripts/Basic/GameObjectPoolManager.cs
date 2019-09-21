using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolManager : MonoSingleton<GameObjectPoolManager>
{
    private GameObjectPoolManager()
    {
    }

    public enum PrefabNames
    {
        MouseHoverBox,
        Chess_Blue,
        Chess_Green,
        Chess_Red,
        Chess_Yellow,
        ChessBoardLine,
    }

    public static PrefabNames[] ChessColors = new[]
    {
        PrefabNames.Chess_Blue,
        PrefabNames.Chess_Yellow,
        PrefabNames.Chess_Red,
        PrefabNames.Chess_Green,
    };

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>
    {
        {PrefabNames.MouseHoverBox, 2},
        {PrefabNames.Chess_Blue, 2},
        {PrefabNames.Chess_Green, 2},
        {PrefabNames.Chess_Red, 2},
        {PrefabNames.Chess_Yellow, 2},
        {PrefabNames.ChessBoardLine, 50},
    };

    public Dictionary<PrefabNames, int> PoolWarmUpDict = new Dictionary<PrefabNames, int>
    {
    };

    public Dictionary<PrefabNames, GameObjectPool> PoolDict = new Dictionary<PrefabNames, GameObjectPool>();

    void Awake()
    {
        foreach (KeyValuePair<PrefabNames, int> kv in PoolConfigs)
        {
            string prefabName = kv.Key.ToString();
            GameObject go = new GameObject("Pool_" + prefabName);
            GameObjectPool pool = go.AddComponent<GameObjectPool>();
            PoolDict.Add(kv.Key, pool);
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (!go_Prefab)
            {
                continue;
            }

            PoolObject po = go_Prefab.GetComponent<PoolObject>();
            if (!po)
            {
                continue;
            }

            pool.Initiate(po, kv.Value);
            pool.transform.SetParent(transform);
        }
    }

    public void OptimizeAllGameObjectPools()
    {
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            kv.Value.OptimizePool();
        }
    }
}