using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManagerSpawn : MonoBehaviour
{
    [SerializeField] private Transform[] spawners;

    [SerializeField] private GameObject zombie;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            SpawnZombie();
        }
    }

    private void SpawnZombie()
    {
        int randomInt = Random.Range(1, spawners.Length);
        Transform randomSapwner = spawners[randomInt];

        Instantiate(zombie, randomSapwner.position, randomSapwner.rotation);
    }
}
