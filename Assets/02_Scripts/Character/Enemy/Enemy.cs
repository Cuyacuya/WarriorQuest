using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using WarriorQuest.Character.Enemy.FSM;
using WarriorQuest.Character.Interface;

namespace WarriorQuest.Character.Enemy
{
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        // [Header("기본 스텟 Data")]
        [SerializeField] protected float curHp;
       
        //사망 여부 프로퍼티
        public bool IsDaed => curHp <= 0;
        
        //Player 레이어 마스크
        [Header("Player 레이어 마스크")]
        [SerializeField] protected LayerMask playerMask;
        
        //추적 대상
        public Transform target;
        
        //추적 검출 간격
        [SerializeField] private float detectInterval = 0.3f;
        private float lastDetectTime = 0f;
        
        //상태 머신 변수
        protected StateMachine stateMachine;

        //상태 머신 프로퍼티
        public StateMachine StateMachine => stateMachine;

        //현재 상태를 표시
        public string CurStateName => StateMachine?.curState.GetType().Name ?? "None";

        //컴포넌트 캐싱
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        [NonSerialized]public Animator anim;

        //애니메이션 해시 변수
        public static readonly int hashIsMoving = Animator.StringToHash("IsMoving");
        protected static readonly int hashHit= Animator.StringToHash("Hit");
        
        //상태를 저장할 딕셔너리 변수
        protected Dictionary<Type, IState> states;

        //기본 스텟 SO
        public EnemySO enemySo;
        
        //상태 초기화 추상 메서드
        protected abstract void InitStates();

        #region 유니티 라이프사이클
        protected virtual void Awake()
        {
            //상태 초기화 호출
            InitStates();

            //컴포넌트 캐싱
            InitComponents();
        }

        protected void Start()
        {
            //상태 머신 초기화
            stateMachine = new StateMachine(this);

            //초기 상태 설정
            ChangeState<IdleState>();
            
            //기본 스텟 초기화
            InitBasicStats();
        }

        private void Update()
        {
            //사망 여부 판단
            
            //FSM DeadState
            if (IsDaed) return;
            
            //상태 머신 업데이트
            stateMachine.Update();
        }
        #endregion

        #region 상태 관련 메서드
        //상태 전환 메서드
        public void ChangeState<T>() where T : IState
        {
            //딕셔너리에서 상태를 가져와 상태 전환
            //IState newState = states[typeof(T)];
            if(states.TryGetValue(typeof(T), out IState state))
            {
                stateMachine.ChangeState(state);
            }
        }
        
        //주인공을 검출하는 메서드
        public bool DetectPlayer()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, enemySo.chaseDistance, playerMask);

            if (colliders.Length > 0)
            {
                //LINQ를 사용하여 가장 가까운 플레이어 선택 (SQL)
                target = colliders
                    .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                    .First()
                    .transform;
                
                return target != null;
            }

            target = null;
            return false;
        }
        
        //검출 시간이 초과했는지 여부를 확인하는 메서드
        public bool PlayerDetectable()
        {
            if (Time.time >= lastDetectTime + detectInterval)
            {
                lastDetectTime = Time.time;
                return true;
            }
            return false;
        }
        
        //추적 로직
        public void MoveToPlayer()
        {
            if (target == null) return;
            
            //이동 방향 계산 (플레이어 위치 - 적 위치).nomarlized
            Vector2 direction = (target.position - transform.position).normalized;
            //Target의 위치에 따라 스프라이트 Flip 처리
            spriteRenderer.flipX = direction.x < 0; //왼쪽을 바라봄.
            //rigidbody2D를 사용하여 이동 처리
            rb.linearVelocity = direction * enemySo.moveSpeed;
        }
        
        //정지 메서드
        public void StopMoving()
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        //공격 쿨타임이 지났는지 확인하는 메서드
        public bool CanAttack(float lastAttackTime)
        {
            if (Time.time >= lastAttackTime + enemySo.attackCooldown)
            {
                return true;
            }

            return false;
        }
        
        //공격 사정거리 이내에 플레이어 존재 여부 확인 
        public bool IsPlayerAttackRange()
        {
            float attackRange = Vector2.Distance(transform.position, target.position);
            return (attackRange <= enemySo.attackDistance);
        }

        #endregion

        #region 초기화 메서드
        private void InitComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
        }
        
        private void InitBasicStats()
        {
            //maxHp = enemySo.maxHp;
            curHp = enemySo.maxHp;
            // moveSpeed = enemySo.moveSpeed;
            // chaseDistance = enemySo.chaseDistance;
            // attackDistance = enemySo.attackDistance;
            // attackCooldown = enemySo.attackCooldown;
        }
        #endregion

        #region 테스트 코드
        private void TestFSM()
        {
            if(Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ChangeState<IdleState>();
            }
            if(Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ChangeState<ChaseState>();
            }
            if(Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ChangeState<AttackState>();
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemySo.chaseDistance); //외각선(원모양) 그리기
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemySo.attackDistance);
        }
        #endregion

        #region 피격 관련 메서드

        public virtual void TakeDamage(float damage)
        {
            if (IsDaed) return;
            
            curHp -= damage;

            if (curHp <= 0)
            {
                Die();
            }
            else
            {
                //Hit 처리 애니메이션
                anim.SetTrigger(hashHit);
            }
        }

        protected void Die()
        {
            StartCoroutine(DestroyEnemy());
        }

        private IEnumerator DestroyEnemy()
        {
            yield return new WaitForSeconds(1.0f);
            Destroy(gameObject);
        }
        #endregion
        
    }
}
