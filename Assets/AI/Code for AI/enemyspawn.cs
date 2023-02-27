using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class enemyspawn : MonoBehaviour
{

    
    public GameObject Enemy;

    public Transform[] spawnPoints;

    public int numberofEnemies;

    public float spawnDelay;






    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine Spawnenemy());
    }

    



// Update is called once per frame
    void Update()
    {
       
    }
}

Void Spawnenemy(){
    int spawnIndex = Random.Range(0, spawnPoints.Length);
    Transform spawnPoints = spawnPoints[spawnIndex];

    GameObject enemy = Instantiate(Enemy, spawnPoints.position, spawnPoints.rotation);

    NavMeshAgent newMeshAgent = enemy.GetComponent<NavMeshAgent>();
    navMeshAgent.warp(spawnPoint.positon)
}

IEnumerator spawnenimies()
{
    for (int i = 0; i< numberofEnemies,i++)
    {
        Spawnenemy();
        yield return new WaitForSeconds(spawnDelay);
    }
}