using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class ChaseState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            Debug.Log("ChaseState 진입");
            enemy.anim.SetBool(Enemy.hashIsMoving, true);

        }

        public void OnUpdate(Enemy enemy)
        {
            if (enemy.PlayerDetectable())
            {
                if (enemy.DetectPlayer())
                {
                    //추적 로직
                    enemy.MoveToPlayer();
                }
                else
                {
                    //정치 호출
                    enemy.StopMoving();
                    enemy.ChangeState<IdleState>();
                }
            }
        }

        public void OnExit(Enemy enemy)
        {
            Debug.Log("ChaseState 탈출");
        }
    }
}
