using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SurvivalGame
{
    /// <summary>
    /// Makes the left mouse button trigger the Player's existing attack system.
    /// The component is attached automatically at runtime, so no scene setup is required.
    /// </summary>
    public class MouseAttackInput : MonoBehaviour
    {
        private Player m_Player;
        private MethodInfo m_AttackMethod;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Player player = Player.m_Current;
            if (player == null)
                player = Object.FindFirstObjectByType<Player>();

            if (player != null && player.GetComponent<MouseAttackInput>() == null)
                player.gameObject.AddComponent<MouseAttackInput>();
        }

        private void Awake()
        {
            m_Player = GetComponent<Player>();
            m_AttackMethod = typeof(Player).GetMethod(
                "Co_Attack",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (m_Player == null || m_AttackMethod == null)
                return;

            IEnumerator attackRoutine = m_AttackMethod.Invoke(m_Player, null) as IEnumerator;
            if (attackRoutine != null)
                StartCoroutine(attackRoutine);
        }
    }
}
