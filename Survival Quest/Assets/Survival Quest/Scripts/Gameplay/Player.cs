using SurvivalGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalGame
{
    public class Player : MonoBehaviour
    {
        public CharacterController m_PlayerController;

        public Camera m_MainCamera;

        public float m_MovementSpeed = 5f;

        [HideInInspector]
        public float m_Health = 100f;

        [HideInInspector]
        public int m_WeaponInHand = 0;

        [HideInInspector]
        public int m_SwordHits = 0;
        [HideInInspector]
        public int m_HammerHits = 0;

        public GameObject[] m_WeaponModels;

        [HideInInspector]
        public bool m_CanHit = true;
        [HideInInspector]
        public bool isNearWorkbench = false;
        [HideInInspector]
        public bool m_CanMove = true;

        public Transform m_HitPoint;

        public static Player m_Current;

        public Animator m_Animator;
        [HideInInspector]
        public float m_AnimatorMoveSpeed = 0f;

        public GameObject m_HitParticlePrefab;

        private void Awake()
        {
            m_Current = this;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (m_PlayerController == null)
            {
                m_PlayerController = GetComponent<CharacterController>();
            }

            if (m_Animator == null)
            {
                m_Animator = GetComponentInChildren<Animator>();
            }

            if (m_HitPoint == null)
            {
                Transform foundHp = transform.Find("HitPoint");
                if (foundHp != null)
                {
                    m_HitPoint = foundHp;
                }
                else
                {
                    GameObject hpGo = new GameObject("HitPoint");
                    hpGo.transform.SetParent(transform, false);
                    hpGo.transform.localPosition = new Vector3(0f, 0.872f, 1f);
                    m_HitPoint = hpGo.transform;
                }
            }

            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
            }

            // Auto-discover weapon models if array is empty or contains nulls
            FindOrFixWeaponModels();
        }

        private void FindOrFixWeaponModels()
        {
            if (m_WeaponModels != null && m_WeaponModels.Length >= 5 &&
                m_WeaponModels[0] != null && m_WeaponModels[1] != null &&
                m_WeaponModels[2] != null && m_WeaponModels[3] != null &&
                m_WeaponModels[4] != null)
            {
                return;
            }

            // Attempt to locate weapon GameObjects in children
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            GameObject axe = null;
            GameObject sword = null;
            GameObject hammer = null;
            GameObject trap = null;
            GameObject bomb = null;

            foreach (Transform t in allChildren)
            {
                string lower = t.name.ToLower();
                if (lower.Contains("axe") && axe == null) axe = t.gameObject;
                else if (lower.Contains("sword") && sword == null) sword = t.gameObject;
                else if (lower.Contains("hammer") && hammer == null) hammer = t.gameObject;
                else if (lower.Contains("trap") && trap == null) trap = t.gameObject;
                else if (lower.Contains("bomb") && bomb == null) bomb = t.gameObject;
            }

            List<GameObject> list = new List<GameObject>();
            if (axe != null) list.Add(axe);
            if (sword != null) list.Add(sword);
            if (hammer != null) list.Add(hammer);
            if (trap != null) list.Add(trap);
            if (bomb != null) list.Add(bomb);

            if (list.Count > 0)
            {
                m_WeaponModels = list.ToArray();
            }
        }

        void Start()
        {
            isNearWorkbench = false;
            m_CanMove = true;
            m_CanHit = true;
            m_WeaponInHand = 0;
            m_Health = 100f;

            if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                GameControl.m_Current.m_Data.m_Weapons != null && GameControl.m_Current.m_Data.m_Weapons.Length > 0)
            {
                GameControl.m_Current.m_Data.m_Weapons[0] = 1;
            }

            SelectWeapon(0);
        }

        void Update()
        {
            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            // Use Joystick / InputControl if active and providing input
            if (InputControl.m_Main != null && Joystick.m_Main != null)
            {
                Vector3 joyMove = InputControl.m_Main.m_Movement;
                if (joyMove.sqrMagnitude > 0.001f)
                {
                    horizontal = joyMove.x;
                    vertical = joyMove.z;
                }
            }

            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
            }

            Vector3 camRight = m_MainCamera != null ? m_MainCamera.transform.right : Vector3.right;
            Vector3 camForward = m_MainCamera != null ? m_MainCamera.transform.forward : Vector3.forward;

            Vector3 movementDirection = horizontal * camRight + vertical * camForward;
            Vector3 rotation = movementDirection;

            movementDirection.y = 0f;
            if (movementDirection.sqrMagnitude > 0.0001f)
            {
                movementDirection.Normalize();
            }
            else
            {
                movementDirection = Vector3.zero;
            }

            rotation.y = 0f;
            if (rotation.sqrMagnitude > 0.0001f)
            {
                rotation.Normalize();
            }
            else
            {
                rotation = Vector3.zero;
            }

            float targetSpeed = movementDirection.magnitude;
            m_AnimatorMoveSpeed = Mathf.Lerp(m_AnimatorMoveSpeed, targetSpeed, 10f * Time.deltaTime);

            if (m_Animator != null)
            {
                m_Animator.SetFloat("move-blend", m_AnimatorMoveSpeed);
            }

            Vector3 totalMovement = new Vector3(0f, -10f, 0f);
            if (movementDirection.magnitude > 0.1f && m_CanMove)
            {
                totalMovement += movementDirection * m_MovementSpeed;
            }

            if (m_PlayerController != null && m_PlayerController.enabled)
            {
                m_PlayerController.Move(totalMovement * Time.deltaTime);
            }

            if (movementDirection != Vector3.zero && rotation != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(rotation, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 10f * Time.deltaTime);
            }

            // Workbench interaction
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isNearWorkbench && InGameUI.Current != null && InGameUI.Current.Panel_Crafting != null)
                {
                    bool willOpen = !InGameUI.Current.Panel_Crafting.activeSelf;
                    InGameUI.Current.Panel_Crafting.SetActive(willOpen);
                    m_CanMove = !willOpen;
                }
            }

            // Attack input (Space key or Joystick Fire button)
            bool firePressed = Input.GetKeyDown(KeyCode.Space) ||
                               (InputControl.m_Main != null && InputControl.m_Main.m_Fire);

            if (firePressed)
            {
                StartCoroutine(Co_Attack());
            }

            // Weapon switching
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToDeafultWeapon();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TrySelectWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TrySelectWeapon(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TrySelectWeapon(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TrySelectWeapon(4);

            if (m_WeaponInHand > 0 && GameControl.m_Current != null && GameControl.m_Current.m_Data != null)
            {
                if (GameControl.m_Current.m_Data.m_Weapons[m_WeaponInHand] <= 0)
                {
                    SwitchToDeafultWeapon();
                }
            }

            // Health items
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                    GameControl.m_Current.m_Data.m_Resources[2] > 0)
                {
                    GameControl.m_Current.m_Data.m_Resources[2] -= 1;
                    m_Health = Mathf.Min(m_Health + 40f, 100f);
                }
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                    GameControl.m_Current.m_Data.m_Resources[3] > 0)
                {
                    GameControl.m_Current.m_Data.m_Resources[3] -= 1;
                    m_Health = Mathf.Min(m_Health + 10f, 100f);
                }
            }

            HandleHealth();
        }

        private void TrySelectWeapon(int index)
        {
            if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                GameControl.m_Current.m_Data.m_Weapons.Length > index &&
                GameControl.m_Current.m_Data.m_Weapons[index] > 0)
            {
                SelectWeapon(index);
                m_WeaponInHand = index;
                if (InGameUI.Current != null)
                {
                    InGameUI.Current.Btn_SelectItem(index);
                }
            }
        }

        IEnumerator Co_Attack()
        {
            if (!m_CanHit) yield break;

            m_CanHit = false;

            if (m_Animator != null)
            {
                m_Animator.Play("hit-1", 0, 0f);
            }

            switch (m_WeaponInHand)
            {
                case 0: // Axe
                    yield return new WaitForSeconds(0.3f);
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Contents != null)
                    {
                        CheckHit(GameControl.m_Current.m_Contents.m_Equipment[0].m_Damage);
                    }
                    else
                    {
                        CheckHit(10f);
                    }
                    break;

                case 1: // Sword
                    yield return new WaitForSeconds(0.3f);
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Contents != null)
                    {
                        CheckHit(GameControl.m_Current.m_Contents.m_Equipment[1].m_Damage);
                        m_SwordHits++;
                        if (m_SwordHits >= GameControl.m_Current.m_Contents.m_Equipment[1].m_Durability)
                        {
                            Invoke("SwitchToDeafultWeapon", 0.5f);
                            GameControl.m_Current.m_Data.m_Weapons[1]--;
                            m_SwordHits = 0;
                        }
                    }
                    break;

                case 2: // Hammer
                    yield return new WaitForSeconds(0.3f);
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Contents != null)
                    {
                        CheckHit(GameControl.m_Current.m_Contents.m_Equipment[2].m_Damage);
                        m_HammerHits++;
                        if (m_HammerHits >= GameControl.m_Current.m_Contents.m_Equipment[2].m_Durability)
                        {
                            Invoke("SwitchToDeafultWeapon", 0.5f);
                            GameControl.m_Current.m_Data.m_Weapons[2]--;
                            m_HammerHits = 0;
                        }
                    }
                    break;

                case 3: // Trap
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Contents != null)
                    {
                        GameObject obj = Instantiate(GameControl.m_Current.m_Contents.m_Equipment[3].m_Prefab);
                        obj.transform.position = transform.position;
                        GameControl.m_Current.m_Data.m_Weapons[3]--;
                    }
                    break;

                case 4: // Bomb
                    yield return new WaitForSeconds(0.2f);
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Contents != null)
                    {
                        Vector3 spawnPos = m_HitPoint != null ? m_HitPoint.position : transform.position + transform.forward + Vector3.up;
                        GameObject obj1 = Instantiate(GameControl.m_Current.m_Contents.m_Equipment[4].m_Prefab);
                        obj1.transform.position = spawnPos;
                        Rigidbody body = obj1.GetComponent<Rigidbody>();
                        if (body != null)
                        {
                            body.AddForce(4f * transform.forward + new Vector3(0f, 6f, 0f), ForceMode.VelocityChange);
                            body.angularVelocity = obj1.transform.rotation * new Vector3(20f, 0f, 0f);
                        }
                        GameControl.m_Current.m_Data.m_Weapons[4]--;
                    }
                    break;
            }

            yield return new WaitForSeconds(0.2f);
            m_CanHit = true;
        }

        public void SelectWeapon(int num)
        {
            if (m_WeaponModels == null || m_WeaponModels.Length == 0)
            {
                FindOrFixWeaponModels();
            }

            if (m_WeaponModels != null)
            {
                for (int i = 0; i < m_WeaponModels.Length; i++)
                {
                    if (m_WeaponModels[i] != null)
                    {
                        m_WeaponModels[i].SetActive(i == num);
                    }
                }
            }

            m_WeaponInHand = num;
        }

        public void CheckHit(float damage)
        {
            Vector3 hitCenter = m_HitPoint != null ? m_HitPoint.position : transform.position + transform.forward * 1.2f + Vector3.up * 0.8f;
            Collider[] hits = Physics.OverlapSphere(hitCenter, 1.2f);
            foreach (Collider c in hits)
            {
                if (c == null || c.gameObject == gameObject) continue;

                if (c.CompareTag("Resource"))
                {
                    Resource res = c.GetComponent<Resource>();
                    if (res != null)
                    {
                        res.m_Health -= damage;
                        res.HandleHit();
                        CreateHitParticle();
                    }
                }
                else if (c.CompareTag("NormalEnemy"))
                {
                    NormalEnemy enemy = c.GetComponent<NormalEnemy>();
                    if (enemy != null)
                    {
                        enemy.m_Health -= damage;
                        CreateHitParticle();
                    }
                }
            }
        }

        public void HandleHealth()
        {
            if (m_Health <= 0f)
            {
                UISystem.ShowUI("lose-ui");
                gameObject.SetActive(false);
            }
        }

        public void CreateHitParticle()
        {
            if (m_HitParticlePrefab == null) return;
            Vector3 hitPos = m_HitPoint != null ? m_HitPoint.position : transform.position + transform.forward + Vector3.up;
            GameObject obj = Instantiate(m_HitParticlePrefab);
            obj.transform.position = hitPos;
            Destroy(obj, 3f);
        }

        public void EnableCanHit()
        {
            m_CanHit = true;
        }

        public void SwitchToDeafultWeapon()
        {
            SelectWeapon(0);
            m_WeaponInHand = 0;
            if (InGameUI.Current != null)
            {
                InGameUI.Current.Btn_SelectItem(0);
            }
        }

        public void HitImpulse(Vector3 dir)
        {
            if (m_PlayerController != null && m_PlayerController.enabled)
            {
                m_PlayerController.Move(dir);
            }
        }
    }
}