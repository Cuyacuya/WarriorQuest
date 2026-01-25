using UnityEngine;
using UnityEngine.InputSystem;
using WarriorQuest.Character.Enemy.FSM;

namespace WarriorQuest.Character.Enemy
{
    public class Enemy : MonoBehaviour
    {
        //상태 머신 구현 예정
        protected StateMachine stateMachine;

        //상태 전환 메서드
        public void ChangeState(IState newState)
        {
            stateMachine.ChangeState(newState);
        }

        #region 유니티 생명주기
        protected virtual void Awake()
        {
            //상태 머신 초기화
            stateMachine = new StateMachine(this);

            //초기 상태 설정
            ChangeState(new IdleState());
        }

        private void Update()
        {
            //상태 머신 업데이트
            stateMachine.Update();

            //테스트용 상태 전환
            TestFSM();
        }
        #endregion

        #region 테스트 코드
        private void TestFSM()
        {
            if(Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ChangeState(new IdleState());
            }
            if(Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ChangeState(new ChaseState());
            }
            if(Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ChangeState(new AttackState());
            }
        }
        #endregion
    }
}