using UnityEngine;

namespace SurvivalQuest.Environment
{
    /// <summary>
    /// Automatically creates a polished procedural environment the first time the scene runs.
    /// Add this component to any scene root object, or let the editor setup tool add it.
    /// </summary>
    public class EnvironmentAutoBootstrap : MonoBehaviour
    {
        [SerializeField] private bool generateOnAwake = true;
        [SerializeField] private bool generateOnStartIfMissing = true;

        private void Awake()
        {
            if (generateOnAwake)
                EnsureEnvironment();
        }

        private void Start()
        {
            if (generateOnStartIfMissing)
                EnsureEnvironment();
        }

        public void EnsureEnvironment()
        {
            if (transform.Find("__ProceduralEnvironment") != null)
                return;

            var generator = GetComponent<ProceduralEnvironmentGenerator>();
            if (generator == null)
                generator = gameObject.AddComponent<ProceduralEnvironmentGenerator>();

            generator.GenerateEnvironment();
        }
    }
}
