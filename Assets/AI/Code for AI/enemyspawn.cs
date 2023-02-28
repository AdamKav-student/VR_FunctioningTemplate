using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace AI.Code_for_AI
{
    public class EnemySpawn : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public Transform[] spawnPoints;
        public int numberOfEnemies;

        public float spawnDelay;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(SpawnEnemies());
        }

        IEnumerator SpawnEnemies()
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        void SpawnEnemy()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            var position = spawnPoint.position;
            GameObject enemy = Instantiate(enemyPrefab, position, spawnPoint.rotation);

            NavMeshAgent navMeshAgent = enemy.GetComponent<NavMeshAgent>();
            navMeshAgent.Warp(position);
        }
    }
}
