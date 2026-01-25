# Player-Warrior 클래스 UML 다이어그램

## 클래스 다이어그램

```mermaid
classDiagram
    direction TB

    %% 인터페이스
    class IDamageable {
        <<interface>>
        +TakeDamage(damage: float) void
    }

    %% Unity 기본 클래스
    class MonoBehaviour {
        <<Unity>>
    }

    %% Player 추상 클래스
    class Player {
        <<abstract>>
        #maxHp: float
        #curHp: float
        #moveSpeed: float
        #attackDamage: float
        #attackCooldown: float
        #isDead: bool
        #rb: Rigidbody2D
        #anim: Animator
        #spriteRenderer: SpriteRenderer
        #inputHandler: InputHandler
        #weaponArm: Transform
        #hashIsMoving: int$
        #hashAttack: int$
        #hashHit: int$
        +MaxHp: float
        +CurHp: float
        +MoveSpeed: float
        +AttackDamge: float
        +AttackCooldown: float
        #Awake()* void
        #OnEnable() void
        #OnDisable() void
        -FlipDirection(facingRight: bool) void
        +TakeDamage(damage: float)* void
        #Die()* void
        -OnMove(ctx: Vector2) void
        -OnAttack() void
        -OnInteraction(ctx: bool) void
        #Attack()* void
    }

    %% Warrior 구체 클래스
    class Warrior {
        -defense: float
        #Awake() void
        #Attack() void
        +TakeDamage(damage: float) void
    }

    %% 의존 컴포넌트
    class InputHandler {
        <<Component>>
        +OnMoveAction: event
        +OnAttackAction: event
        +OnInteractAction: event
    }

    class Rigidbody2D {
        <<Unity Component>>
        +linearVelocity: Vector2
    }

    class Animator {
        <<Unity Component>>
        +SetBool()
        +SetTrigger()
    }

    class SpriteRenderer {
        <<Unity Component>>
        +flipX: bool
    }

    %% 상속 관계
    MonoBehaviour <|-- Player : extends
    IDamageable <|.. Player : implements
    Player <|-- Warrior : extends

    %% 의존 관계 (RequireComponent)
    Player *-- InputHandler : requires
    Player *-- Rigidbody2D : requires
    Player *-- Animator : requires
    Player *-- SpriteRenderer : requires
```

## 관계 설명

| 관계 | 설명 |
|------|------|
| `MonoBehaviour <\|-- Player` | Player는 Unity의 MonoBehaviour를 상속 |
| `IDamageable <\|.. Player` | Player는 IDamageable 인터페이스를 구현 |
| `Player <\|-- Warrior` | Warrior는 Player를 상속 |
| `Player *-- Components` | Player는 RequireComponent로 필수 컴포넌트 명시 |

## 메서드 접근 제어자 범례

| 기호 | 의미 |
|------|------|
| `+` | public |
| `#` | protected |
| `-` | private |
| `*` | virtual |
| `$` | static |

## 오버라이드 체인

```mermaid
flowchart LR
    subgraph Awake 호출 순서
        W_Awake[Warrior.Awake] --> |스탯 설정 후| P_Awake[base.Awake]
        P_Awake --> |curHp = maxHp| Init[컴포넌트 캐싱]
    end
```

```mermaid
flowchart LR
    subgraph TakeDamage 호출 순서
        W_TD[Warrior.TakeDamage] --> |방어력 계산| P_TD[base.TakeDamage]
        P_TD --> |HP 감소| Check{curHp <= 0?}
        Check --> |Yes| Die[Die]
        Check --> |No| End[종료]
    end
```

---

*작성일: 2026-01-25*
