using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Equipment", menuName = "CustomObjects/Equipment", order = 2)]
    public class Equipment : ScriptableObject
    {
        public string m_EquipmentName;
        public float m_Damage;
        public int m_Durability;
        public int[] m_RequiredResources;
        public GameObject m_Prefab;

    }
}
