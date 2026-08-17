#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using SurvivalQuest.Gameplay;

[InitializeOnLoad]
public static class SurvivalQuestForestBuilder
{
    private const string NatureRoot = "Assets/Survival Quest/Prefabs/Nature/";
    private const string GameplayRoot = "Assets/Survival Quest/Prefabs/Gameplay/";
    private const string ForestObject = "SURVIVAL QUEST FOREST";
    private const string EnemyObject = "SURVIVAL QUEST ENEMIES";

    static SurvivalQuestForestBuilder()
    {
        EditorApplication.delayCall += AutoBuild;
    }

    private static void AutoBuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "SampleScene") return;

        CleanupMissingScripts(scene);

        if (GameObject.Find(ForestObject) == null || GameObject.Find(EnemyObject) == null)
            Build(false);
        else
            EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/Survival Quest/Build Forest & Enemies")]
    public static void BuildManually() => Build(true);

    [MenuItem("Tools/Survival Quest/Clean Missing Scripts")]
    public static void CleanMissingScriptsManually()
    {
        CleanupMissingScripts(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void CleanupMissingScripts(Scene scene)
    {
        if (!scene.IsValid()) return;
        int removed = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameObject[] objects = root.GetComponentsInChildren<Transform>(true).Length > 0
                ? System.Array.ConvertAll(root.GetComponentsInChildren<Transform>(true), t => t.gameObject)
                : new GameObject[0];

            foreach (GameObject go in objects)
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }

        if (removed > 0)
        {
            Debug.Log("[SurvivalQuest] Removed " + removed + " missing script component(s) from SampleScene.");
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    private static void Build(bool interactive)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;

        CleanupMissingScripts(scene);

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
            EditorUtility.DisplayDialog("Survival Quest", "Forest and enemies are now installed in SampleScene.", "OK");
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

        GameObject[] trees = FindPrefabs("tree");
        GameObject[] bushes = FindPrefabs("bush");
        GameObject[] rocks = FindPrefabs("rock");

        SetPrefabArray(so, "treePrefabs", trees);
        SetPrefabArray(so, "bushPrefabs", bushes);
        SetPrefabArray(so, "rockPrefabs", rocks);
        SetInt(so, "treeCount", 55);
        SetInt(so, "bushCount", 85);
        SetInt(so, "rockCount", 25);
        SetFloat(so, "minDistanceFromPlayer", 8f);
        SetFloat(so, "maxScaleVariation", 0.25f);
        so.ApplyModifiedPropertiesWithoutUndo();

        controller.ClearForest();
        controller.GenerateForest();
    }

    private static void ConfigureEnemies(Transform parent, Transform player)
    {
        EnemySpawner spawner = parent.GetComponent<EnemySpawner>();
        if (spawner == null) spawner = parent.gameObject.AddComponent<EnemySpawner>();

        GameObject normal = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "NormalEnemy.prefab");
        GameObject bruteSource = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayRoot + "Brute.fbx");
        GameObject brute = bruteSource != null ? CreateOrGetBrutePrefab(bruteSource) : null;

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
        else SetPrefabArray(so, "enemyPrefabs", new GameObject[0]);

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