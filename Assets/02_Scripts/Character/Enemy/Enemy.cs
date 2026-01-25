using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WarriorQuest.Character.Enemy.FSM;

namespace WarriorQuest.Character.Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        //상태 머신 구현 예정
        protected StateMachine stateMachine;

        //상태 전환 메서드
        public void ChangeState<T>() where T : IState
        {
            //딕셔너리에서 상태를 가져와 상태 전환
            //IState newState = states[typeof(T)];
            if(states.TryGetValue(typeof(T), out IState state))
            {
                stateMachine.ChangeState(state);
            }
        }
        
        //컴포넌트 캐싱
        protected Rigidbody rb;
        protected SpriteRenderer spriteRenderer;
        protected Animator anim;

        //애니메이션 해시 추출
        protected static readonly int hashIsMoving = Animator.StringToHash("IsMoving");
        protected static readonly int hashHit= Animator.StringToHash("Hiy");


        //상태를 저장할 딕셔너리 선언
        protected Dictionary<Type, IState> states;

        //상태 초기화 추상 메서드
        protected abstract void InisStates();

        #region 유니티 생명주기
        protected virtual void Awake()
        {
            //상태 초기화 호출
            InisStates();

            //컴포넌트 캐싱
            InitComponents();
        }

        protected void Start()
        {
            //상태 머신 초기화
            stateMachine = new StateMachine(this);

            //초기 상태 설정
            ChangeState<IdleState>();
        }

        private void Update()
        {
            //상태 머신 업데이트
            stateMachine.Update();

            //테스트용 상태 전환
            TestFSM();
        }
        #endregion

        #region 초기화 메서드
        private void InitComponents()
        {
            rb = GetComponent<Rigidbody>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
        }
        #endregion

        #region 테스트 코드
        private void TestFSM()
        {
            if(Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ChangeState<IdleState>();
            }
            if(Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ChangeState<ChaseState>();
            }
            if(Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ChangeState<AttackState>();
            }
        }
        #endregion
    }
}