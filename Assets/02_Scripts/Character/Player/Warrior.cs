using System;
using System.Collections;
using _02_Scripts.Event;
using UnityEngine;
using WarriorQuest.Character.Interface;
using  WarriorQuest.Audio;
using WarriorQuest.InventorySystem;

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
        
        //[Header("Events")] [SerializeField] HealthEventSO healthEventSO;

        #region 유니티 라이프사이클
        protected override void Awake()
        {
            maxHp = warriorSo.maxHp;
            moveSpeed = warriorSo.moveSpeed;
            attackDamage = warriorSo.attackDamage;
            attackCooldown = warriorSo.attackCooldown;
            defense = warriorSo.defense;

            Debug.Log($"전사 클래스가 생성되었습니다. 방어력 : {warriorSo.defense}");
            base.Awake();
        }

        private IEnumerator Start()
        {
            yield return null;
            //인벤토리 start call - 초기화
            yield return null;
            
            var inventory = FindFirstObjectByType<Inventory>();
            SetInitializeItems(inventory);
        }

        #endregion

        #region 초기 아이템 지급

        private void SetInitializeItems(Inventory inventory)
        {
            //기본 무기 (Rusty Sword) - 자동 장착
            ItemData defaultWeapon = Resources.Load<ItemData>("ItemData/RustySword");

            if (defaultWeapon != null)
            {
                defaultWeapon.isEquip = true;
                var success = inventory.AddItem(defaultWeapon);

                if (success)
                {
                    Debug.Log("기본 무기 장착됨.");
                }
            }
            else
            {
                Debug.Log("기본 장착 무기 데이터가 없습니다.");
            }

            //회복 포션
            ItemData hpPortion = Resources.Load<ItemData>("ItemData/HpPortionLarge");

            if (hpPortion != null)
            {
                inventory.AddItem(hpPortion);
                inventory.AddItem(hpPortion);
                Debug.Log("회복 포션 2개가 지급 되었습니다.");
            }
            else
            {
                Debug.Log("회복 포션 데이터가 없습니다.");

            }
        }

        #endregion

        #region 공격 및 데미지 처리
        protected override void Attack()
        {
            //공격 사운드 재샐
            AudioManager.Instance.PlayerSFX(AudioManager.Instance.audioData.playerAttackSFX);

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
            AudioManager.Instance.StopPlayerSFX();
            
            //방어력 적용
            float actualDamage = Mathf.Max(damage- warriorSo.defense, 5f);

            base.TakeDamage(actualDamage);
            
            //데미지 이벤트 발생 요청
            healthEventSO.Raise(curHp, maxHp);
            //Debug.Log($"Warrior가 {actualDamage}를 받았습니다. 현재 체력 : {curHp}/{maxHp}");
        }
        #endregion
        

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
