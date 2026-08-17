using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    /// <summary>
    /// Lightweight procedural forest dressing controller.
    /// Assign tree/bush/rock prefabs and a parent transform, then place this on the scene.
    /// It only adds decorative objects and does not modify existing gameplay systems.
    /// </summary>
    public class ForestEnvironmentController : MonoBehaviour
    {
        [Header("Area")]
        [SerializeField] private Vector2 areaSize = new Vector2(80f, 80f);
        [SerializeField] private int seed = 12345;
        [SerializeField] private Transform player;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] treePrefabs;
        [SerializeField] private GameObject[] bushPrefabs;
        [SerializeField] private GameObject[] rockPrefabs;
        [SerializeField] private Transform forestParent;

        [Header("Density")]
        [SerializeField] private int treeCount = 80;
        [SerializeField] private int bushCount = 60;
        [SerializeField] private int rockCount = 35;
        [SerializeField] private float minDistanceFromPlayer = 6f;
        [SerializeField] private float maxScaleVariation = 0.25f;

        [ContextMenu("Generate Forest")]
        public void GenerateForest()
        {
            ClearForest();
            Random.InitState(seed);
            SpawnObjects(treePrefabs, treeCount, 1.0f);
            SpawnObjects(bushPrefabs, bushCount, 0.65f);
            SpawnObjects(rockPrefabs, rockCount, 0.8f);
        }

        [ContextMenu("Clear Forest")]
        public void ClearForest()
        {
            if (forestParent == null) return;
            for (int i = forestParent.childCount - 1; i >= 0; i--)
            {
                GameObject child = forestParent.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(child);
                else Destroy(child);
#else
                Destroy(child);
#endif
            }
        }

        private void SpawnObjects(GameObject[] prefabs, int count, float baseScale)
        {
            if (prefabs == null || prefabs.Length == 0 || forestParent == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                    0f,
                    Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

                if (player != null && Vector3.Distance(player.position, position) < minDistanceFromPlayer)
                {
                    i--;
                    continue;
                }

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject instance = Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), forestParent);
                float variation = Random.Range(1f - maxScaleVariation, 1f + maxScaleVariation);
                instance.transform.localScale *= baseScale * variation;
            }
        }
    }
}
