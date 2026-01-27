using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class IdleState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            enemy.anim.SetBool(Enemy.hashIsMoving, false);
        }

        public void OnUpdate(Enemy enemy)
        {
            if (enemy.PlayerDetectable())
            {
                if (enemy.DetectPlayer())
                {
                    enemy.ChangeState<ChaseState>();
                }
            }
        }

        public void OnExit(Enemy enemy)
        {
        }
    }
}
