#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SurvivalGame;

/// <summary>
/// Sets up the Brute character as the Player prefab.
/// Can be triggered manually via: Tools -> Setup Brute Player.
/// </summary>
public static class BrutePlayerSetup
{
    [MenuItem("Tools/Setup Brute Player")]
    public static void SetupBrutePlayer()
    {
        const string bruteFbxPath        = "Assets/Survival Quest/Prefabs/Gameplay/Brute.fbx";
        const string bruteControllerPath = "Assets/Survival Quest/Prefabs/Gameplay/BruteAnim.controller";
        const string playerPrefabPath    = "Assets/Survival Quest/Prefabs/Gameplay/Player.prefab";
        const string hitParticlePath     = "Assets/Survival Quest/Prefabs/Gameplay/Hit Particle.prefab";

        const string axePrefabPath    = "Assets/Survival Quest/Prefabs/Weapons/wpn-axe-1.prefab";
        const string swordPrefabPath  = "Assets/Survival Quest/Prefabs/Weapons/wpn-sword-1.prefab";
        const string hammerPrefabPath = "Assets/Survival Quest/Prefabs/Weapons/wpn-hammer-1.prefab";
        const string trapPrefabPath   = "Assets/Survival Quest/Prefabs/Weapons/Trap.prefab";
        const string bombPrefabPath   = "Assets/Survival Quest/Prefabs/Weapons/wpn-bomb-1.prefab";

        GameObject bruteFbx = AssetDatabase.LoadAssetAtPath<GameObject>(bruteFbxPath);
        RuntimeAnimatorController bruteController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(bruteControllerPath);
        GameObject playerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);

        if (bruteFbx == null || bruteController == null || playerPrefabAsset == null)
        {
            return;
        }

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
        {
            GameObject root = editScope.prefabContentsRoot;

            // 1. Remove old character-1 child if it exists and is not the Brute
            Transform oldChar = root.transform.Find("character-1");
            if (oldChar != null)
            {
                Object.DestroyImmediate(oldChar.gameObject);
            }

            // Remove any leftover old Armature or meshes directly under root
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = root.transform.GetChild(i);
                string lower = child.name.ToLower();
                if (lower == "armature" || lower == "character-1" || lower.StartsWith("cube") || lower.StartsWith("capsule") || lower.StartsWith("sphere"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // 2. Instantiate Brute.fbx as child
            GameObject bruteGO = (GameObject)PrefabUtility.InstantiatePrefab(bruteFbx, root.transform);
            bruteGO.name = "character-1";
            bruteGO.transform.localPosition = Vector3.zero;
            bruteGO.transform.localRotation = Quaternion.identity;
            bruteGO.transform.localScale    = Vector3.one;

            // 3. Setup Animator
            Animator anim = bruteGO.GetComponent<Animator>();
            if (anim == null) anim = bruteGO.GetComponentInChildren<Animator>(true);
            if (anim == null) anim = bruteGO.AddComponent<Animator>();

            anim.runtimeAnimatorController = bruteController;
            anim.applyRootMotion = false;

            Animator fbxAnim = bruteFbx.GetComponent<Animator>();
            if (fbxAnim != null && fbxAnim.avatar != null)
            {
                anim.avatar = fbxAnim.avatar;
            }

            // 4. Find right hand bone
            Transform rightHand = FindRightHand(bruteGO.transform);
            Transform weaponParent = rightHand != null ? rightHand : bruteGO.transform;

            // 5. Instantiate weapon models under weaponParent
            GameObject axe = LoadAndAttach(axePrefabPath, weaponParent, "wpn-axe-1", new Vector3(0.08f, 0.05f, 0.02f), Quaternion.Euler(0, 90, 90), Vector3.one * 0.8f);
            GameObject sword = LoadAndAttach(swordPrefabPath, weaponParent, "wpn-sword-1", new Vector3(0.08f, 0.05f, 0.02f), Quaternion.Euler(0, 90, 90), Vector3.one * 0.8f);
            GameObject hammer = LoadAndAttach(hammerPrefabPath, weaponParent, "wpn-hammer-1", new Vector3(0.08f, 0.05f, 0.02f), Quaternion.Euler(0, 90, 90), Vector3.one * 0.8f);
            GameObject trap = LoadAndAttach(trapPrefabPath, weaponParent, "trap", new Vector3(0.08f, 0.05f, 0.02f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.3f);
            GameObject bomb = LoadAndAttach(bombPrefabPath, weaponParent, "wpn-bomb-1", new Vector3(0.08f, 0.05f, 0.02f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.8f);

            // 6. Setup Player component
            Player player = root.GetComponent<Player>();
            if (player == null) player = root.AddComponent<Player>();

            player.m_Animator = anim;
            player.m_MovementSpeed = 5f;

            CharacterController cc = root.GetComponent<CharacterController>();
            if (cc == null) cc = root.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);
            player.m_PlayerController = cc;

            // HitPoint
            Transform existingHp = root.transform.Find("HitPoint");
            if (existingHp != null)
            {
                player.m_HitPoint = existingHp;
            }
            else
            {
                GameObject hpGo = new GameObject("HitPoint");
                hpGo.transform.SetParent(root.transform, false);
                hpGo.transform.localPosition = new Vector3(0f, 0.872f, 1f);
                player.m_HitPoint = hpGo.transform;
            }

            // Hit particle
            GameObject hpParticle = AssetDatabase.LoadAssetAtPath<GameObject>(hitParticlePath);
            if (hpParticle != null)
            {
                player.m_HitParticlePrefab = hpParticle;
            }

            // Weapon models array
            List<GameObject> weapons = new List<GameObject>();
            if (axe != null) weapons.Add(axe);
            if (sword != null) weapons.Add(sword);
            if (hammer != null) weapons.Add(hammer);
            if (trap != null) weapons.Add(trap);
            if (bomb != null) weapons.Add(bomb);
            player.m_WeaponModels = weapons.ToArray();

            // Set axe active by default, others inactive
            if (axe != null) axe.SetActive(true);
            if (sword != null) sword.SetActive(false);
            if (hammer != null) hammer.SetActive(false);
            if (trap != null) trap.SetActive(false);
            if (bomb != null) bomb.SetActive(false);

            Debug.Log("[BrutePlayerSetup] Brute character successfully configured as Player!");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static Transform FindRightHand(Transform root)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in all)
        {
            string n = t.name.ToLower();
            if (n.Contains("righthand") && !n.Contains("thumb") && !n.Contains("index") && !n.Contains("middle") && !n.Contains("ring") && !n.Contains("pinky"))
            {
                return t;
            }
        }
        return null;
    }

    private static GameObject LoadAndAttach(string path, Transform parent, string name, Vector3 localPos, Quaternion localRot, Vector3 localScale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return null;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.localPosition = localPos;
        instance.transform.localRotation = localRot;
        instance.transform.localScale    = localScale;
        return instance;
    }
}
#endif
