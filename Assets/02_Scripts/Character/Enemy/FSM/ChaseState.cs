using Unity.VisualScripting;
using UnityEngine;

namespace WarriorQuest.Character.Enemy.FSM
{
    public class ChaseState : IState
    {
        public void OnEnter(Enemy enemy)
        {
            Debug.Log("ChaseState 진입");
        }

        public void OnUpdate(Enemy enemy)
        {
            Debug.Log("ChaseState 유지");
        }

        public void OnExit(Enemy enemy)
        {
            Debug.Log("ChaseState 탈출");
        }
    }
}
