using Packages.MagicLoading.Runtime.Loading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPEArenaSpawnEnnemies : BasicObjectSpawner, INeedLinkFromSpawner
{
    public List<MagicAsset> EnnemyType;
    public override List<MagicAsset> ToSpawn { get => EnnemyType; set { value = EnnemyType; } }

    public GPEArenaManager arenaManager;

    public int EnnemiesSpawnedFromthisSpawner { get; set; }

    public void Spawn(MagicAsset ennemyTypeToSpawn, float delay)
    {
        EnnemyType.Clear();
        EnnemyType.Add(ennemyTypeToSpawn);
        if(EnnemyType.Count == 0 || EnnemyType[0] == null)
        {
            Debug.LogError("Wrong asset reference in Arena Spawner");
            return;
        }
        EnnemiesSpawnedFromthisSpawner++;
        if (delay <= 0)
            StartCoroutine(EffectiveSpawn());
        else
            Magic.Helpers.MagicUtility.RunAfter(() => StartCoroutine(EffectiveSpawn()), delay);
    }

    private void Start()
    {
        arenaManager.RegisterSpawner();
        EnnemiesSpawnedFromthisSpawner = 0;
    }

    public void Link(GameObject target)
    {
        if(target.GetComponent<MagicCustomController>())
        {
            arenaManager.RegisterEnnemies(this, target.GetComponent<MagicCustomController>());
        }
    }
}
