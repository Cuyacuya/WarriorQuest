using UnityEngine;
using WarriorQuest.Character.Interface;

namespace WarriorQuest.Character.Player
{
    public class Warrior : Player
    {
        [Header("전사 고유 스탯")]
        [SerializeField] private WarriorSO warriorSo;
        
        [Header("적 검출 설정")]
        [SerializeField] private Vector2 size = new Vector2(1f,2f);
        [SerializeField] private float offset = 0.5f;
        [SerializeField] private LayerMask enemyLayer;

        #region 유니티 라이프사이클
        protected override void Awake()
        {
            maxHp = warriorSo.maxHp;
            moveSpeed = warriorSo.moveSpeed;
            attackDamage = warriorSo.attackDamage;
            attackCooldown = warriorSo.attackCooldown;

            Debug.Log($"전사 클래스가 생성되었습니다. 방어력 : {warriorSo.defense}");
            base.Awake();
        }
        #endregion
        protected override void Attack()
        {
            Debug.Log("공격!!!");

        }

        //애니메이션의 이벤트에서 호출될 메서드
        public void OnAttackAnimationEvent()
        {
            //공격 판정 처리 예정
            Debug.Log("공격 애니메이션 이벤트 발생 - 공격 판정 처리");
            
            //공격 범위 계산
            //방향
            Vector2 dircetion = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + (dircetion * offset);

            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0 ,enemyLayer);

            foreach (var col in colliders)
            {
                col.GetComponent<IDamageable>()?.TakeDamage(warriorSo.attackDamage);
            }
        }

        public override void TakeDamage(float damage)
        {
            //방어력 적용
            float actualDamage = Mathf.Max(damage- warriorSo.defense, 5f);

            base.TakeDamage(actualDamage);

            Debug.Log($"Warrior가 {actualDamage}를 받았습니다. 현재 체력 : {curHp}/{maxHp}");
        }

        #region Gizmos

        private void OnDrawGizmos()
        {
            if(spriteRenderer == null)spriteRenderer = GetComponent<SpriteRenderer>();
            
            Vector2 dircetion = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + (dircetion * offset);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(size.x, size.y, 0f));
        }

        #endregion
    }
}
