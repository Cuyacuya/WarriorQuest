using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WarriorQuest.InputSystem
{
    public class InputHandler : MonoBehaviour
    {
        //InputSystem_Actions의 인스턴스
        private InputSystem_Actions inputActions;

        //액션 변수 선언
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction interactAction;

        public event Action<Vector2> OnMoveAction;
        public event Action OnAttackAction;
        public event Action<bool> OnInteractAction;

        #region 유니티 라이프사이클
        private void Awake() //메모리 할당하고 각 변수에 액션값을 저장함
        {
            inputActions = new InputSystem_Actions();
            moveAction = inputActions.Player.Move; //wasd or Arrow Keys
            attackAction = inputActions.Player.Attack;
            interactAction = inputActions.Player.Interact;
        }

        private void OnEnable()
        {
            //입력 시스템을 활성화
            inputActions.Enable();

            moveAction.performed += onMove;
            moveAction.canceled += onMove;

            attackAction.performed += onAttack;

            interactAction.performed += onInteract;
            interactAction.canceled += onInteract;
        }


        private void OnDisable()
        {
            inputActions.Disable();

            moveAction.performed -= onMove;
            moveAction.canceled -= onMove;

            attackAction.performed -= onAttack;

            interactAction.performed -= onInteract;
            interactAction.canceled -= onInteract;
        }

        #endregion

        //콜백 메서드(Callback Methods) : 특정 메서드가 호출되었을 때 자동적으로 그 후에 호출되는 메서드
        #region 콜백 메서드
        private void onMove(InputAction.CallbackContext context)
        {
            //Debug.Log($"Move : {context.ReadValue<Vector2>()}"); //ReadValue : context의 읽기 값을 리턴하는 명령
            OnMoveAction?.Invoke(context.ReadValue<Vector2>());
        }

        private void onAttack(InputAction.CallbackContext context)
        {
            //Debug.Log("공격");
            OnAttackAction?.Invoke();
        }

        private void onInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) //눌려져 있는지 확인
            {
                //Debug.Log("상호작용 시작");
                OnInteractAction?.Invoke(true);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                //Debug.Log("상호작용 끝");
                OnInteractAction?.Invoke(false);
            }
        }



        #endregion
    }
}

