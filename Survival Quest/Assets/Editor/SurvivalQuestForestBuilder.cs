#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using SurvivalQuest.Gameplay;

/// <summary>
/// One-click editor setup for the Survival Quest forest prototype.
/// Menu: Tools -> Survival Quest -> Build Forest & Enemies
/// Uses assets already present in the project.
/// </summary>
public static class SurvivalQuestForestBuilder
{
    private const string NatureRoot = "Assets/Survival Quest/Prefabs/Nature/";
    private const string GameplayRoot = "Assets/Survival Quest/Prefabs/Gameplay/";

    [MenuItem("Tools/Survival Quest/Build Forest & Enemies")]
    public static void Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Survival Quest", "Open SampleScene before running the builder.", "OK");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        Transform forestParent = GetOrCreate("SURVIVAL QUEST FOREST").transform;
        Transform enemyParent = GetOrCreate("SURVIVAL QUEST ENEMIES").transform;

        ConfigureForest(forestParent, player != null ? player.transform : null);
        ConfigureEnemies(enemyParent, player != null ? player.transform : null);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = forestParent.gameObject;
        EditorUtility.DisplayDialog(
            "Survival Quest",
            "Forest and enemy systems have been configured.\n\nPress Play to test the scene.",
            "OK");
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

        SetPrefabArray(so, "treePrefabs", FindPrefabs("tree"));
        SetPrefabArray(so, "bushPrefabs", FindPrefabs("bush"));
        SetPrefabArray(so, "rockPrefabs", FindPrefabs("rock"));

        SetInt(so, "treeCount", 80);
        SetInt(so, "bushCount", 60);
        SetInt(so, "rockCount", 35);
        SetFloat(so, "minDistanceFromPlayer", 7f);
        SetFloat(so, "maxScaleVariation", 0.25f);
        so.ApplyModifiedPropertiesWithoutUndo();

        ClearChildren(parent);
        controller.GenerateForest();
    }

    private static void ConfigureEnemies(Transform parent, Transform player)
    {
        EnemySpawner spawner = parent.GetComponent<EnemySpawner>();
        if (spawner == null) spawner = parent.gameObject.AddComponent<EnemySpawner>();

        GameObject normal = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "NormalEnemy.prefab");
        GameObject brute = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "Brute.fbx");

        if (brute != null)
        {
            GameObject brutePrefab = CreateOrGetBrutePrefab(brute);
            ConfigureEnemyPrefab(brutePrefab, 180f, 1.8f, 16f, 2.0f, 30f, 1.8f);
            brute = brutePrefab;
        }

        SerializedObject so = new SerializedObject(spawner);
        SetObject(so, "player", player);
        SetInt(so, "initialEnemies", 6);
        SetFloat(so, "spawnRadius", 25f);
        SetFloat(so, "minDistanceFromPlayer", 10f);
        SetFloat(so, "respawnDelay", 8f);
        SetInt(so, "maxAliveEnemies", 12);

        if (normal != null && brute != null)
            SetPrefabArray(so, "enemyPrefabs", new[] { normal, brute });
        else if (normal != null)
            SetPrefabArray(so, "enemyPrefabs", new[] { normal });
        else if (brute != null)
            SetPrefabArray(so, "enemyPrefabs", new[] { brute });

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

        Animator animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.GetComponentInChildren<Animator>(true);

        string controllerPath = GameplayRoot + "BruteAnim.controller";
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (animator != null && controller != null)
        {
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }

        if (instance.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = instance.AddComponent<CapsuleCollider>();
            collider.height = 2.0f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 1f, 0f);
        }

        if (instance.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = instance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        EnemyBase enemy = instance.GetComponent<EnemyBase>();
        if (enemy == null) enemy = instance.AddComponent<EnemyBase>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return prefab;
    }

    private static void ConfigureEnemyPrefab(GameObject prefab, float health, float speed, float detection, float attackRange, float damage, float cooldown)
    {
        if (prefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        EnemyBase enemy = instance.GetComponent<EnemyBase>();
        if (enemy == null) enemy = instance.AddComponent<EnemyBase>();

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
        if (existing != null) return existing;
        return new GameObject(name);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
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
