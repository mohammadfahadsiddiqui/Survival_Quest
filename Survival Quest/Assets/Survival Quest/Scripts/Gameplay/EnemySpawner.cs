using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform player;

        // Keep the battlefield less crowded.
        [SerializeField] private int initialEnemies = 3;
        [SerializeField] private float spawnRadius = 25f;
        [SerializeField] private float minDistanceFromPlayer = 12f;
        [SerializeField] private float respawnDelay = 12f;
        [SerializeField] private int maxAliveEnemies = 6;

        private float nextRespawnTime;

        private void Start()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }

            for (int i = 0; i < initialEnemies; i++)
                SpawnEnemy();
        }

        private void Update()
        {
            if (Time.time < nextRespawnTime || transform.childCount >= maxAliveEnemies)
                return;

            nextRespawnTime = Time.time + respawnDelay;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                return;

            Vector3 center = player != null ? player.position : transform.position;
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minDistanceFromPlayer, spawnRadius);
            Vector3 position = new Vector3(center.x + offset.x, center.y, center.z + offset.y);

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (prefab != null)
                Instantiate(prefab, position, Quaternion.identity, transform);
        }
    }
}
