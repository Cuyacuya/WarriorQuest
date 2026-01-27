# Goblin 몬스터 구현 가이드

## 1. 개요

고블린은 `Enemy` 추상 클래스를 상속받아 구현합니다. 기존 `Slime`과 동일한 FSM(Finite State Machine) 구조를 사용하며, 공격 방식은 **몸통박치기(돌진 공격)**입니다.

---

## 2. 사용 가능한 스프라이트

```
Assets/04_Images/0x72_DungeonTilesetII_v1.7/.../frames/
├── goblin_idle_anim_f0~f3.png  (4프레임) - 대기 애니메이션
└── goblin_run_anim_f0~f3.png   (4프레임) - 이동 애니메이션
```

---

## 3. 구현 구조

### 3.1 클래스 상속 구조

```
Enemy (추상 클래스)
├── Slime (기존)
└── Goblin (신규)
```

### 3.2 FSM 상태 (기존 재사용)

| 상태 | 클래스 | 설명 |
|------|--------|------|
| Idle | `IdleState` | 대기 상태, 플레이어 감지 시 Chase로 전환 |
| Chase | `ChaseState` | 플레이어 추적, 공격 범위 진입 시 Attack으로 전환 |
| Attack | `AttackState` | 공격 실행, 완료 후 Idle로 복귀 |

---

## 4. 필요한 작업 목록

### 4.1 Unity 에디터 작업

#### A. 스프라이트 설정
1. `goblin_idle_anim_f0~f3.png` 선택
2. Inspector에서 설정:
   - Sprite Mode: Single
   - Pixels Per Unit: 16 (타일셋 기준)
   - Filter Mode: Point (no filter)

#### B. 애니메이션 생성
1. **Goblin_Idle** 애니메이션 클립
   - `goblin_idle_anim_f0~f3` 스프라이트 사용
   - Sample Rate: 8~10

2. **Goblin_Run** 애니메이션 클립
   - `goblin_run_anim_f0~f3` 스프라이트 사용
   - Sample Rate: 10~12

#### C. 애니메이터 컨트롤러 생성
```
Assets/05_Animations/Goblin/
├── GoblinAnimator.controller
├── Goblin_Idle.anim
└── Goblin_Run.anim
```

**파라미터:**
- `IsMoving` (Bool) - 이동 여부
- `Hit` (Trigger) - 피격 시 (선택사항)

**전환 조건:**
- Idle → Run: `IsMoving == true`
- Run → Idle: `IsMoving == false`

#### D. 프리팹 생성
```
Assets/03_Prefabs/Goblin.prefab
```

**필수 컴포넌트:**
- `SpriteRenderer`
- `Rigidbody2D` (Body Type: Dynamic, Gravity Scale: 0)
- `BoxCollider2D` (Is Trigger: true)
- `Animator`
- `Goblin` (스크립트)

**설정:**
- Layer: Enemy
- Tag: Enemy (필요시)

---

### 4.2 스크립트 작업

#### A. Goblin.cs 생성

```
Assets/02_Scripts/Character/Enemy/Goblin.cs
```

**구현 내용:**
```csharp
namespace WarriorQuest.Character.Enemy
{
    public class Goblin : Enemy
    {
        // 몸통박치기 공격 설정
        [Header("고블린 공격 스텟")]
        [SerializeField] private float chargeSpeed = 8f;      // 돌진 속도
        [SerializeField] private float chargeDistance = 1.5f; // 돌진 거리
        [SerializeField] private float chargeDelay = 0.3f;    // 돌진 전 딜레이

        // 넉백 설정 (Slime과 동일)
        [Header("넉백 설정")]
        [SerializeField] private float knockbackSpeed = 12f;
        [SerializeField] private float knockbackDistance = 0.4f;

        // 공격 상태 관리
        public bool IsAttacking { get; private set; } = false;
        public float LastAttackTime { get; private set; }

        // 상태 초기화 (필수 구현)
        protected override void InitStates() { ... }

        // 몸통박치기 공격 코루틴
        public IEnumerator ChargeAttack() { ... }

        // 충돌 감지 (플레이어 데미지)
        private void OnTriggerEnter2D(Collider2D other) { ... }

        // 피격 처리 오버라이드
        public override void TakeDamage(float damage) { ... }
    }
}
```

#### B. ScriptableObject 에셋 생성

```
Assets/02_Scripts/Character/Data/GoblinData.asset
```

**권장 스탯 값:**
| 항목 | 값 | 설명 |
|------|-----|------|
| maxHp | 60 | Slime보다 낮음 |
| moveSpeed | 2.5f | Slime보다 빠름 |
| chaseDistance | 6f | 감지 범위 |
| attackDistance | 1.5f | 공격 시작 거리 |
| attackCooldown | 1.5f | 공격 쿨타임 |
| attackDamage | 15f | 공격력 |

---

## 5. Slime vs Goblin 비교

| 특성 | Slime | Goblin |
|------|-------|--------|
| 체력 | 높음 (80) | 낮음 (60) |
| 이동속도 | 느림 (1) | 빠름 (2.5) |
| 공격방식 | 대시 후 복귀 | 돌진 (복귀X) |
| 공격력 | 보통 (10) | 높음 (15) |
| 특징 | 탱커형 | 어그로형 |

---

## 6. 구현 순서 (권장)

```
1. [Unity] 스프라이트 Import 설정
2. [Unity] 애니메이션 클립 생성 (Idle, Run)
3. [Unity] 애니메이터 컨트롤러 생성
4. [Script] Goblin.cs 작성
5. [Unity] GoblinData.asset 생성 (EnemySO)
6. [Unity] Goblin 프리팹 생성 및 컴포넌트 설정
7. [Unity] 씬에 배치 및 테스트
```

---

## 7. 참고: 기존 코드 위치

- **Enemy 베이스 클래스**: `Assets/02_Scripts/Character/Enemy/Enemy.cs`
- **Slime 구현 예시**: `Assets/02_Scripts/Character/Enemy/Slime.cs`
- **FSM 상태들**: `Assets/02_Scripts/Character/Enemy/FSM/`
- **EnemySO 정의**: `Assets/02_Scripts/Character/Data/EnemySO.cs`
