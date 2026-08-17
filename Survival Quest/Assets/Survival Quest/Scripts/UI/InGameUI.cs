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
            Panel_Crafting.SetActive(false);
            Btn_SelectItem(0);
        }

        void Update()
        {
            for (int i = 0; i < Text_ResourcesCount.Length; i++)
            {
                Text_ResourcesCount[i].text = m_DataStorage.m_Resources[i].ToString();
            }

            for (int i = 0;i<5;i++)
            {
                if (m_DataStorage.m_Weapons[i]>0)
                {
                    m_WeaponImages[i].color = Color.white;
                }
                else
                {
                    m_WeaponImages[i].color = new Color(.6f, .6f, .6f, .5f);
                }
            }
            

            //Text_TargetCounter.text = GameControl.Current.m_TargetDestroyedCount + " / " + GameControl.Current.m_MaxTargetCount;

            Text_LifeCounter.text = ((int)Player.m_Current.m_Health).ToString();


            if(Player.m_Current.isNearWorkbench)
            {
                Button_CraftingMenu.gameObject.SetActive(true);
            }
            else
            {
                Button_CraftingMenu.gameObject.SetActive(false);
            }

            if(Panel_Crafting.gameObject.activeSelf)
            {
                Button_CraftingMenu.gameObject.SetActive(false);
            }

            for (int i = 0; i < 2; i++)
            {
                Text_SwordResources[i].text = m_Contents.m_Equipment[1].m_RequiredResources[i].ToString();
                Text_HammerResources[i].text = m_Contents.m_Equipment[2].m_RequiredResources[i].ToString();
                Text_TrapResources[i].text = m_Contents.m_Equipment[3].m_RequiredResources[i].ToString();
                Text_BombResources[i].text = m_Contents.m_Equipment[4].m_RequiredResources[i].ToString();
            }
            
        }

      

        public void Btn_SelectItem(int num)
        {
            
            //ThrowControl.m_Main.ObjectNum = num;
            
            
            switch(num)
            {
                case 0:
                    for (int i = 0; i < m_SelectionFrames.Length; i++)
                    {
                        m_SelectionFrames[i].gameObject.SetActive(false);
                    }
                    Player.m_Current.SelectWeapon(num);
                    Player.m_Current.m_WeaponInHand = 0;
                    m_SelectionFrames[num].gameObject.SetActive(true);
                    break;
                case 1:
                    
                    if (GameControl.m_Current.m_Data.m_Weapons[0] > 0)
                    {
                        for (int i = 0; i < m_SelectionFrames.Length; i++)
                        {
                            m_SelectionFrames[i].gameObject.SetActive(false);
                        }
                        Player.m_Current.SelectWeapon(num);
                        Player.m_Current.m_WeaponInHand = 1;
                        m_SelectionFrames[num].gameObject.SetActive(true);
                    }
                    break;
                case 2:
                    if (GameControl.m_Current.m_Data.m_Weapons[1] > 0)
                    {
                        for (int i = 0; i < m_SelectionFrames.Length; i++)
                        {
                            m_SelectionFrames[i].gameObject.SetActive(false);
                        }
                        Player.m_Current.SelectWeapon(num);
                        Player.m_Current.m_WeaponInHand = 2;
                        m_SelectionFrames[num].gameObject.SetActive(true);
                    }
                    
                    break;
                case 3:
                    if (GameControl.m_Current.m_Data.m_Weapons[2] > 0)
                    {
                        for (int i = 0; i < m_SelectionFrames.Length; i++)
                        {
                            m_SelectionFrames[i].gameObject.SetActive(false);
                        }
                        Player.m_Current.SelectWeapon(num);
                        Player.m_Current.m_WeaponInHand = 3;
                        m_SelectionFrames[num].gameObject.SetActive(true);
                    }
                    
                    break;
                case 4:
                    if (GameControl.m_Current.m_Data.m_Weapons[3] > 0)
                    {
                        for (int i = 0; i < m_SelectionFrames.Length; i++)
                        {
                            m_SelectionFrames[i].gameObject.SetActive(false);
                        }
                        Player.m_Current.SelectWeapon(num);
                        Player.m_Current.m_WeaponInHand = 4;
                        m_SelectionFrames[num].gameObject.SetActive(true);
                    }
                    
                    break;

            }
            
        }

        public void Btn_CraftWeapon(int num)
        {
            if (m_DataStorage.m_Resources[0] >= m_Contents.m_Equipment[num].m_RequiredResources[0] && m_DataStorage.m_Resources[1] >= m_Contents.m_Equipment[num].m_RequiredResources[1])
            {
                GameControl.m_Current.m_Data.m_Weapons[num]++;
                m_DataStorage.m_Resources[0] -= m_Contents.m_Equipment[num].m_RequiredResources[0];
                m_DataStorage.m_Resources[1] -= m_Contents.m_Equipment[num].m_RequiredResources[1];
            }
            
        }

      
        public void Btn_OpenCraftMenu()
        {
            Panel_Crafting.SetActive(true);
            Player.m_Current.m_CanMove = false;
        }

        public void Btn_CloseCraftMenu()
        {
            Panel_Crafting.SetActive(false);
            Player.m_Current.m_CanMove = true;
        }


        public void BtnPause()
        {
            //m_GameplayData.m_PowerIngameUIButton = false;
            //m_GameplayData.m_GameMode = 0;
            //GameControl.Current.PauseGame();
        }
       

   
  
    }
}
