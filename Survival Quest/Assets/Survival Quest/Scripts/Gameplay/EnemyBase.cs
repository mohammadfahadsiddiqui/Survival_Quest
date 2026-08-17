using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    /// <summary>
    /// Enemy foundation with very close player detection, chasing and melee attack.
    /// Enemies stay idle until the player is almost beside them.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float moveSpeed = 2.5f;

        // Very small detection radius: enemy notices the player only when nearby.
        [SerializeField] private float detectionRange = 0.75f;

        // Attack only when the player is essentially beside the enemy.
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

            Vector3 flatTarget = target.position;
            flatTarget.y = transform.position.y;
            float distance = Vector3.Distance(transform.position, flatTarget);

            // Outside 0.75m: completely idle. No chasing and no attack.
            if (distance > detectionRange)
                return;

            // Inside 0.75m but not yet at attack distance: move toward the player.
            if (distance > attackRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    flatTarget,
                    moveSpeed * Time.deltaTime);

                Vector3 direction = flatTarget - transform.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(direction),
                        10f * Time.deltaTime);
                }
            }
            // Inside 0.5m: attack.
            else if (Time.time >= nextAttackTime)
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
