using System;
using UnityEngine;

namespace SurvivalQuest.Environment
{
    /// <summary>
    /// Lightweight procedural environment dressing for Survival Quest.
    /// Uses Unity primitives so the system works even when external art packs are unavailable.
    /// Attach to an empty GameObject in the scene and press Generate in the inspector/context menu.
    /// </summary>
    public class ProceduralEnvironmentGenerator : MonoBehaviour
    {
        [Header("Generation")]
        [SerializeField] private int seed = 2418;
        [SerializeField] private Vector2 worldSize = new Vector2(160f, 160f);
        [SerializeField] private int treeCount = 90;
        [SerializeField] private int bushCount = 120;
        [SerializeField] private int rockCount = 65;
        [SerializeField] private bool clearPreviousGeneration = true;

        [Header("Mountain Range")]
        [SerializeField] private int mountainCount = 12;
        [SerializeField] private Vector2 mountainHeight = new Vector2(12f, 30f);
        [SerializeField] private Vector2 mountainRadius = new Vector2(10f, 22f);

        [Header("River")]
        [SerializeField] private bool createRiver = true;
        [SerializeField] private float riverWidth = 5f;
        [SerializeField] private float riverDepth = 0.35f;
        [SerializeField] private int riverSegments = 34;
        [SerializeField] private float riverWander = 7f;

        [Header("Waterfall")]
        [SerializeField] private bool createWaterfall = true;
        [SerializeField] private float waterfallHeight = 7f;
        [SerializeField] private float waterfallWidth = 5f;

        [Header("Visual Polish")]
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material foliageMaterial;
        [SerializeField] private Material rockMaterial;

        private Transform generatedRoot;
        private System.Random rng;

        [ContextMenu("Generate Environment")]
        public void GenerateEnvironment()
        {
            rng = new System.Random(seed);

            if (clearPreviousGeneration && generatedRoot != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedRoot.gameObject);
                else
                    DestroyImmediate(generatedRoot.gameObject);
            }

            generatedRoot = new GameObject("__ProceduralEnvironment").transform;
            generatedRoot.SetParent(transform, false);

            CreateMountains();
            CreateRiver();
            CreateTrees();
            CreateBushes();
            CreateRocks();
            CreateWaterfall();
        }

        [ContextMenu("Clear Generated Environment")]
        public void ClearGeneratedEnvironment()
        {
            var existing = transform.Find("__ProceduralEnvironment");
            if (existing == null)
                return;

            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);

            generatedRoot = null;
        }

        private void CreateMountains()
        {
            var parent = CreateGroup("Mountains");

            for (int i = 0; i < mountainCount; i++)
            {
                float angle = (float)i / Mathf.Max(1, mountainCount) * Mathf.PI * 2f;
                float distance = Mathf.Min(worldSize.x, worldSize.y) * 0.45f;
                Vector3 center = new Vector3(
                    Mathf.Cos(angle) * distance + Rand(-7f, 7f),
                    0f,
                    Mathf.Sin(angle) * distance + Rand(-7f, 7f));

                float height = Rand(mountainHeight.x, mountainHeight.y);
                float radius = Rand(mountainRadius.x, mountainRadius.y);
                CreateMountain(parent, center, radius, height);
            }
        }

        private void CreateMountain(Transform parent, Vector3 center, float radius, float height)
        {
            GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mountain.name = "Mountain";
            mountain.transform.SetParent(parent, false);
            mountain.transform.position = center + Vector3.up * height * 0.35f;
            mountain.transform.localScale = new Vector3(radius, height * 0.7f, radius);
            ApplyMaterial(mountain, rockMaterial);

            // Snow cap / secondary rock mass for a more natural silhouette.
            GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            peak.name = "MountainPeak";
            peak.transform.SetParent(parent, false);
            peak.transform.position = center + Vector3.up * height * 0.72f;
            peak.transform.localScale = new Vector3(radius * 0.48f, height * 0.3f, radius * 0.48f);
            ApplyMaterial(peak, rockMaterial);
        }

        private void CreateRiver()
        {
            if (!createRiver)
                return;

            Transform parent = CreateGroup("River");
            Vector3 previous = new Vector3(-worldSize.x * 0.5f, -riverDepth, Rand(-15f, 15f));

            for (int i = 0; i < riverSegments; i++)
            {
                float t = i / (float)Mathf.Max(1, riverSegments - 1);
                float z = Mathf.Lerp(-worldSize.y * 0.35f, worldSize.y * 0.35f, t) + Mathf.Sin(t * Mathf.PI * 2f) * riverWander;
                Vector3 next = new Vector3(Mathf.Lerp(-worldSize.x * 0.5f, worldSize.x * 0.5f, t), -riverDepth, z);

                Vector3 midpoint = (previous + next) * 0.5f;
                Vector3 direction = next - previous;
                float length = direction.magnitude;

                GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "RiverSegment";
                water.transform.SetParent(parent, false);
                water.transform.position = midpoint;
                water.transform.localScale = new Vector3(riverWidth, 0.18f, length + 0.2f);
                water.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                ApplyMaterial(water, waterMaterial);

                previous = next;
            }
        }

        private void CreateWaterfall()
        {
            if (!createWaterfall)
                return;

            Transform parent = CreateGroup("Waterfall");
            Vector3 basePos = new Vector3(worldSize.x * 0.18f, waterfallHeight * 0.5f - riverDepth, 0f);

            GameObject fall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fall.name = "WaterfallCurtain";
            fall.transform.SetParent(parent, false);
            fall.transform.position = basePos;
            fall.transform.localScale = new Vector3(waterfallWidth, waterfallHeight, 0.35f);
            ApplyMaterial(fall, waterMaterial);

            GameObject plunge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plunge.name = "WaterfallPool";
            plunge.transform.SetParent(parent, false);
            plunge.transform.position = basePos + new Vector3(0f, -waterfallHeight * 0.5f, 2f);
            plunge.transform.localScale = new Vector3(waterfallWidth * 0.8f, 0.12f, waterfallWidth * 0.8f);
            ApplyMaterial(plunge, waterMaterial);
        }

        private void CreateTrees()
        {
            Transform parent = CreateGroup("Trees");

            for (int i = 0; i < treeCount; i++)
            {
                Vector3 p = SampleGroundPoint(6f);
                CreateTree(parent, p);
            }
        }

        private void CreateTree(Transform parent, Vector3 position)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.SetParent(parent, false);
            trunk.transform.position = position + Vector3.up * 1.7f;
            trunk.transform.localScale = new Vector3(0.35f, 1.7f, 0.35f);
            ApplyMaterial(trunk, rockMaterial);

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "TreeCrown";
            crown.transform.SetParent(parent, false);
            crown.transform.position = position + Vector3.up * 4.0f;
            float scale = Rand(1.8f, 2.7f);
            crown.transform.localScale = Vector3.one * scale;
            ApplyMaterial(crown, foliageMaterial);
        }

        private void CreateBushes()
        {
            Transform parent = CreateGroup("Bushes");

            for (int i = 0; i < bushCount; i++)
            {
                Vector3 p = SampleGroundPoint(2.5f);
                GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "Bush";
                bush.transform.SetParent(parent, false);
                bush.transform.position = p + Vector3.up * Rand(0.45f, 0.8f);
                float uniform = Rand(0.8f, 1.35f);
                bush.transform.localScale = new Vector3(uniform * 1.2f, uniform, uniform);
                ApplyMaterial(bush, foliageMaterial);
            }
        }

        private void CreateRocks()
        {
            Transform parent = CreateGroup("Rocks");

            for (int i = 0; i < rockCount; i++)
            {
                Vector3 p = SampleGroundPoint(1.5f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock";
                rock.transform.SetParent(parent, false);
                rock.transform.position = p + Vector3.up * Rand(0.25f, 0.8f);
                rock.transform.localScale = new Vector3(Rand(0.5f, 1.6f), Rand(0.35f, 1f), Rand(0.5f, 1.4f));
                rock.transform.rotation = UnityEngine.Random.rotation;
                ApplyMaterial(rock, rockMaterial);
            }
        }

        private Vector3 SampleGroundPoint(float avoidRiverDistance)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                float x = Rand(-worldSize.x * 0.5f, worldSize.x * 0.5f);
                float z = Rand(-worldSize.y * 0.5f, worldSize.y * 0.5f);
                if (Mathf.Abs(z) > avoidRiverDistance)
                    return new Vector3(x, 0f, z);
            }

            return Vector3.zero;
        }

        private Transform CreateGroup(string groupName)
        {
            GameObject group = new GameObject(groupName);
            group.transform.SetParent(generatedRoot, false);
            return group.transform;
        }

        private void ApplyMaterial(GameObject target, Material material)
        {
            if (material != null)
                target.GetComponent<Renderer>().sharedMaterial = material;
        }

        private float Rand(float min, float max)
        {
            return (float)(rng.NextDouble() * (max - min) + min);
        }
    }
}
