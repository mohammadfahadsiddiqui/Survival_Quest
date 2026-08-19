using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    /// <summary>
    /// Forest controller. Attach this script only to the
    /// "SURVIVAL QUEST FOREST" GameObject.
    ///
    /// This script does not modify enemies, the player, UI, or other scenes.
    /// Assign forest prefabs in the Inspector if you want to use the generator.
    /// </summary>
    public class ForestEnvironmentController : MonoBehaviour
    {
        [Header("Forest Area")]
        [SerializeField] private Vector2 areaSize = new Vector2(80f, 80f);
        [SerializeField] private int seed = 240817;

        [Header("Forest Prefabs")]
        [SerializeField] private GameObject[] treePrefabs;
        [SerializeField] private GameObject[] bushPrefabs;
        [SerializeField] private GameObject[] rockPrefabs;

        [Header("Density")]
        [SerializeField] private int treeCount = 45;
        [SerializeField] private int bushCount = 85;
        [SerializeField] private int rockCount = 12;
        [SerializeField] private float maxScaleVariation = 0.3f;

        [ContextMenu("Generate Forest")]
        public void GenerateForest()
        {
            Random.InitState(seed);

            Generate(treePrefabs, treeCount, 1f);
            Generate(bushPrefabs, bushCount, 0.7f);
            Generate(rockPrefabs, rockCount, 1f);
        }

        private void Generate(GameObject[] prefabs, int count, float baseScale)
        {
            if (prefabs == null || prefabs.Length == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab == null)
                    continue;

                Vector3 position = new Vector3(
                    Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                    0f,
                    Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

                GameObject instance = Instantiate(
                    prefab,
                    transform.position + position,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                    transform);

                float scale = baseScale * Random.Range(
                    1f - maxScaleVariation,
                    1f + maxScaleVariation);

                instance.transform.localScale *= scale;
            }
        }
    }
}
