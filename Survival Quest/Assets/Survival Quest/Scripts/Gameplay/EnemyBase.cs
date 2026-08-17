using UnityEngine;

namespace SurvivalQuest.Gameplay
{
    /// <summary>
    /// Enemy foundation with nearby-player detection, chasing and melee attack.
    /// Enemies remain idle when the player is outside detectionRange.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float moveSpeed = 2.5f;

        // Enemy will not react to or chase the player outside this range.
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 1.8f;
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

            // Player is too far away: enemy stays completely idle.
            if (distance > detectionRange)
                return;

            // Player has entered the detection area: chase and attack.
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
                        8f * Time.deltaTime);
                }
            }
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
