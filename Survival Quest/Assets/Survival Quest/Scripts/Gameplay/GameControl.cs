using SurvivalGame.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
    public class GameControl : MonoBehaviour
    {
        public List<Resource> m_Resources = new List<Resource>();

        [HideInInspector]
        public Resource m_DestroyedResource;

        public DataStorage m_Data;
        public Contents m_Contents;

        public static GameControl m_Current;

        private void Awake()
        {
            m_Current = this;
            if (m_Resources == null)
            {
                m_Resources = new List<Resource>();
            }
            if (m_Data != null)
            {
                m_Data.LoadData();
            }
        }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
}