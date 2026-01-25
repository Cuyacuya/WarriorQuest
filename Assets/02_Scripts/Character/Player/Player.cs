using UnityEngine;
using WarriorQuest.Character.Interface;
using WarriorQuest.InputSystem;

namespace WarriorQuest.Character.Player
{
    [RequireComponent(typeof(Rigidbody2D))] //~~Ÿ���� �� �ʿ��ϴ�!
    [RequireComponent(typeof(Animator))] //~~Ÿ���� �� �ʿ��ϴ�!
    [RequireComponent(typeof(SpriteRenderer))] //~~Ÿ���� �� �ʿ��ϴ�!
    [RequireComponent(typeof(InputHandler))] //~~Ÿ���� �� �ʿ��ϴ�!
    public abstract class Player : MonoBehaviour, IDamageable
    {
        #region �⺻ ����
        [Header("�⺻ ����")]
        [SerializeField] protected float maxHp = 100f;
        [SerializeField] protected float curHp = 100f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float attackDamage = 20f;
        [SerializeField] protected float attackCooldown = 0.5f;

        protected bool isDead => curHp <= 0;
        #endregion

        #region ������Ƽ
        public float MaxHp => maxHp;
        public float CurHp => curHp;
        public float MoveSpeed => moveSpeed; 
        public float AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;
        #endregion

        #region ������Ʈ ĳ��
        protected Rigidbody2D rb;
        protected Animator anim;
        protected SpriteRenderer spriteRenderer;
        protected InputHandler inputHandler;
        #endregion

        //Facing ó���� ���� Weapon Transform
        protected Transform weaponArm;

        //�ִϸ��̼� �Ķ���� �ؽð��� �̸� ���
        protected static readonly int hashIsMoving = Animator.StringToHash("IsMoving");
        protected static readonly int hashAttack = Animator.StringToHash("Attack");
        protected static readonly int hashHit = Animator.StringToHash("Hit");

        //������ ���� �ð�
        private float lastAttackTime = 0f;

        #region ����Ƽ �����ֱ�
        protected virtual void Awake()
        {
            //�ʱ� ü�� ����
            curHp = maxHp;
            //������Ʈ Ĺ��
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            inputHandler = GetComponent<InputHandler>();

            //weaponArm ����
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

        #region ���� �޼���
        //���� ����
        private void FlipDirection(bool facingRight)
        {
            if(facingRight)
            {
                //�������� �� ��
                spriteRenderer.flipX = false;
                weaponArm.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                //������ �ٶ� ��
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
            Debug.Log("�÷��̾ ����߽��ϴ�.");
        }
        #endregion

        #region �Է� ó�� �޼���
        private void OnMove(Vector2 ctx)
        {
            if (isDead) return;

            rb.linearVelocity = ctx.normalized * MoveSpeed;

            //���� ��ȯ
            if (ctx.x != 0)
            {
                FlipDirection(ctx.x > 0);
            }

            //�ִϸ��̼�
            anim.SetBool(hashIsMoving, ctx.sqrMagnitude > 0.01f);
        }
        private void OnAttack()
        {
            if(isDead) return;

            //���� ��Ÿ�� üũ
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
            Debug.Log($"��ȣ�ۿ� : {ctx}");
        }


        #endregion

        #region �߻� �޼���
        protected abstract void Attack();

        #endregion
    }


}