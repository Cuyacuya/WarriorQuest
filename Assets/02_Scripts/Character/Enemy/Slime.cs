using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarriorQuest.Character.Enemy.FSM;
using WarriorQuest.Character.Interface;

namespace WarriorQuest.Character.Enemy
{
    public class Slime : Enemy
    {
        [Header("슬라임 공격 스텟")] [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private float returnSpeed = 8f;
        [SerializeField] private float dashDistance = 0.5f;
        [SerializeField] private float waitingTime = 0.2f;

        [Header("넉백 설정")] 
        [SerializeField] private float knockbackSpeed = 15f;
        [SerializeField] private float knockbackDistance = 0.5f;
        
        private Vector2 originPosition;
        public bool IsAttacking { get; private set; } = false;
        public float LastAttackTime { get; private set; }


        protected override void InitStates()
        {
            //슬라임 전용상태 초기화
            states = new Dictionary<Type, IState>
            {
                [typeof(IdleState)] = new IdleState(),
                [typeof(ChaseState)] = new ChaseState(),
                [typeof(AttackState)] = new AttackState(),
            };

            Debug.Log("Slime 상태 초기화 완료");
        }

        public IEnumerator DashAttack()
        {
            IsAttacking = true;
            //마지막 공격 시간 저장
            LastAttackTime = Time.time;
            //현재 위치를 저장
            originPosition = transform.position;

            //목표 좌표 계산
            //현재 위치 Vector2
            Vector2 curPosition = new Vector2(transform.position.x, transform.position.y);
            //공격 방향 벡터 계산
            Vector2 dashDir = (target.position - transform.position).normalized;
            //공격 좌표 계산 (현재 위치 + (방향 * 거리))
            Vector2 dashTarget = curPosition + (dashDir * dashDistance);

            //실제 이동한 시간
            float dashTime = 0f;
            //이동 시간 = 거리 / 속도
            float dashDuration = dashDistance / dashSpeed;

            yield return new WaitForSeconds(waitingTime);

            //While : Dash
            while (dashTime < dashDuration)
            {
                if(!IsAttacking) yield break;
                transform.position = Vector2.MoveTowards(transform.position, dashTarget, dashSpeed * Time.deltaTime);
                dashTime += Time.deltaTime;
                yield return null;
            }

            //잠시 대기
            yield return new WaitForSeconds(waitingTime);

            //원위치 복귀
            float returnTime = 0f;
            float returnDistance = Vector2.Distance(transform.position, originPosition);
            float returnDuration = returnDistance / returnSpeed;

            //While : Return
            while (returnTime < returnDuration)
            {
                transform.position =
                    Vector2.MoveTowards(transform.position, originPosition, dashSpeed * Time.deltaTime);
                returnTime += Time.deltaTime;

                yield return null;
            }

            IsAttacking = false;
        }
        #region 출동감지 로직
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<IDamageable>().TakeDamage(enemySo.attackDamage);
            }
        }

        #endregion

        #region 피격 처리

        public override void TakeDamage(float damage)
        {
            //공격 코루틴 정지
            if (IsAttacking)
            {
                IsAttacking = false;
            }
            
            base.TakeDamage(damage);
            
            //넉백호출
            StartCoroutine(Knockback());

        }
        
        //넉백 처리 코루틴
        private IEnumerator Knockback()
        {
            //넉백 방향 벡터 계산
            Vector2 knockbackDir = (transform.position - target.position).normalized;
            float knockbackTime = 0f;
            
            //넉백 시간 계산
            float duration = knockbackDistance / knockbackSpeed;
            while (knockbackTime < duration)
            {
                transform.Translate(knockbackDir * knockbackSpeed * Time.deltaTime);
                knockbackTime += Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    }
    
    
}
