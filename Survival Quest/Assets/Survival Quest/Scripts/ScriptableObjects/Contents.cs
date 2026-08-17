using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Contents", menuName = "CustomObjects/Contents", order = 1)]
    public class Contents : ScriptableObject
    {
        public Equipment[] m_Equipment;
        public GameObject[] m_LevelPrefabs;
        
    }
}
