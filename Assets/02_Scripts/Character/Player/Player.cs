using System;
using Unity.VisualScripting;
using UnityEngine;
using WarriorQuest.Character.Interface;
using WarriorQuest.InputSystem;


namespace WarriorQuest.Characte.Player
{
    [RequireComponent(typeof(Rigidbody2D))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(Animator))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(SpriteRenderer))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(InputHandler))] //~~타입이 꼭 필요하다!
    public abstract class Player : MonoBehaviour, IDamageable
    {
        #region 기본 스텟
        [Header("기본 스텟")]
        [SerializeField] protected float maxHp = 100f;
        [SerializeField] protected float curHp = 100f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float attackDamage = 20f;
        [SerializeField] protected float attackCooldown = 0.5f;

        protected bool isDead => curHp <= 0;
        #endregion

        #region 프로퍼티
        public float MaxHp => maxHp;
        public float CurHp => curHp;
        public float MoveSpeed => moveSpeed; 
        public float AttackDamge => attackDamage;
        public float AttackCooldown => attackCooldown;
        #endregion

        #region 컴포넌트 캐싱
        protected Rigidbody2D rb;
        protected Animator anim;
        protected SpriteRenderer spriteRenderer;
        protected InputHandler inputHandler;
        #endregion

        //Facing 처리를 위한 Weapon Transform
        protected Transform weaponArm;

        //애니메이션 파라메터 해시값을 미리 계산
        protected static readonly int hashIsMoving = Animator.StringToHash("IsMoving");
        protected static readonly int hashAttack = Animator.StringToHash("Attack");
        protected static readonly int hashHit = Animator.StringToHash("Hit");

        //마지막 공격 시간
        private float lastAttackTime = 0f;

        #region 유니티 생명주기
        protected virtual void Awake()
        {
            //초기 체력 설정
            curHp = maxHp;
            //컴포넌트 캣이
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            inputHandler = GetComponent<InputHandler>();

            //weaponArm 설정
            weaponArm = transform.Find("Arm");
        }

        protected void OnEnable()
        {
            inputHandler.OnMoveAction += OnMove;
            inputHandler.OnAttackAction += OnAttack;
            inputHandler.OnInteractAction += OnInteraction;
        }
        protected void OnDisable()
        {
            inputHandler.OnMoveAction -= OnMove;
            inputHandler.OnAttackAction -= OnAttack;
            inputHandler.OnInteractAction -= OnInteraction;
        }
        #endregion

        #region 공통 메서드
        //방향 설정
        private void FlipDirection(bool facingRight)
        {
            if(facingRight)
            {
                //오른쪽을 볼 때
                spriteRenderer.flipX = false;
                weaponArm.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                //왼쪽을 바라볼 떄
                spriteRenderer.flipX = true;
                weaponArm.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            curHp -= damage;
            anim.SetTrigger(hashHit);

            if (curHp <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            curHp = 0;
            Debug.Log("플레이어가 사망했습니다.");
        }
        #endregion

        #region 입력 처리 메서드
        private void OnMove(Vector2 ctx)
        {
            if (isDead) return;

            rb.linearVelocity = ctx.normalized * MoveSpeed;

            //방향 전환
            if (ctx.x != 0)
            {
                FlipDirection(ctx.x > 0);
            }

            //애니메이션
            anim.SetBool(hashIsMoving, ctx.sqrMagnitude > 0.01f);
        }
        private void OnAttack()
        {
            if(isDead) return;

            //공격 쿨타임 체크
            if(Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                anim.SetTrigger(hashAttack);
                Attack();
            }
        }

        private void OnInteraction(bool ctx)
        {
            if (isDead) return;
            Debug.Log($"상호작용 : {ctx}");
        }


        #endregion

        #region 추상 메서드
        protected abstract void Attack();

        #endregion
    }


}