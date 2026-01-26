using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class AttackState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            Debug.Log("AttackState 진입");
        }

        public void OnUpdate(Enemy enemy)
        {
            Debug.Log("AttackState 유지");
        }

        public void OnExit(Enemy enemy)
        {
            Debug.Log("AttackState 탈출");
        }
    }
}
