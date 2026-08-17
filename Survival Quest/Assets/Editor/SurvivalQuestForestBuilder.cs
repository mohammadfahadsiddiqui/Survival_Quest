#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using SurvivalQuest.Gameplay;

/// <summary>
/// Builds the forest and enemy setup directly into SampleScene.
/// The setup runs automatically once when SampleScene is opened/imported.
/// Manual menu: Tools -> Survival Quest -> Build Forest & Enemies
/// </summary>
[InitializeOnLoad]
public static class SurvivalQuestForestBuilder
{
    private const string NatureRoot = "Assets/Survival Quest/Prefabs/Nature/";
    private const string GameplayRoot = "Assets/Survival Quest/Prefabs/Gameplay/";
    private const string ForestObject = "SURVIVAL QUEST FOREST";
    private const string EnemyObject = "SURVIVAL QUEST ENEMIES";

    static SurvivalQuestForestBuilder()
    {
        EditorApplication.delayCall += AutoBuildIfNeeded;
    }

    private static void AutoBuildIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "SampleScene") return;
        if (GameObject.Find(ForestObject) != null || GameObject.Find(EnemyObject) != null) return;
        Build(false);
    }

    [MenuItem("Tools/Survival Quest/Build Forest & Enemies")]
    public static void BuildManually()
    {
        Build(true);
    }

    private static void Build(bool interactive)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        Transform forestParent = GetOrCreate(ForestObject).transform;
        Transform enemyParent = GetOrCreate(EnemyObject).transform;

        ConfigureForest(forestParent, player != null ? player.transform : null);
        ConfigureEnemies(enemyParent, player != null ? player.transform : null);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (interactive)
        {
            Selection.activeGameObject = forestParent.gameObject;
            EditorUtility.DisplayDialog("Survival Quest", "Forest and enemies have been added to SampleScene. Press Play to test.", "OK");
        }
    }

    private static void ConfigureForest(Transform parent, Transform player)
    {
        ForestEnvironmentController controller = parent.GetComponent<ForestEnvironmentController>();
        if (controller == null) controller = parent.gameObject.AddComponent<ForestEnvironmentController>();

        SerializedObject so = new SerializedObject(controller);
        SetVector2(so, "areaSize", new Vector2(80f, 80f));
        SetInt(so, "seed", 240817);
        SetObject(so, "player", player);
        SetObject(so, "forestParent", parent);

        // This project currently contains bush prefabs rather than named tree/rock prefabs.
        // Use bushes as the dense forest dressing and the hill/cliff assets as terrain landmarks.
        GameObject[] bushes = FindPrefabs("bush");
        GameObject[] hills = FindPrefabs("hill");
        GameObject[] cliffs = FindPrefabs("cliff");
        GameObject[] ground = FindPrefabs("ground");

        SetPrefabArray(so, "treePrefabs", bushes);
        SetPrefabArray(so, "bushPrefabs", bushes);
        SetPrefabArray(so, "rockPrefabs", hills.Length > 0 ? hills : cliffs);
        SetInt(so, "treeCount", 45);
        SetInt(so, "bushCount", 85);
        SetInt(so, "rockCount", 12);
        SetFloat(so, "minDistanceFromPlayer", 8f);
        SetFloat(so, "maxScaleVariation", 0.30f);
        so.ApplyModifiedPropertiesWithoutUndo();

        ClearChildren(parent);
        controller.GenerateForest();

        // Add a few large terrain pieces around the outside, when available.
        GameObject terrainPrefab = hills.Length > 0 ? hills[0] : (cliffs.Length > 0 ? cliffs[0] : (ground.Length > 0 ? ground[0] : null));
        if (terrainPrefab != null)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 32f, 0f, Mathf.Sin(angle) * 32f);
                GameObject piece = (GameObject)PrefabUtility.InstantiatePrefab(terrainPrefab, parent);
                piece.transform.position = pos;
                piece.transform.rotation = Quaternion.Euler(0f, i * 60f, 0f);
                piece.transform.localScale = Vector3.one * 1.5f;
            }
        }
    }

    private static void ConfigureEnemies(Transform parent, Transform player)
    {
        EnemySpawner spawner = parent.GetComponent<EnemySpawner>();
        if (spawner == null) spawner = parent.gameObject.AddComponent<EnemySpawner>();

        GameObject normal = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "NormalEnemy.prefab");
        GameObject bruteSource = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "Brute.fbx");
        GameObject brute = bruteSource != null ? CreateOrGetBrutePrefab(bruteSource) : null;

        if (brute != null) ConfigureEnemyPrefab(brute, 180f, 1.8f, 16f, 2f, 30f, 1.8f);

        SerializedObject so = new SerializedObject(spawner);
        SetObject(so, "player", player);
        SetInt(so, "initialEnemies", 6);
        SetFloat(so, "spawnRadius", 25f);
        SetFloat(so, "minDistanceFromPlayer", 10f);
        SetFloat(so, "respawnDelay", 8f);
        SetInt(so, "maxAliveEnemies", 12);

        if (normal != null && brute != null) SetPrefabArray(so, "enemyPrefabs", new[] { normal, brute });
        else if (normal != null) SetPrefabArray(so, "enemyPrefabs", new[] { normal });
        else if (brute != null) SetPrefabArray(so, "enemyPrefabs", new[] { brute });
        so.ApplyModifiedPropertiesWithoutUndo();

        ClearChildren(parent);
    }

    private static GameObject CreateOrGetBrutePrefab(GameObject source)
    {
        const string path = GameplayRoot + "SurvivalQuest_Brute.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = "SurvivalQuest_Brute";

        Animator animator = instance.GetComponent<Animator>() ?? instance.GetComponentInChildren<Animator>(true);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(GameplayRoot + "BruteAnim.controller");
        if (animator != null && controller != null)
        {
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }

        if (instance.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = instance.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 1f, 0f);
        }

        if (instance.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = instance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (instance.GetComponent<EnemyBase>() == null) instance.AddComponent<EnemyBase>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    private static void ConfigureEnemyPrefab(GameObject prefab, float health, float speed, float detection, float attackRange, float damage, float cooldown)
    {
        if (prefab == null) return;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        EnemyBase enemy = instance.GetComponent<EnemyBase>() ?? instance.AddComponent<EnemyBase>();
        SerializedObject so = new SerializedObject(enemy);
        SetFloat(so, "maxHealth", health);
        SetFloat(so, "moveSpeed", speed);
        SetFloat(so, "detectionRange", detection);
        SetFloat(so, "attackRange", attackRange);
        SetFloat(so, "attackDamage", damage);
        SetFloat(so, "attackCooldown", cooldown);
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
        Object.DestroyImmediate(instance);
    }

    private static GameObject[] FindPrefabs(string keyword)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { NatureRoot });
        var results = new System.Collections.Generic.List<GameObject>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!name.Contains(keyword)) continue;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) results.Add(prefab);
        }
        return results.ToArray();
    }

    private static GameObject GetOrCreate(string name)
    {
        GameObject existing = GameObject.Find(name);
        return existing != null ? existing : new GameObject(name);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--) Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static void SetObject(SerializedObject so, string property, Object value)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null) p.objectReferenceValue = value;
    }

    private static void SetPrefabArray(SerializedObject so, string property, GameObject[] values)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p == null) return;
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void SetInt(SerializedObject so, string property, int value)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null) p.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string property, float value)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null) p.floatValue = value;
    }

    private static void SetVector2(SerializedObject so, string property, Vector2 value)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null) p.vector2Value = value;
    }
}
#endif
