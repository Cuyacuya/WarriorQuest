using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class AttackState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            //공격을 위해 이동 정지
            enemy.StopMoving();
        }

        public void OnUpdate(Enemy enemy)
        {
            if (!enemy.IsPlayerAttackRange())
            {
                enemy.ChangeState<ChaseState>();
                return;
                
            }
            
            if (enemy is Slime slime && enemy.CanAttack(slime.LastAttackTime))
            {
                enemy.StartCoroutine(slime.DashAttack());
            }
        }

        public void OnExit(Enemy enemy)
        {
        }
    }
}
