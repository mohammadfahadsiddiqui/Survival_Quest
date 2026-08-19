using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace SurvivalGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DataStorage", menuName = "CustomObjects/DataStorage", order = 1)]
    public class DataStorage : ScriptableObject
    {
        public int m_WinCount;

        [SerializeField, Space]
        private Contents m_Contents;
        public int Coin;
        public int Gem;
        public int m_PlayerXP;
        public int LevelUnlocked;
        public int[] m_Resources = new int[5];
        public int[] m_Weapons = new int[5];


        public bool MuteMusic;
        public bool FullVersion = false;
        public int[] m_PowerCount;


        public void SaveData()
        {
            PlayerPrefs.SetInt("m_WinCount", m_WinCount);

            int tempCoin = PlayerPrefs.GetInt("Coin", 0);

            if (Coin - tempCoin <= 40000)
            {
                PlayerPrefs.SetInt("Coin", Coin);
            }
            else
            {
                //cheating
                Debug.Log("CHEATING");
                Coin = tempCoin;
                PlayerPrefs.SetInt("Coin", tempCoin);
            }

            int tempGem = PlayerPrefs.GetInt("Gem", 0);

            if (Gem - tempGem <= 2000)
            {
                PlayerPrefs.SetInt("Gem", Gem);
            }
            else
            {
                //cheating
                Debug.Log("CHEATING");
                Gem = tempGem;
                PlayerPrefs.SetInt("Gem", tempGem);
            }



            PlayerPrefs.SetInt("m_PlayerXP", m_PlayerXP);
            PlayerPrefs.SetInt("LevelUnlocked", LevelUnlocked);


            if (MuteMusic)
                PlayerPrefs.SetInt("MuteMusic", 1);
            else
                PlayerPrefs.SetInt("MuteMusic", 0);
          

            if (FullVersion)
                PlayerPrefs.SetInt("FullVersion", 1);
            else
                PlayerPrefs.SetInt("FullVersion", 0);
           

            //for (int i = 0; i < 3; i++)
            //{
            //    PlayerPrefs.SetInt("m_PowerCount" + i.ToString(), m_PowerCount[i]);
            //}

            if (m_Resources != null)
            {
                for (int i = 0; i < m_Resources.Length; i++)
                {
                    PlayerPrefs.SetInt("m_Resources" + i.ToString(), m_Resources[i]);
                }
            }

            if (m_Weapons != null)
            {
                for (int i = 0; i < m_Weapons.Length; i++)
                {
                    PlayerPrefs.SetInt("m_Weapons" + i.ToString(), m_Weapons[i]);
                }
            }

            PlayerPrefs.Save();
        }

        public void LoadData()
        {
            m_WinCount = PlayerPrefs.GetInt("m_WinCount", 0);
          
            Coin = PlayerPrefs.GetInt("Coin", 0);
            Gem = PlayerPrefs.GetInt("Gem", 0);


            m_PlayerXP = PlayerPrefs.GetInt("m_PlayerXP", 0);
            LevelUnlocked = PlayerPrefs.GetInt("LevelUnlocked", 0);
            MuteMusic = (PlayerPrefs.GetInt("MuteMusic", 0) == 1);
           
            FullVersion = (PlayerPrefs.GetInt("FullVersion", 0) == 1);
          
            //m_PowerCount = new int[3];
            //for (int i = 0; i < 3; i++)
            //{
            //    m_PowerCount[i] = PlayerPrefs.GetInt("m_PowerCount" + i.ToString(), 0);
            //}

            if (m_Resources == null || m_Resources.Length < 5)
            {
                m_Resources = new int[5];
            }
            for (int i = 0; i < m_Resources.Length; i++)
            {
                m_Resources[i] = PlayerPrefs.GetInt("m_Resources" + i.ToString(), 0);
            }

            if (m_Weapons == null || m_Weapons.Length < 5)
            {
                m_Weapons = new int[5];
            }
            for (int i = 0; i < m_Weapons.Length; i++)
            {
                m_Weapons[i] = PlayerPrefs.GetInt("m_Weapons" + i.ToString(), 0);
            }
        }

        public void ResetSaveData()
        {
            SaveData();
        }

        public void EarnXP(int xpAmount)
        {
            int m_MaxXP = 20;
            m_PlayerXP = m_PlayerXP + xpAmount;
            if (m_PlayerXP >= m_MaxXP)
            {
                //m_PlayerData.m_PlayerLevel++;
                //m_MaxXP = +m_PlayerData.m_PlayerLevel;
            }
            SaveData();
        }

        public void EarnCoin(int coinAmount)
        {
            Coin = +coinAmount;
            SaveData();
        }

        public void SpendCoin(int coinAmount)
        {
            Coin = -coinAmount;
            Coin = Mathf.Max(Coin, 0);
            SaveData();
        }

        public void EarnGem(int gemAmount)
        {
            Gem = +gemAmount;
            SaveData();
        }

        public void SpendGem(int gemAmount)
        {
            Gem = -gemAmount;
            Gem = Mathf.Max(Gem, 0);
            SaveData();
        }

        public bool CheckInternet()
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                return true;



            return false;
        }

    }
}
