# Unity 적 AI 구현 방법론 가이드

> 이 문서는 Unity에서 적 AI를 구현하기 위한 다양한 방법론과 실전 예시를 다룹니다.

---

## 목차

1. [AI 구현 방법론 개요](#1-ai-구현-방법론-개요)
2. [FSM (Finite State Machine)](#2-fsm-finite-state-machine)
3. [Behavior Tree](#3-behavior-tree)
4. [기타 방법론](#4-기타-방법론)
5. [Unity 특화 기능](#5-unity-특화-기능)
6. [WarriorQuest 적용 예시](#6-warriorquest-적용-예시)
7. [비교 및 추천](#7-비교-및-추천)
8. [학습 경로](#8-학습-경로)
9. [참고 리소스](#9-참고-리소스)

---

## 1. AI 구현 방법론 개요

### 1.1 게임 AI란?

게임 AI는 플레이어에게 도전적이고 흥미로운 경험을 제공하기 위해 NPC(Non-Player Character)의 행동을 제어하는 시스템입니다. 진정한 "지능"보다는 **플레이어가 지능적이라고 느끼는 행동**을 만드는 것이 목표입니다.

### 1.2 적 AI의 주요 역할

| 역할 | 설명 |
|------|------|
| **감지(Perception)** | 플레이어 위치, 소리, 시야 등 환경 인식 |
| **의사결정(Decision)** | 현재 상황에서 최적의 행동 선택 |
| **행동(Action)** | 이동, 공격, 회피 등 실제 동작 수행 |
| **기억(Memory)** | 과거 정보 저장 및 활용 |

### 1.3 방법론 선택 기준

```
복잡도 낮음 ◄────────────────────────────► 복잡도 높음

  [단순 조건문] → [FSM] → [Behavior Tree] → [GOAP/Utility AI]

  - 적 종류 수
  - 행동 패턴 복잡도
  - 팀 규모 및 개발 기간
  - 확장 가능성 요구사항
```

---

## 2. FSM (Finite State Machine)

### 2.1 개념 설명

FSM은 **유한한 수의 상태(State)**와 **상태 간 전이(Transition)**로 구성된 시스템입니다. 한 번에 하나의 상태만 활성화되며, 조건에 따라 다른 상태로 전이됩니다.

### 2.2 상태/전이 다이어그램

```
                    ┌─────────────────────────────────────┐
                    │                                     │
                    ▼                                     │
              ┌──────────┐                                │
              │          │     플레이어 감지              │
     시작 ──► │   Idle   │ ──────────────────┐            │
              │          │                   │            │
              └──────────┘                   ▼            │
                    ▲               ┌──────────┐          │
                    │               │          │          │
                    │  감지 해제    │  Chase   │          │
                    └───────────────│          │          │
                                    └──────────┘          │
                                         │                │
                                         │ 공격 범위 진입 │
                                         ▼                │
                                    ┌──────────┐          │
                                    │          │          │
                        HP 0 ──────►│  Attack  │──────────┤
                            │       │          │   대상 이탈
                            │       └──────────┘
                            ▼              │
                       ┌──────────┐        │ HP 낮음
                       │          │        ▼
                       │   Dead   │   ┌──────────┐
                       │          │   │          │
                       └──────────┘   │   Flee   │
                                      │          │
                                      └──────────┘
```

### 2.3 Unity 구현 코드 예시

#### 기본 구조 - Enum 기반 FSM

```csharp
using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Flee,
    Dead
}

public class EnemyFSM : MonoBehaviour
{
    [Header("상태")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    [Header("설정")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float fleeHealthThreshold = 20f;

    [Header("참조")]
    [SerializeField] private Transform player;

    private float currentHealth = 100f;

    private void Update()
    {
        // 상태별 행동 실행
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Flee:
                UpdateFlee();
                break;
            case EnemyState.Dead:
                // 아무 동작 안함
                break;
        }
    }

    private void UpdateIdle()
    {
        // 전이 조건 체크
        if (IsPlayerInRange(detectionRange))
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void UpdatePatrol()
    {
        // 순찰 로직
        PatrolBehavior();

        if (IsPlayerInRange(detectionRange))
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void UpdateChase()
    {
        // 추격 로직
        MoveTowardsPlayer();

        if (!IsPlayerInRange(detectionRange))
        {
            ChangeState(EnemyState.Idle);
        }
        else if (IsPlayerInRange(attackRange))
        {
            ChangeState(EnemyState.Attack);
        }
        else if (currentHealth <= fleeHealthThreshold)
        {
            ChangeState(EnemyState.Flee);
        }
    }

    private void UpdateAttack()
    {
        // 공격 로직
        AttackPlayer();

        if (!IsPlayerInRange(attackRange))
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void UpdateFlee()
    {
        // 도주 로직
        MoveAwayFromPlayer();
    }

    private void ChangeState(EnemyState newState)
    {
        // 상태 종료 처리
        OnExitState(currentState);

        currentState = newState;

        // 상태 진입 처리
        OnEnterState(newState);
    }

    private void OnEnterState(EnemyState state)
    {
        Debug.Log($"상태 진입: {state}");
        // 상태별 초기화 로직
    }

    private void OnExitState(EnemyState state)
    {
        Debug.Log($"상태 종료: {state}");
        // 상태별 정리 로직
    }

    // 헬퍼 메서드
    private bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    private void MoveTowardsPlayer() { /* 구현 */ }
    private void MoveAwayFromPlayer() { /* 구현 */ }
    private void AttackPlayer() { /* 구현 */ }
    private void PatrolBehavior() { /* 구현 */ }
}
```

#### 고급 구조 - State 패턴 기반 FSM

```csharp
// IState 인터페이스
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

// State 기본 클래스
public abstract class BaseState : IState
{
    protected EnemyController enemy;

    public BaseState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}

// 구체적인 상태 클래스
public class IdleState : BaseState
{
    private float idleTimer;
    private float idleDuration = 3f;

    public IdleState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        idleTimer = 0f;
        enemy.Animator.SetBool("IsIdle", true);
    }

    public override void Execute()
    {
        idleTimer += Time.deltaTime;

        // 플레이어 감지 시 추격
        if (enemy.CanSeePlayer())
        {
            enemy.StateMachine.ChangeState(new ChaseState(enemy));
        }
        // 일정 시간 후 순찰
        else if (idleTimer >= idleDuration)
        {
            enemy.StateMachine.ChangeState(new PatrolState(enemy));
        }
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsIdle", false);
    }
}

public class ChaseState : BaseState
{
    public ChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsRunning", true);
        enemy.Agent.speed = enemy.ChaseSpeed;
    }

    public override void Execute()
    {
        enemy.Agent.SetDestination(enemy.Player.position);

        float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        if (distance <= enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(new AttackState(enemy));
        }
        else if (distance > enemy.DetectionRange)
        {
            enemy.StateMachine.ChangeState(new IdleState(enemy));
        }
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsRunning", false);
    }
}

// StateMachine 클래스
public class StateMachine
{
    private IState currentState;

    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Execute();
    }
}

// EnemyController
public class EnemyController : MonoBehaviour
{
    public StateMachine StateMachine { get; private set; }
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Transform Player { get; private set; }

    public float DetectionRange = 15f;
    public float AttackRange = 2f;
    public float ChaseSpeed = 5f;

    private void Awake()
    {
        StateMachine = new StateMachine();
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        StateMachine.ChangeState(new IdleState(this));
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, Player.position);
        return distance <= DetectionRange;
    }
}
```

### 2.4 장단점

| 장점 | 단점 |
|------|------|
| 직관적이고 이해하기 쉬움 | 상태가 많아지면 복잡도 급증 |
| 디버깅이 용이함 | 상태 전이 조건 관리 어려움 |
| 구현이 빠름 | 재사용성이 낮음 |
| 작은 프로젝트에 적합 | 동시 행동 표현 어려움 |

---

## 3. Behavior Tree

### 3.1 개념 설명

Behavior Tree는 **트리 구조**로 AI 행동을 구성하는 방법입니다. 루트에서 시작하여 각 노드를 순회하며 행동을 결정합니다.

#### 핵심 노드 타입

| 노드 타입 | 설명 | 반환값 |
|----------|------|--------|
| **Selector (?)** | 자식 중 하나가 성공할 때까지 실행 (OR 논리) | 첫 성공/모두 실패 |
| **Sequence (→)** | 모든 자식이 성공해야 함 (AND 논리) | 첫 실패/모두 성공 |
| **Action** | 실제 행동 수행 (리프 노드) | 성공/실패/실행중 |
| **Condition** | 조건 검사 (리프 노드) | 참/거짓 |
| **Decorator** | 자식 노드 결과 수정 | 수정된 결과 |

### 3.2 트리 구조 다이어그램

```
                            [Root]
                               │
                               ▼
                    ┌─────────────────────┐
                    │    ? Selector       │
                    │   (우선순위 선택)    │
                    └─────────────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
   ┌─────────────┐     ┌─────────────┐      ┌─────────────┐
   │ → Sequence  │     │ → Sequence  │      │   Action    │
   │  (공격)     │     │   (추격)    │      │   (순찰)    │
   └─────────────┘     └─────────────┘      └─────────────┘
          │                    │
     ┌────┴────┐          ┌────┴────┐
     │         │          │         │
     ▼         ▼          ▼         ▼
┌─────────┐ ┌───────┐ ┌─────────┐ ┌───────┐
│Condition│ │Action │ │Condition│ │Action │
│공격범위?│ │ 공격  │ │플레이어 │ │ 추격  │
└─────────┘ └───────┘ │ 감지?   │ └───────┘
                      └─────────┘

실행 흐름:
1. Selector가 첫 번째 자식(공격 Sequence) 시도
2. 공격 범위 조건 확인 → 거짓이면 다음으로
3. Selector가 두 번째 자식(추격 Sequence) 시도
4. 플레이어 감지 조건 확인 → 참이면 추격 실행
5. 모든 조건 실패 시 → 순찰 Action 실행
```

### 3.3 Unity 구현 코드 예시

#### 노드 기본 구조

```csharp
using System.Collections.Generic;
using UnityEngine;

// 노드 실행 결과
public enum NodeState
{
    Running,  // 실행 중
    Success,  // 성공
    Failure   // 실패
}

// 노드 기본 클래스
public abstract class Node
{
    protected NodeState state;
    public Node parent;
    protected List<Node> children = new List<Node>();

    private Dictionary<string, object> dataContext = new Dictionary<string, object>();

    public Node()
    {
        parent = null;
    }

    public Node(List<Node> children)
    {
        foreach (Node child in children)
            Attach(child);
    }

    private void Attach(Node node)
    {
        node.parent = this;
        children.Add(node);
    }

    public abstract NodeState Evaluate();

    // 데이터 공유를 위한 메서드
    public void SetData(string key, object value)
    {
        dataContext[key] = value;
    }

    public object GetData(string key)
    {
        if (dataContext.TryGetValue(key, out object value))
            return value;

        Node node = parent;
        while (node != null)
        {
            if (node.dataContext.TryGetValue(key, out value))
                return value;
            node = node.parent;
        }
        return null;
    }

    public bool ClearData(string key)
    {
        if (dataContext.ContainsKey(key))
        {
            dataContext.Remove(key);
            return true;
        }

        Node node = parent;
        while (node != null)
        {
            if (node.ClearData(key))
                return true;
            node = node.parent;
        }
        return false;
    }
}
```

#### Composite 노드 (Selector, Sequence)

```csharp
// Selector: 자식 중 하나가 성공하면 성공
public class Selector : Node
{
    public Selector() : base() { }
    public Selector(List<Node> children) : base(children) { }

    public override NodeState Evaluate()
    {
        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    continue;
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
            }
        }

        state = NodeState.Failure;
        return state;
    }
}

// Sequence: 모든 자식이 성공해야 성공
public class Sequence : Node
{
    public Sequence() : base() { }
    public Sequence(List<Node> children) : base(children) { }

    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;

        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
                case NodeState.Running:
                    anyChildRunning = true;
                    continue;
            }
        }

        state = anyChildRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}
```

#### 조건 및 액션 노드

```csharp
// 조건 노드: 플레이어 감지
public class CheckPlayerInRange : Node
{
    private Transform transform;
    private float range;

    public CheckPlayerInRange(Transform transform, float range)
    {
        this.transform = transform;
        this.range = range;
    }

    public override NodeState Evaluate()
    {
        object target = GetData("target");
        if (target == null)
        {
            Collider[] colliders = Physics.OverlapSphere(
                transform.position, range, LayerMask.GetMask("Player"));

            if (colliders.Length > 0)
            {
                parent.parent.SetData("target", colliders[0].transform);
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }

        state = NodeState.Success;
        return state;
    }
}

// 조건 노드: 공격 범위 확인
public class CheckAttackRange : Node
{
    private Transform transform;
    private float attackRange;

    public CheckAttackRange(Transform transform, float attackRange)
    {
        this.transform = transform;
        this.attackRange = attackRange;
    }

    public override NodeState Evaluate()
    {
        object target = GetData("target");
        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        Transform targetTransform = (Transform)target;
        float distance = Vector3.Distance(transform.position, targetTransform.position);

        state = distance <= attackRange ? NodeState.Success : NodeState.Failure;
        return state;
    }
}

// 액션 노드: 추격
public class TaskChase : Node
{
    private Transform transform;
    private NavMeshAgent agent;

    public TaskChase(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        agent.SetDestination(target.position);

        state = NodeState.Running;
        return state;
    }
}

// 액션 노드: 공격
public class TaskAttack : Node
{
    private Transform transform;
    private Animator animator;
    private float attackCooldown = 1f;
    private float lastAttackTime;

    public TaskAttack(Transform transform, Animator animator)
    {
        this.transform = transform;
        this.animator = animator;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        // 타겟 방향 바라보기
        Vector3 direction = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // 쿨다운 체크
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            // 데미지 처리
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(10);
        }

        state = NodeState.Running;
        return state;
    }
}

// 액션 노드: 순찰
public class TaskPatrol : Node
{
    private Transform transform;
    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float waitTime = 2f;
    private float waitCounter = 0f;
    private bool waiting = false;

    public TaskPatrol(Transform transform, NavMeshAgent agent, Transform[] waypoints)
    {
        this.transform = transform;
        this.agent = agent;
        this.waypoints = waypoints;
    }

    public override NodeState Evaluate()
    {
        if (waypoints.Length == 0)
        {
            state = NodeState.Failure;
            return state;
        }

        if (waiting)
        {
            waitCounter += Time.deltaTime;
            if (waitCounter >= waitTime)
            {
                waiting = false;
            }
        }
        else
        {
            Transform wp = waypoints[currentWaypointIndex];
            float distance = Vector3.Distance(transform.position, wp.position);

            if (distance < 0.5f)
            {
                waiting = true;
                waitCounter = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else
            {
                agent.SetDestination(wp.position);
            }
        }

        state = NodeState.Running;
        return state;
    }
}
```

#### Behavior Tree 조립

```csharp
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBT : MonoBehaviour
{
    private Node root;
    private NavMeshAgent agent;
    private Animator animator;

    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Transform[] waypoints;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ConstructBehaviorTree();
    }

    private void ConstructBehaviorTree()
    {
        // 공격 시퀀스
        Sequence attackSequence = new Sequence(new List<Node>
        {
            new CheckAttackRange(transform, attackRange),
            new TaskAttack(transform, animator)
        });

        // 추격 시퀀스
        Sequence chaseSequence = new Sequence(new List<Node>
        {
            new CheckPlayerInRange(transform, detectionRange),
            new TaskChase(transform, agent)
        });

        // 순찰
        TaskPatrol patrol = new TaskPatrol(transform, agent, waypoints);

        // 루트 Selector
        root = new Selector(new List<Node>
        {
            attackSequence,
            chaseSequence,
            patrol
        });
    }

    private void Update()
    {
        root.Evaluate();
    }
}
```

### 3.4 장단점

| 장점 | 단점 |
|------|------|
| 시각적으로 이해하기 쉬움 | 초기 설정이 복잡함 |
| 모듈화 및 재사용성 높음 | 러닝 커브가 있음 |
| 확장이 용이함 | 실시간 디버깅이 어려울 수 있음 |
| 복잡한 행동 패턴 표현 가능 | 메모리 사용량 증가 |
| AAA 게임에서 검증된 방식 | 단순한 AI에는 과도할 수 있음 |

---

## 4. 기타 방법론

### 4.1 GOAP (Goal-Oriented Action Planning)

#### 개념

GOAP는 **목표(Goal)**를 설정하고, 해당 목표를 달성하기 위한 **행동(Action) 계획**을 자동으로 수립하는 AI 시스템입니다.

#### 다이어그램

```
목표 설정 및 계획 수립 과정:

┌─────────────────────────────────────────────────────────────┐
│                         World State                          │
│  { hasWeapon: true, enemyVisible: true, enemyDead: false }  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                          Goals                               │
│  1. KillEnemy (priority: 10) → { enemyDead: true }          │
│  2. GetWeapon (priority: 5)  → { hasWeapon: true }          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Available Actions                          │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │   Attack     │  │  MoveToEnemy │  │   PickUpWeapon   │   │
│  │ Pre: enemy   │  │ Pre: enemy   │  │ Pre: weaponNear  │   │
│  │      Visible │  │      Visible │  │ Post: hasWeapon  │   │
│  │ Post: enemy  │  │ Post: enemy  │  │                  │   │
│  │       Dead   │  │       Near   │  │                  │   │
│  │ Cost: 5      │  │ Cost: 2      │  │ Cost: 3          │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     A* Planner                               │
│         최적 경로 탐색 (비용 최소화)                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Generated Plan                            │
│       MoveToEnemy (2) → Attack (5) = Total Cost: 7          │
└─────────────────────────────────────────────────────────────┘
```

#### 코드 예시

```csharp
using System.Collections.Generic;
using UnityEngine;

// 세계 상태
public class WorldState
{
    public Dictionary<string, bool> states = new Dictionary<string, bool>();

    public WorldState Clone()
    {
        WorldState clone = new WorldState();
        foreach (var kvp in states)
            clone.states[kvp.Key] = kvp.Value;
        return clone;
    }
}

// 액션 정의
public abstract class GOAPAction
{
    public string actionName;
    public float cost = 1f;

    // 전제 조건
    public Dictionary<string, bool> preconditions = new Dictionary<string, bool>();
    // 효과
    public Dictionary<string, bool> effects = new Dictionary<string, bool>();

    public abstract bool IsAchievable();
    public abstract bool Perform(GameObject agent);

    public bool ArePreconditionsMet(WorldState state)
    {
        foreach (var precondition in preconditions)
        {
            if (!state.states.ContainsKey(precondition.Key) ||
                state.states[precondition.Key] != precondition.Value)
                return false;
        }
        return true;
    }
}

// 구체적인 액션
public class MoveToEnemyAction : GOAPAction
{
    public MoveToEnemyAction()
    {
        actionName = "MoveToEnemy";
        cost = 2f;

        preconditions.Add("enemyVisible", true);
        effects.Add("enemyInRange", true);
    }

    public override bool IsAchievable() => true;

    public override bool Perform(GameObject agent)
    {
        // 이동 로직
        return true;
    }
}

public class AttackAction : GOAPAction
{
    public AttackAction()
    {
        actionName = "Attack";
        cost = 5f;

        preconditions.Add("enemyInRange", true);
        preconditions.Add("hasWeapon", true);
        effects.Add("enemyDead", true);
    }

    public override bool IsAchievable() => true;

    public override bool Perform(GameObject agent)
    {
        // 공격 로직
        return true;
    }
}

// 목표 정의
public class GOAPGoal
{
    public string goalName;
    public int priority;
    public Dictionary<string, bool> desiredState = new Dictionary<string, bool>();

    public bool IsAchieved(WorldState worldState)
    {
        foreach (var state in desiredState)
        {
            if (!worldState.states.ContainsKey(state.Key) ||
                worldState.states[state.Key] != state.Value)
                return false;
        }
        return true;
    }
}

// 플래너
public class GOAPPlanner
{
    public Queue<GOAPAction> Plan(
        WorldState worldState,
        List<GOAPAction> availableActions,
        GOAPGoal goal)
    {
        List<PlannerNode> leaves = new List<PlannerNode>();

        PlannerNode start = new PlannerNode(null, 0, worldState, null);

        bool success = BuildGraph(start, leaves, availableActions, goal);

        if (!success)
            return null;

        // 가장 저렴한 계획 선택
        PlannerNode cheapest = null;
        foreach (PlannerNode leaf in leaves)
        {
            if (cheapest == null || leaf.runningCost < cheapest.runningCost)
                cheapest = leaf;
        }

        // 액션 추출
        List<GOAPAction> result = new List<GOAPAction>();
        PlannerNode n = cheapest;
        while (n != null)
        {
            if (n.action != null)
                result.Insert(0, n.action);
            n = n.parent;
        }

        return new Queue<GOAPAction>(result);
    }

    private bool BuildGraph(
        PlannerNode parent,
        List<PlannerNode> leaves,
        List<GOAPAction> availableActions,
        GOAPGoal goal)
    {
        bool foundPath = false;

        foreach (GOAPAction action in availableActions)
        {
            if (action.ArePreconditionsMet(parent.state))
            {
                WorldState newState = parent.state.Clone();
                foreach (var effect in action.effects)
                    newState.states[effect.Key] = effect.Value;

                PlannerNode node = new PlannerNode(
                    parent,
                    parent.runningCost + action.cost,
                    newState,
                    action
                );

                if (goal.IsAchieved(newState))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    List<GOAPAction> remaining = new List<GOAPAction>(availableActions);
                    remaining.Remove(action);

                    if (BuildGraph(node, leaves, remaining, goal))
                        foundPath = true;
                }
            }
        }

        return foundPath;
    }
}

public class PlannerNode
{
    public PlannerNode parent;
    public float runningCost;
    public WorldState state;
    public GOAPAction action;

    public PlannerNode(PlannerNode parent, float cost, WorldState state, GOAPAction action)
    {
        this.parent = parent;
        this.runningCost = cost;
        this.state = state;
        this.action = action;
    }
}
```

### 4.2 Utility AI

#### 개념

Utility AI는 각 행동의 **유용성(Utility) 점수**를 계산하여 가장 높은 점수의 행동을 선택합니다. 부드럽고 자연스러운 의사결정이 가능합니다.

#### 다이어그램

```
                        Utility AI 의사결정 과정

Input Data (현재 상태):
┌────────────────────────────────────────────────────────────┐
│  Health: 30%  │  Ammo: 80%  │  EnemyDist: 5m  │  Allies: 2 │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│                    Considerations                           │
│                                                             │
│  Action: Attack                                             │
│  ├─ Health Factor:     ████░░░░░░ 0.3 (낮음 → 낮은 점수)    │
│  ├─ Ammo Factor:       ████████░░ 0.8 (충분)               │
│  └─ Distance Factor:   ██████░░░░ 0.6 (적당)               │
│                        ─────────────                        │
│                        Score: 0.3 × 0.8 × 0.6 = 0.144      │
│                                                             │
│  Action: Flee                                               │
│  ├─ Health Factor:     ████████░░ 0.8 (낮음 → 높은 점수)    │
│  ├─ Enemy Count:       ██████░░░░ 0.6 (적당)               │
│  └─ Escape Route:      ████████░░ 0.9 (있음)               │
│                        ─────────────                        │
│                        Score: 0.8 × 0.6 × 0.9 = 0.432      │
│                                                             │
│  Action: Heal                                               │
│  ├─ Health Factor:     █████████░ 0.9 (낮음 → 매우 높음)    │
│  ├─ Has Medkit:        ██████████ 1.0 (있음)               │
│  └─ Safety Factor:     ██████░░░░ 0.6 (적당)               │
│                        ─────────────                        │
│                        Score: 0.9 × 1.0 × 0.6 = 0.540  ◄── 최고!
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │ Selected: Heal  │
                    └─────────────────┘
```

#### 코드 예시

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

// Consideration: 개별 요소 평가
public abstract class Consideration
{
    public string Name { get; protected set; }
    public AnimationCurve ResponseCurve { get; set; }

    public abstract float Score(AIContext context);

    protected float EvaluateCurve(float input)
    {
        return ResponseCurve?.Evaluate(input) ?? input;
    }
}

// 체력 기반 고려사항
public class HealthConsideration : Consideration
{
    private bool inverseScore; // true면 체력이 낮을수록 높은 점수

    public HealthConsideration(bool inverse = false)
    {
        Name = "Health";
        inverseScore = inverse;

        // 기본 선형 커브
        ResponseCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    public override float Score(AIContext context)
    {
        float normalizedHealth = context.Health / context.MaxHealth;
        float score = EvaluateCurve(normalizedHealth);

        return inverseScore ? 1f - score : score;
    }
}

// 거리 기반 고려사항
public class DistanceConsideration : Consideration
{
    private float maxDistance;

    public DistanceConsideration(float maxDist = 20f)
    {
        Name = "Distance";
        maxDistance = maxDist;
    }

    public override float Score(AIContext context)
    {
        float distance = Vector3.Distance(
            context.Agent.position,
            context.Target.position
        );
        float normalized = Mathf.Clamp01(distance / maxDistance);

        return EvaluateCurve(1f - normalized); // 가까울수록 높은 점수
    }
}

// Action: 실행 가능한 행동
public class UtilityAction
{
    public string Name { get; private set; }
    public List<Consideration> Considerations { get; private set; }
    public Action<AIContext> Execute { get; private set; }

    public UtilityAction(string name, Action<AIContext> execute)
    {
        Name = name;
        Execute = execute;
        Considerations = new List<Consideration>();
    }

    public void AddConsideration(Consideration consideration)
    {
        Considerations.Add(consideration);
    }

    public float CalculateUtility(AIContext context)
    {
        if (Considerations.Count == 0)
            return 0f;

        float finalScore = 1f;

        foreach (var consideration in Considerations)
        {
            float score = consideration.Score(context);

            // 보정 인수 적용 (낮은 개수 보정)
            float modificationFactor = 1f - (1f / Considerations.Count);
            float makeupValue = (1f - score) * modificationFactor;
            score += makeupValue * score;

            finalScore *= score;

            // 0이면 조기 종료
            if (finalScore <= 0f)
                return 0f;
        }

        return finalScore;
    }
}

// AI Context: 판단에 필요한 정보
public class AIContext
{
    public Transform Agent { get; set; }
    public Transform Target { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float Ammo { get; set; }
    public float MaxAmmo { get; set; }
    public bool HasMedkit { get; set; }
    public bool InCover { get; set; }
}

// Utility AI 브레인
public class UtilityAIBrain : MonoBehaviour
{
    private List<UtilityAction> actions = new List<UtilityAction>();
    private AIContext context;
    private UtilityAction currentAction;

    private void Start()
    {
        SetupActions();
        context = new AIContext
        {
            Agent = transform,
            MaxHealth = 100f,
            MaxAmmo = 30f
        };
    }

    private void SetupActions()
    {
        // 공격 액션
        var attackAction = new UtilityAction("Attack", ctx =>
        {
            Debug.Log("Attacking!");
        });
        attackAction.AddConsideration(new HealthConsideration(false));
        attackAction.AddConsideration(new DistanceConsideration(15f));
        actions.Add(attackAction);

        // 도주 액션
        var fleeAction = new UtilityAction("Flee", ctx =>
        {
            Debug.Log("Fleeing!");
        });
        fleeAction.AddConsideration(new HealthConsideration(true)); // 체력 낮으면 높은 점수
        actions.Add(fleeAction);

        // 치료 액션
        var healAction = new UtilityAction("Heal", ctx =>
        {
            Debug.Log("Healing!");
        });
        healAction.AddConsideration(new HealthConsideration(true));
        actions.Add(healAction);
    }

    private void Update()
    {
        UpdateContext();
        SelectAndExecuteAction();
    }

    private void UpdateContext()
    {
        context.Health = GetComponent<Health>()?.CurrentHealth ?? 100f;
        context.Target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void SelectAndExecuteAction()
    {
        UtilityAction bestAction = null;
        float bestScore = 0f;

        foreach (var action in actions)
        {
            float score = action.CalculateUtility(context);
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != null && bestAction != currentAction)
        {
            currentAction = bestAction;
            Debug.Log($"Switching to: {bestAction.Name} (Score: {bestScore:F2})");
        }

        currentAction?.Execute(context);
    }
}
```

---

## 5. Unity 특화 기능

### 5.1 NavMesh / NavMeshAgent

NavMesh는 Unity의 내장 네비게이션 시스템으로, 3D 환경에서 AI의 경로 탐색을 처리합니다.

#### 기본 설정

```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 120f;

    [Header("회피 설정")]
    [SerializeField] private int avoidancePriority = 50;
    [SerializeField] private float avoidanceRadius = 0.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
    }

    private void ConfigureAgent()
    {
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;
        agent.avoidancePriority = avoidancePriority;
        agent.radius = avoidanceRadius;
    }

    public void MoveTo(Vector3 destination)
    {
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void SetRunning(bool running)
    {
        agent.speed = running ? runSpeed : walkSpeed;
    }

    public bool HasReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    public void Resume()
    {
        agent.isStopped = false;
    }
}
```

#### NavMeshAgent 속성 설명

| 속성 | 설명 | 권장값 |
|------|------|--------|
| `speed` | 이동 속도 | 3.5 ~ 6 |
| `angularSpeed` | 회전 속도 | 120 ~ 360 |
| `acceleration` | 가속도 | 8 |
| `stoppingDistance` | 정지 거리 | 0.5 ~ 2 |
| `autoBraking` | 자동 감속 | true |
| `avoidancePriority` | 회피 우선순위 (0-99) | 50 |

### 5.2 Animation 연동

```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    // Animator 파라미터 해시
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField] private float animationDampTime = 0.1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        UpdateLocomotion();
    }

    private void UpdateLocomotion()
    {
        // NavMeshAgent 속도를 애니메이션에 반영
        float speed = agent.velocity.magnitude / agent.speed;

        animator.SetFloat(SpeedHash, speed, animationDampTime, Time.deltaTime);
        animator.SetBool(IsMovingHash, speed > 0.1f);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(AttackHash);
    }

    public void PlayHit()
    {
        animator.SetTrigger(HitHash);
    }

    public void PlayDeath()
    {
        animator.SetTrigger(DieHash);
        // 사망 후 NavMeshAgent 비활성화
        agent.enabled = false;
    }

    // Animation Event에서 호출
    public void OnAttackHit()
    {
        // 공격 판정 로직
        Debug.Log("Attack hit frame!");
    }

    public void OnAttackEnd()
    {
        // 공격 종료 처리
        Debug.Log("Attack ended!");
    }
}
```

### 5.3 Physics2D 감지

2D 게임을 위한 적 감지 시스템입니다.

```csharp
using UnityEngine;

public class Enemy2DDetection : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("디버그")]
    [SerializeField] private bool showGizmos = true;

    private Transform detectedPlayer;

    public Transform DetectedPlayer => detectedPlayer;
    public bool HasDetectedPlayer => detectedPlayer != null;

    private void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        detectedPlayer = null;

        // 원형 범위 내 플레이어 검색
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectionRadius,
            playerLayer
        );

        if (playerCollider == null)
            return;

        Transform player = playerCollider.transform;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // 시야각 체크
        float angle = Vector2.Angle(transform.right, directionToPlayer);
        if (angle > fieldOfView / 2f)
            return;

        // 장애물 체크 (Raycast)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        if (hit.collider == null)
        {
            detectedPlayer = player;
        }
    }

    // OverlapBox를 활용한 근접 감지
    public bool IsPlayerInMeleeRange(float range)
    {
        Collider2D hit = Physics2D.OverlapBox(
            transform.position + transform.right * (range / 2f),
            new Vector2(range, 1f),
            0f,
            playerLayer
        );
        return hit != null;
    }

    // CircleCast를 활용한 돌진 경로 체크
    public bool CanChargeToPlayer(float chargeDistance)
    {
        if (detectedPlayer == null)
            return false;

        Vector2 direction = (detectedPlayer.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            0.5f,
            direction,
            chargeDistance,
            obstacleLayer
        );

        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 시야각
        Vector3 rightDir = Quaternion.Euler(0, 0, -fieldOfView / 2f) * transform.right;
        Vector3 leftDir = Quaternion.Euler(0, 0, fieldOfView / 2f) * transform.right;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRadius);

        // 감지된 플레이어
        if (detectedPlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, detectedPlayer.position);
        }
    }
}
```

---

## 6. WarriorQuest 적용 예시

### 6.1 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Enemy/
│   │   ├── Base/
│   │   │   ├── Enemy.cs              # 추상 기본 클래스
│   │   │   ├── EnemyStats.cs         # 스탯 ScriptableObject
│   │   │   └── EnemyStateMachine.cs  # FSM 관리자
│   │   ├── States/
│   │   │   ├── EnemyIdleState.cs
│   │   │   ├── EnemyPatrolState.cs
│   │   │   ├── EnemyChaseState.cs
│   │   │   ├── EnemyAttackState.cs
│   │   │   └── EnemyDeadState.cs
│   │   └── Types/
│   │       ├── Slime.cs
│   │       ├── Goblin.cs
│   │       └── Skeleton.cs
│   └── Interfaces/
│       ├── IDamageable.cs
│       └── IAttacker.cs
```

### 6.2 Enemy 추상 클래스 설계

```csharp
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    void Die();
    bool IsDead { get; }
}

public interface IAttacker
{
    void Attack();
    float AttackDamage { get; }
    float AttackRange { get; }
}

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Enemy : MonoBehaviour, IDamageable, IAttacker
{
    [Header("기본 스탯")]
    [SerializeField] protected EnemyStats stats;

    [Header("감지")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected LayerMask playerLayer;

    // 컴포넌트 참조
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected EnemyStateMachine stateMachine;

    // 상태
    protected float currentHealth;
    protected Transform player;
    protected bool facingRight = true;

    // 프로퍼티
    public bool IsDead { get; protected set; }
    public float AttackDamage => stats.attackDamage;
    public float AttackRange => stats.attackRange;
    public Transform Player => player;
    public float DetectionRange => detectionRange;
    public EnemyStats Stats => stats;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stateMachine = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        currentHealth = stats.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        InitializeStateMachine();
    }

    protected abstract void InitializeStateMachine();

    protected virtual void Update()
    {
        if (!IsDead)
        {
            stateMachine.CurrentState?.Execute();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!IsDead)
        {
            stateMachine.CurrentState?.PhysicsUpdate();
        }
    }

    // IDamageable 구현
    public virtual void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        // 넉백 효과
        ApplyKnockback();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        IsDead = true;
        animator.SetTrigger("Die");

        // 물리 비활성화
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        // 상태 전환
        stateMachine.ChangeState(new EnemyDeadState(this, stateMachine));

        // 일정 시간 후 제거
        Destroy(gameObject, 2f);
    }

    // IAttacker 구현
    public abstract void Attack();

    // 유틸리티 메서드
    public bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= range;
    }

    public void FacePlayer()
    {
        if (player == null) return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != facingRight)
        {
            Flip();
        }
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }

    public void Move(Vector2 direction, float speed)
    {
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        if (direction.x > 0 && !facingRight)
            Flip();
        else if (direction.x < 0 && facingRight)
            Flip();
    }

    protected virtual void ApplyKnockback()
    {
        if (player == null) return;

        Vector2 knockbackDir = (transform.position - player.position).normalized;
        rb.AddForce(knockbackDir * stats.knockbackForce, ForceMode2D.Impulse);
    }
}
```

### 6.3 EnemyStats ScriptableObject

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "WarriorQuest/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("체력")]
    public float maxHealth = 100f;

    [Header("이동")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("전투")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("물리")]
    public float knockbackForce = 5f;

    [Header("보상")]
    public int experienceReward = 50;
    public int goldReward = 10;
}
```

### 6.4 StateMachine 구현

```csharp
public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}

public abstract class EnemyState
{
    protected Enemy enemy;
    protected EnemyStateMachine stateMachine;
    protected float stateTimer;

    public EnemyState(Enemy enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        stateTimer = 0f;
    }

    public virtual void Execute()
    {
        stateTimer += Time.deltaTime;
    }

    public virtual void PhysicsUpdate() { }

    public virtual void Exit() { }
}
```

### 6.5 구체적인 상태 클래스

```csharp
// Idle 상태
public class EnemyIdleState : EnemyState
{
    private float idleDuration;

    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        enemy.GetComponent<Animator>().SetBool("IsMoving", false);
        idleDuration = Random.Range(1f, 3f);
    }

    public override void Execute()
    {
        base.Execute();

        if (enemy.IsPlayerInRange(enemy.DetectionRange))
        {
            stateMachine.ChangeState(new EnemyChaseState(enemy, stateMachine));
        }
        else if (stateTimer >= idleDuration)
        {
            stateMachine.ChangeState(new EnemyPatrolState(enemy, stateMachine));
        }
    }
}

// Chase 상태
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        enemy.GetComponent<Animator>().SetBool("IsMoving", true);
    }

    public override void Execute()
    {
        base.Execute();

        if (enemy.Player == null)
        {
            stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
            return;
        }

        enemy.FacePlayer();

        if (enemy.IsPlayerInRange(enemy.AttackRange))
        {
            stateMachine.ChangeState(new EnemyAttackState(enemy, stateMachine));
        }
        else if (!enemy.IsPlayerInRange(enemy.DetectionRange * 1.5f))
        {
            stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
        }
    }

    public override void PhysicsUpdate()
    {
        if (enemy.Player == null) return;

        Vector2 direction = (enemy.Player.position - enemy.transform.position).normalized;
        enemy.Move(direction, enemy.Stats.chaseSpeed);
    }

    public override void Exit()
    {
        enemy.Move(Vector2.zero, 0);
    }
}

// Attack 상태
public class EnemyAttackState : EnemyState
{
    private bool hasAttacked;

    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        hasAttacked = false;
        enemy.Move(Vector2.zero, 0);
        enemy.FacePlayer();
    }

    public override void Execute()
    {
        base.Execute();

        if (!hasAttacked)
        {
            enemy.Attack();
            hasAttacked = true;
        }

        if (stateTimer >= enemy.Stats.attackCooldown)
        {
            if (enemy.IsPlayerInRange(enemy.AttackRange))
            {
                stateMachine.ChangeState(new EnemyAttackState(enemy, stateMachine));
            }
            else if (enemy.IsPlayerInRange(enemy.DetectionRange))
            {
                stateMachine.ChangeState(new EnemyChaseState(enemy, stateMachine));
            }
            else
            {
                stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
            }
        }
    }
}

// Dead 상태
public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // 사망 처리는 Enemy.Die()에서 이미 수행됨
    }

    public override void Execute()
    {
        // 아무 동작 안함
    }
}
```

### 6.6 구체적인 적 클래스

```csharp
// Slime 클래스
public class Slime : Enemy
{
    [Header("Slime 전용")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 2f;

    private float lastJumpTime;

    protected override void InitializeStateMachine()
    {
        stateMachine.Initialize(new EnemyIdleState(this, stateMachine));
    }

    public override void Attack()
    {
        animator.SetTrigger("Attack");

        // 점프 공격
        if (Time.time - lastJumpTime >= jumpCooldown)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.AddForce(new Vector2(direction.x * jumpForce, jumpForce), ForceMode2D.Impulse);
            lastJumpTime = Time.time;
        }
    }

    // Animation Event
    public void OnAttackHitbox()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            AttackRange,
            playerLayer
        );

        if (hit != null)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(AttackDamage);
        }
    }
}

// Goblin 클래스
public class Goblin : Enemy
{
    [Header("Goblin 전용")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool isRanged = false;
    [SerializeField] private float rangedAttackRange = 6f;

    protected override void InitializeStateMachine()
    {
        stateMachine.Initialize(new EnemyPatrolState(this, stateMachine));
    }

    public override void Attack()
    {
        animator.SetTrigger("Attack");

        if (isRanged)
        {
            // 원거리 공격
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || attackPoint == null) return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            attackPoint.position,
            Quaternion.identity
        );

        Vector2 direction = (player.position - attackPoint.position).normalized;
        projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f;

        Destroy(projectile, 3f);
    }

    // Animation Event
    public void OnMeleeHit()
    {
        if (isRanged) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            0.5f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(AttackDamage);
        }
    }
}

// Skeleton 클래스
public class Skeleton : Enemy
{
    [Header("Skeleton 전용")]
    [SerializeField] private float blockChance = 0.3f;
    [SerializeField] private float counterAttackWindow = 0.5f;

    private bool isBlocking;
    private float blockTimer;

    protected override void InitializeStateMachine()
    {
        stateMachine.Initialize(new EnemyPatrolState(this, stateMachine));
    }

    public override void TakeDamage(float damage)
    {
        // 방어 확률 체크
        if (!isBlocking && Random.value < blockChance)
        {
            StartBlock();
            return;
        }

        base.TakeDamage(damage);
    }

    private void StartBlock()
    {
        isBlocking = true;
        blockTimer = 0f;
        animator.SetBool("IsBlocking", true);
    }

    protected override void Update()
    {
        base.Update();

        if (isBlocking)
        {
            blockTimer += Time.deltaTime;
            if (blockTimer >= counterAttackWindow)
            {
                isBlocking = false;
                animator.SetBool("IsBlocking", false);

                // 반격
                if (IsPlayerInRange(AttackRange * 1.5f))
                {
                    stateMachine.ChangeState(new EnemyAttackState(this, stateMachine));
                }
            }
        }
    }

    public override void Attack()
    {
        animator.SetTrigger("Attack");
    }
}
```

---

## 7. 비교 및 추천

### 7.1 방법론별 비교 표

| 항목 | FSM | Behavior Tree | GOAP | Utility AI |
|------|-----|---------------|------|------------|
| **학습 난이도** | ★☆☆☆☆ | ★★★☆☆ | ★★★★☆ | ★★★☆☆ |
| **구현 복잡도** | 낮음 | 중간 | 높음 | 중간 |
| **확장성** | 낮음 | 높음 | 높음 | 높음 |
| **재사용성** | 낮음 | 높음 | 중간 | 높음 |
| **디버깅** | 쉬움 | 중간 | 어려움 | 중간 |
| **유연성** | 낮음 | 높음 | 매우 높음 | 높음 |
| **메모리 사용** | 낮음 | 중간 | 높음 | 중간 |
| **성능** | 빠름 | 보통 | 느림 (계획 시) | 보통 |
| **적합한 AI 수** | 1~10 | 10~100 | 10~50 | 10~100 |

### 7.2 상황별 추천

```
프로젝트 규모별 추천:

┌─────────────────┬────────────────────────────────────────┐
│    프로젝트     │              추천 방법론               │
├─────────────────┼────────────────────────────────────────┤
│ 프로토타입      │ FSM (빠른 구현)                        │
│ 소규모 인디     │ FSM 또는 간단한 Behavior Tree          │
│ 중규모 프로젝트 │ Behavior Tree                          │
│ AAA급 프로젝트  │ Behavior Tree + GOAP/Utility 하이브리드│
└─────────────────┴────────────────────────────────────────┘

적 AI 유형별 추천:

┌─────────────────┬────────────────────────────────────────┐
│     AI 유형     │              추천 방법론               │
├─────────────────┼────────────────────────────────────────┤
│ 단순한 몬스터   │ FSM                                    │
│ 보스 몬스터     │ Behavior Tree                          │
│ 전략 게임 유닛  │ GOAP 또는 Utility AI                   │
│ 스텔스 게임 AI  │ Behavior Tree + 감지 시스템            │
│ 동료 AI (NPC)   │ Utility AI                             │
│ 군중 시뮬레이션 │ 간단한 FSM + Flocking                  │
└─────────────────┴────────────────────────────────────────┘
```

### 7.3 하이브리드 접근법

실제 게임에서는 여러 방법론을 조합하여 사용합니다.

```
예시: 전략 게임 유닛 AI

        ┌───────────────────┐
        │   Utility AI      │  ← 상위 레벨: 어떤 목표를 추구할지 결정
        │   (Goal Selection)│     (공격, 방어, 자원 수집 등)
        └─────────┬─────────┘
                  │
                  ▼
        ┌───────────────────┐
        │   GOAP            │  ← 중간 레벨: 목표 달성을 위한 계획 수립
        │   (Planning)      │     (이동 → 자원 채취 → 복귀)
        └─────────┬─────────┘
                  │
                  ▼
        ┌───────────────────┐
        │   Behavior Tree   │  ← 하위 레벨: 개별 행동 실행
        │   (Execution)     │     (경로 이동, 공격 모션 등)
        └───────────────────┘
```

---

## 8. 학습 경로

### 8.1 초급 (1-2주)

```
Week 1: FSM 기초
├── Day 1-2: Enum 기반 FSM 구현
│   └── 실습: Idle → Chase → Attack 3개 상태 AI
├── Day 3-4: State 패턴 학습
│   └── 실습: 인터페이스 기반 FSM 리팩토링
└── Day 5-7: Unity 연동
    └── 실습: Animator + FSM 통합

Week 2: 감지 시스템
├── Day 1-3: Physics2D 기반 감지
│   └── 실습: OverlapCircle, Raycast 활용
└── Day 4-7: 시야각 시스템
    └── 실습: FOV 기반 플레이어 감지
```

### 8.2 중급 (2-4주)

```
Week 3-4: Behavior Tree
├── Day 1-3: 노드 시스템 이해
│   └── Selector, Sequence, Decorator
├── Day 4-7: 커스텀 노드 작성
│   └── 실습: 순찰 → 감지 → 추격 → 공격 BT
├── Day 8-10: 블랙보드 패턴
│   └── 실습: 데이터 공유 시스템 구현
└── Day 11-14: 디버그 시스템
    └── 실습: BT 시각화 도구 제작

Week 5-6: 고급 이동 시스템
├── Day 1-4: NavMesh 심화
│   └── 동적 장애물, NavMeshLink
├── Day 5-7: A* 알고리즘 이해
│   └── 2D 게임용 커스텀 경로탐색
└── Day 8-14: 그룹 이동
    └── 실습: Flocking, 대형 유지
```

### 8.3 고급 (1-2개월)

```
Month 1: 고급 AI 아키텍처
├── Week 1: GOAP 구현
│   └── 액션 정의, 플래너 작성
├── Week 2: Utility AI
│   └── 고려사항 시스템, 반응 커브
├── Week 3: 하이브리드 시스템
│   └── BT + Utility 조합
└── Week 4: 최적화
    └── Job System, Burst Compiler

Month 2: 전문화
├── Week 1-2: 보스 AI 패턴
│   └── 페이즈 시스템, 패턴 스크립팅
├── Week 3: 협동 AI
│   └── 팀 전술, 역할 분담
└── Week 4: 메타 AI
    └── 난이도 조절, 플레이어 분석
```

---

## 9. 참고 리소스

### 9.1 Unity 공식 문서

| 주제 | 링크 |
|------|------|
| NavMesh | https://docs.unity3d.com/Manual/nav-NavigationSystem.html |
| Physics2D | https://docs.unity3d.com/Manual/Physics2DReference.html |
| Animator | https://docs.unity3d.com/Manual/AnimatorControllers.html |

### 9.2 추천 에셋

| 에셋 | 설명 | 가격 |
|------|------|------|
| **Behavior Designer** | 비주얼 BT 에디터 | 유료 |
| **NodeCanvas** | BT + FSM 통합 | 유료 |
| **A* Pathfinding Project** | 고급 경로탐색 | 무료/유료 |
| **RAIN AI** | 통합 AI 솔루션 | 무료 |

### 9.3 학습 자료

#### 책
- "Game AI Pro" 시리즈
- "Artificial Intelligence for Games" - Ian Millington
- "Unity AI Game Programming" - Ray Barrera

#### 온라인 강좌
- Unity Learn: AI for Beginners
- Udemy: Unity AI 관련 강좌
- YouTube: Sebastian Lague AI 시리즈

#### GDC 발표
- "Building a Better Centaur: AI at Massive Scale"
- "The AI of Halo 2"
- "Creating the AI for Horizon Zero Dawn"

### 9.4 오픈소스 프로젝트

| 프로젝트 | 설명 | 링크 |
|----------|------|------|
| Fluid Behavior Tree | 경량 BT | GitHub |
| UnityFSM | 간단한 FSM | GitHub |
| Unity GOAP | GOAP 구현 | GitHub |

---

## 마무리

이 가이드에서 다룬 내용을 정리하면:

1. **FSM**은 간단하고 직관적이어서 입문에 적합
2. **Behavior Tree**는 업계 표준으로, 복잡한 AI에 적합
3. **GOAP/Utility AI**는 동적이고 지능적인 행동에 적합
4. 실제 프로젝트에서는 **하이브리드 접근**이 일반적

AI 구현의 핵심은 **플레이어 경험**입니다. 기술적으로 완벽한 AI보다 플레이어가 "똑똑하다"고 느끼는 AI가 더 좋은 AI입니다.

---

*이 문서는 WarriorQuest 프로젝트를 위해 작성되었습니다.*
