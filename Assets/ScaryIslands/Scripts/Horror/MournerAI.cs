using UnityEngine;
using UnityEngine.AI;
using ScaryIslands.Combat;

namespace ScaryIslands.Horror
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(MonsterHealth))]
    public sealed class MournerAI : MonoBehaviour
    {
        private enum State { Roam, Investigate, Hunt }

        [SerializeField] private Transform player;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField, Min(1f)] private float sightRange = 14f;
        [SerializeField, Range(1f, 180f)] private float viewAngle = 65f;
        [SerializeField] private LayerMask sightMask = ~0;

        private NavMeshAgent agent;
        private MonsterHealth health;
        private State state;
        private int patrolIndex;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<MonsterHealth>();
        }

        private void OnEnable()
        {
            NoiseBus.Emitted += Hear;
            if (health != null) health.Died += OnDied;
        }

        private void OnDisable()
        {
            NoiseBus.Emitted -= Hear;
            if (health != null) health.Died -= OnDied;
        }

        private void Start() => Patrol();

        private void Update()
        {
            if (health != null && !health.IsAlive) return;

            if (CanSeePlayer())
            {
                state = State.Hunt;
                agent.speed = 4.2f;
                agent.SetDestination(player.position);
            }
            else if (state == State.Hunt)
            {
                state = State.Investigate;
                agent.speed = 2.8f;
            }

            if (!agent.pathPending && agent.remainingDistance < .8f && state != State.Hunt)
                Patrol();
        }

        private void Hear(NoiseEvent e)
        {
            if (health != null && !health.IsAlive) return;
            if (Vector3.Distance(transform.position, e.Position) > Mathf.Lerp(4f, 28f, e.Loudness)) return;

            state = State.Investigate;
            agent.speed = 3f;
            agent.SetDestination(e.Position);
        }

        private bool CanSeePlayer()
        {
            if (player == null) return false;
            Vector3 delta = player.position - transform.position;
            return delta.magnitude <= sightRange &&
                   Vector3.Angle(transform.forward, delta) <= viewAngle * .5f &&
                   Physics.Raycast(transform.position + Vector3.up * 1.6f, delta.normalized, out var hit, sightRange, sightMask) &&
                   hit.transform == player;
        }

        private void Patrol()
        {
            state = State.Roam;
            agent.speed = 1.7f;
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            agent.SetDestination(patrolPoints[patrolIndex++ % patrolPoints.Length].position);
        }

        private void OnDied(MonsterHealth _)
        {
            if (agent != null)
                agent.isStopped = true;
        }
    }
}
