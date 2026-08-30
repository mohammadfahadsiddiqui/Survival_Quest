using System;
using UnityEngine;

namespace SurvivalQuest.Environment
{
    /// <summary>Procedural environment dressing for Survival Quest. Attach to an empty GameObject and generate from the component menu.</summary>
    public class ProceduralEnvironmentGenerator : MonoBehaviour
    {
        [Header("Generation")]
        [SerializeField] private int seed = 2418;
        [SerializeField] private Vector2 worldSize = new Vector2(180f, 180f);
        [SerializeField] private int treeCount = 120;
        [SerializeField] private int bushCount = 180;
        [SerializeField] private int rockCount = 90;
        [SerializeField] private bool clearPreviousGeneration = true;

        [Header("Mountains")]
        [SerializeField] private int mountainCount = 14;
        [SerializeField] private Vector2 mountainHeight = new Vector2(16f, 36f);
        [SerializeField] private Vector2 mountainRadius = new Vector2(12f, 28f);

        [Header("River")]
        [SerializeField] private bool createRiver = true;
        [SerializeField] private float riverWidth = 6f;
        [SerializeField] private float riverDepth = 0.35f;
        [SerializeField] private int riverSegments = 42;
        [SerializeField] private float riverWander = 10f;

        [Header("Waterfall")]
        [SerializeField] private bool createWaterfall = true;
        [SerializeField] private float waterfallHeight = 12f;
        [SerializeField] private float waterfallWidth = 6f;

        [Header("Atmosphere")]
        [SerializeField] private bool createFireflies = true;
        [SerializeField] private int fireflyCount = 45;
        [SerializeField] private bool createMist = true;
        [SerializeField] private int mistCount = 12;

        [Header("Optional Materials")]
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material foliageMaterial;
        [SerializeField] private Material rockMaterial;
        [SerializeField] private Material mistMaterial;

        private Transform generatedRoot;
        private System.Random rng;

        [ContextMenu("Generate Environment")]
        public void GenerateEnvironment()
        {
            rng = new System.Random(seed);
            if (clearPreviousGeneration) ClearGeneratedEnvironment();
            generatedRoot = new GameObject("__ProceduralEnvironment").transform;
            generatedRoot.SetParent(transform, false);

            CreateMountains();
            CreateRiver();
            CreateTrees();
            CreateBushes();
            CreateRocks();
            CreateWaterfall();
            CreateAtmosphere();
        }

        [ContextMenu("Clear Generated Environment")]
        public void ClearGeneratedEnvironment()
        {
            Transform existing = transform.Find("__ProceduralEnvironment");
            if (existing == null) return;
#if UNITY_EDITOR
            if (Application.isPlaying) Destroy(existing.gameObject); else DestroyImmediate(existing.gameObject);
#else
            Destroy(existing.gameObject);
#endif
            generatedRoot = null;
        }

        private void CreateMountains()
        {
            Transform parent = CreateGroup("Mountains");
            for (int i = 0; i < mountainCount; i++)
            {
                float angle = i / (float)Mathf.Max(1, mountainCount) * Mathf.PI * 2f;
                float distance = Mathf.Min(worldSize.x, worldSize.y) * 0.44f;
                Vector3 center = new Vector3(Mathf.Cos(angle) * distance + Rand(-9f, 9f), 0f, Mathf.Sin(angle) * distance + Rand(-9f, 9f));
                float height = Rand(mountainHeight.x, mountainHeight.y);
                float radius = Rand(mountainRadius.x, mountainRadius.y);

                GameObject baseMass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseMass.name = "Mountain_Base";
                baseMass.transform.SetParent(parent, false);
                baseMass.transform.position = center + Vector3.up * height * 0.28f;
                baseMass.transform.localScale = new Vector3(radius, height * 0.56f, radius);
                ApplyMaterial(baseMass, rockMaterial);

                GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                peak.name = "Mountain_Peak";
                peak.transform.SetParent(parent, false);
                peak.transform.position = center + Vector3.up * height * 0.68f;
                peak.transform.localScale = new Vector3(radius * 0.5f, height * 0.3f, radius * 0.5f);
                ApplyMaterial(peak, rockMaterial);
            }
        }

        private void CreateRiver()
        {
            if (!createRiver) return;
            Transform parent = CreateGroup("River");
            Vector3 previous = new Vector3(-worldSize.x * 0.52f, -riverDepth, Rand(-8f, 8f));
            for (int i = 0; i < riverSegments; i++)
            {
                float t = i / (float)Mathf.Max(1, riverSegments - 1);
                float z = Mathf.Lerp(-worldSize.y * 0.38f, worldSize.y * 0.38f, t) + Mathf.Sin(t * Mathf.PI * 2.4f) * riverWander;
                Vector3 next = new Vector3(Mathf.Lerp(-worldSize.x * 0.52f, worldSize.x * 0.52f, t), -riverDepth, z);
                Vector3 midpoint = (previous + next) * 0.5f;
                Vector3 direction = next - previous;
                GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "RiverSegment";
                water.transform.SetParent(parent, false);
                water.transform.position = midpoint;
                water.transform.localScale = new Vector3(riverWidth * Rand(0.9f, 1.15f), 0.16f, direction.magnitude + 0.25f);
                water.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                ApplyMaterial(water, waterMaterial);
                previous = next;
            }

            for (int i = 0; i < 20; i++)
            {
                float z = Rand(-worldSize.y * 0.35f, worldSize.y * 0.35f);
                float x = Mathf.Lerp(-worldSize.x * 0.5f, worldSize.x * 0.5f, Mathf.InverseLerp(-worldSize.y * 0.38f, worldSize.y * 0.38f, z));
                GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "RiverStone";
                stone.transform.SetParent(parent, false);
                stone.transform.position = new Vector3(x + Rand(-riverWidth, riverWidth), 0.25f, z);
                stone.transform.localScale = new Vector3(Rand(0.5f, 1.4f), Rand(0.3f, 0.8f), Rand(0.5f, 1.2f));
                ApplyMaterial(stone, rockMaterial);
            }
        }

        private void CreateTrees()
        {
            Transform parent = CreateGroup("Trees");
            for (int i = 0; i < treeCount; i++) CreateTree(parent, SampleGroundPoint(7f));
        }

        private void CreateTree(Transform parent, Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0f, Rand(0f, 360f), 0f);
            tree.transform.localScale = Vector3.one * Rand(0.8f, 1.35f);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = Vector3.up * 1.8f;
            trunk.transform.localScale = new Vector3(0.38f, 1.8f, 0.38f);
            ApplyMaterial(trunk, rockMaterial);

            for (int j = 0; j < 3; j++)
            {
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = "LeafCluster";
                crown.transform.SetParent(tree.transform, false);
                crown.transform.localPosition = new Vector3(Rand(-0.45f, 0.45f), 3.5f + j * 0.85f, Rand(-0.45f, 0.45f));
                float s = Rand(1.5f, 2.25f);
                crown.transform.localScale = new Vector3(s, s * 0.9f, s);
                ApplyMaterial(crown, foliageMaterial);
            }
        }

        private void CreateBushes()
        {
            Transform parent = CreateGroup("Bushes");
            for (int i = 0; i < bushCount; i++)
            {
                GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "Bush";
                bush.transform.SetParent(parent, false);
                bush.transform.position = SampleGroundPoint(3f) + Vector3.up * Rand(0.4f, 0.75f);
                float s = Rand(0.65f, 1.5f);
                bush.transform.localScale = new Vector3(s * 1.3f, s * 0.8f, s);
                ApplyMaterial(bush, foliageMaterial);
            }
        }

        private void CreateRocks()
        {
            Transform parent = CreateGroup("Rocks");
            for (int i = 0; i < rockCount; i++)
            {
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock";
                rock.transform.SetParent(parent, false);
                rock.transform.position = SampleGroundPoint(1.5f) + Vector3.up * Rand(0.25f, 0.7f);
                rock.transform.localScale = new Vector3(Rand(0.5f, 2f), Rand(0.35f, 1.2f), Rand(0.5f, 1.7f));
                rock.transform.rotation = UnityEngine.Random.rotation;
                ApplyMaterial(rock, rockMaterial);
            }
        }

        private void CreateWaterfall()
        {
            if (!createWaterfall) return;
            Transform parent = CreateGroup("Waterfall");
            Vector3 position = new Vector3(worldSize.x * 0.2f, waterfallHeight * 0.5f - riverDepth, 0f);

            GameObject cliff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cliff.name = "WaterfallCliff";
            cliff.transform.SetParent(parent, false);
            cliff.transform.position = position + Vector3.back * 0.9f;
            cliff.transform.localScale = new Vector3(waterfallWidth * 1.5f, waterfallHeight * 1.05f, 2.2f);
            ApplyMaterial(cliff, rockMaterial);

            GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curtain.name = "WaterfallCurtain";
            curtain.transform.SetParent(parent, false);
            curtain.transform.position = position + Vector3.forward * 1.2f;
            curtain.transform.localScale = new Vector3(waterfallWidth, waterfallHeight, 0.28f);
            ApplyMaterial(curtain, waterMaterial);

            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pool.name = "PlungePool";
            pool.transform.SetParent(parent, false);
            pool.transform.position = position + Vector3.down * (waterfallHeight * 0.5f) + Vector3.forward * 2f;
            pool.transform.localScale = new Vector3(waterfallWidth * 1.35f, 0.12f, waterfallWidth * 1.35f);
            ApplyMaterial(pool, waterMaterial);
        }

        private void CreateAtmosphere()
        {
            if (createFireflies)
            {
                Transform parent = CreateGroup("Fireflies");
                for (int i = 0; i < fireflyCount; i++)
                {
                    GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    glow.name = "Firefly";
                    glow.transform.SetParent(parent, false);
                    glow.transform.position = new Vector3(Rand(-worldSize.x * 0.4f, worldSize.x * 0.4f), Rand(1f, 5f), Rand(-worldSize.y * 0.4f, worldSize.y * 0.4f));
                    glow.transform.localScale = Vector3.one * 0.08f;
                    if (glow.TryGetComponent<Collider>(out Collider c)) DestroyImmediateSafe(c);
                }
            }

            if (createMist)
            {
                Transform parent = CreateGroup("Mist");
                for (int i = 0; i < mistCount; i++)
                {
                    GameObject mist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    mist.name = "MistCloud";
                    mist.transform.SetParent(parent, false);
                    mist.transform.position = new Vector3(Rand(-worldSize.x * 0.35f, worldSize.x * 0.35f), Rand(0.5f, 2.5f), Rand(-worldSize.y * 0.35f, worldSize.y * 0.35f));
                    float s = Rand(4f, 9f);
                    mist.transform.localScale = new Vector3(s, s * 0.3f, s * 0.65f);
                    ApplyMaterial(mist, mistMaterial);
                }
            }
        }

        private Vector3 SampleGroundPoint(float riverAvoid)
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                float x = Rand(-worldSize.x * 0.48f, worldSize.x * 0.48f);
                float z = Rand(-worldSize.y * 0.48f, worldSize.y * 0.48f);
                float riverCenter = Mathf.Sin((x / Mathf.Max(1f, worldSize.x)) * Mathf.PI * 2.4f) * riverWander;
                if (Mathf.Abs(z - riverCenter) > riverAvoid) return new Vector3(x, 0f, z);
            }
            return Vector3.zero;
        }

        private Transform CreateGroup(string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(generatedRoot, false);
            return group.transform;
        }

        private void ApplyMaterial(GameObject target, Material material)
        {
            if (material != null && target.TryGetComponent<Renderer>(out Renderer renderer))
                renderer.sharedMaterial = material;
        }

        private float Rand(float min, float max) => (float)(rng.NextDouble() * (max - min) + min);

        private void DestroyImmediateSafe(Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(obj); else Destroy(obj);
#else
            Destroy(obj);
#endif
        }
    }
}
