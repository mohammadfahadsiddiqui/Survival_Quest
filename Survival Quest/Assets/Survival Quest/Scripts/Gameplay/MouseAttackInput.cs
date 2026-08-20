using System.Collections;
using UnityEngine;

namespace SurvivalGame
{
    /// <summary>
    /// Direct desktop mouse attack input.
    /// Left mouse button starts the same attack animation and hit logic used by the player.
    /// </summary>
    public class MouseAttackInput : MonoBehaviour
    {
        private Player m_Player;

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
        }

        private void Update()
        {
            if (m_Player == null)
                m_Player = Player.m_Current != null
                    ? Player.m_Current
                    : Object.FindFirstObjectByType<Player>();

            if (m_Player == null || !Input.GetMouseButtonDown(0))
                return;

            StartCoroutine(MouseAttack());
        }

        private IEnumerator MouseAttack()
        {
            if (m_Player == null || !m_Player.m_CanHit)
                yield break;

            m_Player.m_CanHit = false;

            if (m_Player.m_Animator != null)
                m_Player.m_Animator.Play("hit-1", 0, 0f);

            // Match the existing melee attack timing in Player.Co_Attack().
            yield return new WaitForSeconds(0.3f);

            float damage = GetCurrentWeaponDamage();
            m_Player.CheckHit(damage);

            yield return new WaitForSeconds(0.2f);
            m_Player.m_CanHit = true;
        }

        private float GetCurrentWeaponDamage()
        {
            int weapon = m_Player.m_WeaponInHand;

            if (GameControl.m_Current != null &&
                GameControl.m_Current.m_Contents != null &&
                GameControl.m_Current.m_Contents.m_Equipment != null &&
                weapon >= 0 &&
                weapon < GameControl.m_Current.m_Contents.m_Equipment.Length &&
                GameControl.m_Current.m_Contents.m_Equipment[weapon] != null)
            {
                return GameControl.m_Current.m_Contents.m_Equipment[weapon].m_Damage;
            }

            // Safe fallbacks matching the existing Player attack behaviour.
            switch (weapon)
            {
                case 1: return 15f;
                case 2: return 20f;
                default: return 10f;
            }
        }
    }
}
