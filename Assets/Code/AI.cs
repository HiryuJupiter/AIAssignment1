using System;
using System.Collections;
using UnityEngine;

namespace Assets.Code
{
    public class AI : MonoBehaviour
    {
        public enum State
        {
            PATROL,
            SEEK,
            ATTACK,
            FLEE
        }

        [Header("Status")]
        public State state = State.PATROL;

        [Header("Parameters")]
        public float MoveSpeed_patrol = 4.0f;
        public float MoveSpeed_chase = 6.0f;
        public float MinDistanceToWaypoint = 0.2f;
        public float ChaseRange = 2f;
        public float AttackRange = 1f;
        public int Damage = 1;
        public int MaxHealth = 100;
        public float attackCD = 0.1f;

        [Header("References")]
        public Transform[] Waypoints;
        public PlayerController Player;
        public SpriteRenderer Sprite;

        int currentWaypoint = 0; //Current waypoint index
        int currentHealth;

        float attackTimer;
        

        void Start()
        {
            //Initialization
            currentHealth = MaxHealth;

            //Run starting state
            BeginNextState();
        }

        private void Update()
        {
            //For every frame, check whether the player is in critical health. This allows 
            //...the AI to override current state and go directly into Flee state.
            if (IsInDanger())
            {
                state = State.FLEE;
            }
        }

        #region State Update
        void BeginNextState()
        {
            //Call state coroutine-methods based on current state.
            switch (state)
            {
                case State.PATROL:
                    StartCoroutine(PatrolState());
                    break;
                case State.SEEK:
                    StartCoroutine(ChaseState());
                    break;
                case State.ATTACK:
                    StartCoroutine(AttackState());
                    break;
                case State.FLEE:
                    StartCoroutine(FleeState());
                    break;
                default:
                    Debug.Log(state.ToString() + " does not exist.");
                    break;
            }
        }

        IEnumerator PatrolState()
        {
            Debug.Log("Enters PatrolState");
            Sprite.color = Color.green;

            while (state == State.PATROL)
            {
                if (IsPlayerInRange(ChaseRange))
                {
                    state = State.SEEK;
                }
                else
                {
                    PatrolWayPoint();
                }
                yield return null;
            }

            //Exit state
            BeginNextState();
        }

        IEnumerator ChaseState()
        {
            Debug.Log("Enters ChaseState");
            Sprite.color = Color.yellow;

            while (state == State.SEEK)
            {
                //If player is in attack range, start attacking;
                //If player stays in chase range, then chase;
                //If player moved outside of chase range, then return to patrol.
                if (IsPlayerInRange(AttackRange))
                {
                    state = State.ATTACK;
                }
                else if (IsPlayerInRange(ChaseRange))
                {
                    MoveTowards(Player.transform.position, MoveSpeed_chase);
                }
                else
                {
                    state = State.PATROL;
                }
                yield return null;
            }

            //Exit state
            BeginNextState();
        }

        IEnumerator AttackState()
        {
            Debug.Log("Enters AttackState");
            Sprite.color = Color.red;

            while (state == State.ATTACK)
            {
                //If player stays in attack range, then attack.
                //If player moved outside of attack range, then chase;
                if (IsPlayerInRange(AttackRange))
                {
                    AttackPlayer();
                }
                else
                {
                    state = State.SEEK;
                }
                yield return null;
            }

            //Exit state
            BeginNextState();
        }

        IEnumerator FleeState ()
        {
            Debug.Log("Enters FleeState");
            Sprite.color = Color.cyan;

            while (state == State.FLEE)
            {
                //If AI is in critical health, then flee away from player.
                //If AI has somehow recovered health, then stop fleeing and go back to patrolling.
                if (IsInDanger())
                {
                    transform.Translate((transform.position - Player.transform.position).normalized * MoveSpeed_patrol * Time.deltaTime);
                }
                else
                {
                    state = State.SEEK;
                }
                yield return null;
            }

            //Exit state
            BeginNextState();
        }
        #endregion

        #region Behaviors
        void PatrolWayPoint ()
        {
            //If we reached current waypoint, go to the next waypoint.
            if (Vector2.Distance(transform.position, Waypoints[currentWaypoint].position) < 0.2f)
            {
                currentWaypoint = (currentWaypoint >= Waypoints.Length - 1) ? 0 : currentWaypoint + 1;
            }

            //Move towards the current waypoint.
            MoveTowards(Waypoints[currentWaypoint].position, MoveSpeed_patrol);
        }

        void MoveTowards(Vector2 targetPosition, float speed)
        {
            //Move AI towards a target location
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }

        void AttackPlayer ()
        {
            //Attack player if attack cooldown allows it.
            if (attackTimer <= 0f)
            {
                Player.TakeDamage(Damage);
                attackTimer = attackCD;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }
        #endregion

        #region Condition Checks
        bool IsPlayerInRange(float range)
        {
            //Check if player is in range.
            return (Vector2.Distance(Player.transform.position,
                        transform.position)
                      < range);
        }

        bool IsInDanger ()
        {
            //If I'm below 25% health and the player also has higher health than me, then flee.
            return ((currentHealth / (float)MaxHealth) < 0.25f && currentHealth < Player.GetHealth);
        }
        #endregion

        private void OnGUI()
        {
            //Displays AI's health 
            GUI.Label(new Rect(20, 20, 200, 20), "AI Health: " + currentHealth);

            //Debug
            if (Input.GetKeyDown(KeyCode.X))
            {
                currentHealth -= 10;
            }
        }
    }
}