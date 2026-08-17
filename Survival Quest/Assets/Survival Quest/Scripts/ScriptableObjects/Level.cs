using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Level", menuName = "CustomObjects/Level", order = 1)]
    public class Level : ScriptableObject
    {
        [Space]
        [Header("Enmy Spawn Settings")]
        public GameObject[] m_EnemyTypes;
        public int[] m_WaveEnemies;
        public float m_SpawnDelay = 3;
        [Space]
        [Space]
        [Header("Data")]
        public int m_StarCount = 0;
        public int m_LevelCoinAmount = 0;

    }
}
