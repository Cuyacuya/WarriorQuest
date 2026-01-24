using UnityEngine;
using WarriorQuest.InputSystem;


namespace WarriorQuest.Characte
{
    [RequireComponent(typeof(Rigidbody2D))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(Animator))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(SpriteRenderer))] //~~타입이 꼭 필요하다!
    [RequireComponent(typeof(InputHandler))] //~~타입이 꼭 필요하다!
    public class Player : MonoBehaviour
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
        public float AttackDage => attackDamage;
        public float AttackCooldown => attackCooldown;
        #endregion

        #region 컴포넌트 캐싱
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        protected InputHandler inputHandler;
        #endregion

        #region 유니티 생명주기
        protected virtual void Awake()
        {
            //초기 체력 설정
            curHp = maxHp;
            //컴포넌트 캣이
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            inputHandler = GetComponent<InputHandler>();
        }
        #endregion
    }


}