using UnityEngine;
using UnityEngine.AI;
namespace AI.Code_for_AI
{
    public class MoveEnemy : MonoBehaviour
    {
        // Start is called before the first frame update
        public enum EnemyState
        {
            Idle,Moving,Following
        }
    
        private NavMeshAgent _navMeshAgent;
        private GameObject _player;
        public EnemyState currentState = EnemyState.Idle;

        void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _player = GameObject.FindWithTag("Player");
        }

        // Update is called once per frame
        void Update()
        {
            switch (currentState){
                case EnemyState.Idle:
                    // do nothing 
                    break;
                case EnemyState.Moving:
                    if (!_navMeshAgent.pathPending && NavMeshAgent.remainingDistance<0.1f){
                        SetRandomDistination();
                    }
                    break;
                case EnemyState.Following:
                    _navMeshAgent.SetDestination(_player.transform.position);
                    break;
            }

            float distanceToPlayer = Vector3.Distance(Transform.position, _player.transform.position);
            if ((currentState is EnemyState.Idle or currentState == EnemyState.Moving) && distanceToPlayer <5f){
                currentState = EnemyState.Following;
        
            }
            if((currentState == EnemyState.Following) && distanceToPlayer>5f){
                currentState = EnemyState.Moving;
            }
        }
        void SetRandomDestinationDestination(){
            Vector3 randomDirection = Random.insideUnitSphere *10f;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);
            NavMeshAgent.SetDestination(hit.position);
        }
    }
}
