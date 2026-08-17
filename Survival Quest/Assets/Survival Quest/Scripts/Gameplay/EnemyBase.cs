using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    /// <summary>
    /// Stationary enemy. The enemy never chases the player.
    /// It only attacks when the player physically comes within attack range.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float attackRange = 0.5f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Target")]
        [SerializeField] private Transform target;

        private float health;
        private float nextAttackTime;

        public float Health => health;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            health = maxHealth;

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
        }

        private void Update()
        {
            if (IsDead || target == null)
                return;

            // The enemy NEVER moves toward the player.
            // It only checks whether the player has reached its position.
            Vector3 flatTarget = target.position;
            flatTarget.y = transform.position.y;
            float distance = Vector3.Distance(transform.position, flatTarget);

            if (distance > attackRange)
                return;

            // Player is close enough: attack.
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                target.SendMessage(
                    "TakeDamage",
                    attackDamage,
                    SendMessageOptions.DontRequireReceiver);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f)
                return;

            health -= damage;

            if (health <= 0f)
                Die();
        }

        private void Die()
        {
            IsDead = true;
            SendMessage("OnEnemyDeath", SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject, 0.1f);
        }
    }
}
