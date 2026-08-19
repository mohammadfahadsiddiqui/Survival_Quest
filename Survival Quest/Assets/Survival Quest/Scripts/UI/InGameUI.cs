using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SurvivalGame.ScriptableObjects;

namespace SurvivalGame.UI
{
    public class InGameUI : MonoBehaviour
    {
        // [SerializeField, Space]
        // private Player m_Player;
        [SerializeField, Space]
        private GameplayData m_GameplayData;
        [SerializeField, Space]
        private Contents m_Contents;
        [SerializeField, Space]
        private DataStorage m_DataStorage;

        [SerializeField]
        private Text Text_TargetCounter;
        [SerializeField]
        private Text Text_LifeCounter;

        [SerializeField]
        private Text[] Text_ResourcesCount;

        [SerializeField]
        private Text[] Text_SwordResources;
        [SerializeField]
        private Text[] Text_HammerResources;
        [SerializeField]
        private Text[] Text_TrapResources;
        [SerializeField]
        private Text[] Text_BombResources;

        [SerializeField]
        private Text Text_LevelNum;

        public GameObject Panel_Crafting;
        public Button Button_CraftingMenu;

        public static InGameUI Current;

        public Image[] m_SelectionFrames;
        public Image[] m_WeaponImages;

        [HideInInspector]
        public int m_SelectedItem = -1;


        void Awake()
        {
            Current = this;
        }

        void Start()
        {
            if (Panel_Crafting != null)
            {
                Panel_Crafting.SetActive(false);
            }
            Btn_SelectItem(0);
        }

        void Update()
        {
            if (m_DataStorage != null && m_DataStorage.m_Resources != null && Text_ResourcesCount != null)
            {
                for (int i = 0; i < Text_ResourcesCount.Length && i < m_DataStorage.m_Resources.Length; i++)
                {
                    if (Text_ResourcesCount[i] != null)
                        Text_ResourcesCount[i].text = m_DataStorage.m_Resources[i].ToString();
                }
            }

            if (m_DataStorage != null && m_DataStorage.m_Weapons != null && m_WeaponImages != null)
            {
                for (int i = 0; i < 5 && i < m_WeaponImages.Length && i < m_DataStorage.m_Weapons.Length; i++)
                {
                    if (m_WeaponImages[i] != null)
                    {
                        if (m_DataStorage.m_Weapons[i] > 0)
                        {
                            m_WeaponImages[i].color = Color.white;
                        }
                        else
                        {
                            m_WeaponImages[i].color = new Color(.6f, .6f, .6f, .5f);
                        }
                    }
                }
            }

            if (Player.m_Current != null)
            {
                if (Text_LifeCounter != null)
                {
                    Text_LifeCounter.text = ((int)Player.m_Current.m_Health).ToString();
                }

                if (Button_CraftingMenu != null)
                {
                    bool showBtn = Player.m_Current.isNearWorkbench;
                    if (Panel_Crafting != null && Panel_Crafting.activeSelf)
                    {
                        showBtn = false;
                    }
                    Button_CraftingMenu.gameObject.SetActive(showBtn);
                }
            }

            if (m_Contents != null && m_Contents.m_Equipment != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Text_SwordResources != null && i < Text_SwordResources.Length && Text_SwordResources[i] != null &&
                        m_Contents.m_Equipment.Length > 1 && m_Contents.m_Equipment[1] != null &&
                        m_Contents.m_Equipment[1].m_RequiredResources != null &&
                        m_Contents.m_Equipment[1].m_RequiredResources.Length > i)
                    {
                        Text_SwordResources[i].text = m_Contents.m_Equipment[1].m_RequiredResources[i].ToString();
                    }

                    if (Text_HammerResources != null && i < Text_HammerResources.Length && Text_HammerResources[i] != null &&
                        m_Contents.m_Equipment.Length > 2 && m_Contents.m_Equipment[2] != null &&
                        m_Contents.m_Equipment[2].m_RequiredResources != null &&
                        m_Contents.m_Equipment[2].m_RequiredResources.Length > i)
                    {
                        Text_HammerResources[i].text = m_Contents.m_Equipment[2].m_RequiredResources[i].ToString();
                    }

                    if (Text_TrapResources != null && i < Text_TrapResources.Length && Text_TrapResources[i] != null &&
                        m_Contents.m_Equipment.Length > 3 && m_Contents.m_Equipment[3] != null &&
                        m_Contents.m_Equipment[3].m_RequiredResources != null &&
                        m_Contents.m_Equipment[3].m_RequiredResources.Length > i)
                    {
                        Text_TrapResources[i].text = m_Contents.m_Equipment[3].m_RequiredResources[i].ToString();
                    }

                    if (Text_BombResources != null && i < Text_BombResources.Length && Text_BombResources[i] != null &&
                        m_Contents.m_Equipment.Length > 4 && m_Contents.m_Equipment[4] != null &&
                        m_Contents.m_Equipment[4].m_RequiredResources != null &&
                        m_Contents.m_Equipment[4].m_RequiredResources.Length > i)
                    {
                        Text_BombResources[i].text = m_Contents.m_Equipment[4].m_RequiredResources[i].ToString();
                    }
                }
            }
        }

      

        public void Btn_SelectItem(int num)
        {
            if (num < 0 || num > 4) return;

            if (m_SelectionFrames != null)
            {
                for (int i = 0; i < m_SelectionFrames.Length; i++)
                {
                    if (m_SelectionFrames[i] != null)
                    {
                        m_SelectionFrames[i].gameObject.SetActive(false);
                    }
                }
            }

            bool canSelect = false;
            if (num == 0)
            {
                canSelect = true;
            }
            else
            {
                if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                    GameControl.m_Current.m_Data.m_Weapons != null && GameControl.m_Current.m_Data.m_Weapons.Length > num &&
                    GameControl.m_Current.m_Data.m_Weapons[num] > 0)
                {
                    canSelect = true;
                }
            }

            if (canSelect)
            {
                if (Player.m_Current != null)
                {
                    Player.m_Current.SelectWeapon(num);
                    Player.m_Current.m_WeaponInHand = num;
                }
                if (m_SelectionFrames != null && num < m_SelectionFrames.Length && m_SelectionFrames[num] != null)
                {
                    m_SelectionFrames[num].gameObject.SetActive(true);
                }
            }
        }

        public void Btn_CraftWeapon(int num)
        {
            if (m_DataStorage != null && m_DataStorage.m_Resources != null && m_DataStorage.m_Resources.Length >= 2 &&
                m_Contents != null && m_Contents.m_Equipment != null && m_Contents.m_Equipment.Length > num &&
                m_Contents.m_Equipment[num] != null && m_Contents.m_Equipment[num].m_RequiredResources != null &&
                m_Contents.m_Equipment[num].m_RequiredResources.Length >= 2)
            {
                if (m_DataStorage.m_Resources[0] >= m_Contents.m_Equipment[num].m_RequiredResources[0] &&
                    m_DataStorage.m_Resources[1] >= m_Contents.m_Equipment[num].m_RequiredResources[1])
                {
                    if (GameControl.m_Current != null && GameControl.m_Current.m_Data != null &&
                        GameControl.m_Current.m_Data.m_Weapons != null && GameControl.m_Current.m_Data.m_Weapons.Length > num)
                    {
                        GameControl.m_Current.m_Data.m_Weapons[num]++;
                    }
                    m_DataStorage.m_Resources[0] -= m_Contents.m_Equipment[num].m_RequiredResources[0];
                    m_DataStorage.m_Resources[1] -= m_Contents.m_Equipment[num].m_RequiredResources[1];
                }
            }
        }

        public void Btn_OpenCraftMenu()
        {
            if (Panel_Crafting != null)
            {
                Panel_Crafting.SetActive(true);
            }
            if (Player.m_Current != null)
            {
                Player.m_Current.m_CanMove = false;
            }
        }

        public void Btn_CloseCraftMenu()
        {
            if (Panel_Crafting != null)
            {
                Panel_Crafting.SetActive(false);
            }
            if (Player.m_Current != null)
            {
                Player.m_Current.m_CanMove = true;
            }
        }


        public void BtnPause()
        {
            //m_GameplayData.m_PowerIngameUIButton = false;
            //m_GameplayData.m_GameMode = 0;
            //GameControl.Current.PauseGame();
        }
       

   
  
    }
}
