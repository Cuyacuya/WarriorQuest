# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

WarriorQuest는 Unity 6 기반의 2D 액션/RPG 게임입니다. URP(Universal Render Pipeline)를 사용하며, Unity의 새로운 Input System으로 플레이어 입력을 처리합니다. 타일맵 기반 레벨 디자인을 채택하고 있습니다.

## 주요 패키지

- **Unity Input System 1.17.0** - `InputSystem_Actions.inputactions`를 통한 플레이어 입력 처리
- **2D Animation/Tilemap/Sprite 패키지** - 2D 그래픽 및 레벨 디자인
- **Universal Render Pipeline 17.3.0** - 그래픽 렌더링

## 아키텍처

### 캐릭터 시스템 (`Assets/02_Scripts/Character/`)

추상 베이스 클래스 패턴을 사용합니다:

- **`Player` (추상 클래스)** - 모든 플레이어 캐릭터의 기본 클래스 (`WarriorQuest.Characte.Player` 네임스페이스)
  - 필수 컴포넌트: `Rigidbody2D`, `Animator`, `SpriteRenderer`, `InputHandler`
  - 담당 기능: 이동, 체력, 피해, 사망, 애니메이션 트리거
  - 서브클래스에서 구현해야 할 추상 메서드: `Attack()`
  - 성능을 위해 애니메이터 해시 캐싱 사용 (`hashIsMoving`, `hashAttack`, `hashHit`)

- **`Warrior`** - 방어력 스탯과 피해 감소 기능을 가진 구체 플레이어 클래스
  - `Awake()`에서 기본 스탯을 오버라이드한 후 `base.Awake()` 호출

- **`IDamageable` 인터페이스** - 피해 처리 계약 (`WarriorQuest.Character.Interface` 네임스페이스)

### 입력 시스템 (`Assets/02_Scripts/InputSystem/`)

- **`InputHandler`** - Unity Input System을 래핑하는 MonoBehaviour
  - `InputSystem_Actions` (`.inputactions` 에셋에서 자동 생성)를 래핑
  - C# 이벤트 노출: `OnMoveAction`, `OnAttackAction`, `OnInteractAction`
  - Player 클래스가 `OnEnable`/`OnDisable`에서 구독

- **입력 액션**: Move (WASD/방향키), Attack, Interact

### 에디터 확장 (`Assets/02_Scripts/Editor/`)

- **`PlayerEditor`** - Player 파생 클래스용 커스텀 인스펙터 (테스트 데미지 버튼 포함)

## 폴더 구조

```
Assets/
├── 01_Scenes/     - Unity 씬 파일
├── 02_Scripts/    - C# 스크립트
│   ├── Character/ - 플레이어/적 캐릭터 클래스
│   ├── Editor/    - 커스텀 Unity 에디터
│   └── InputSystem/ - 입력 처리
├── 03_Prefabs/    - 프리팹 에셋
├── 04_Images/     - 스프라이트 및 텍스처
├── 05_Animations/ - 애니메이션 컨트롤러 및 클립
└── Settings/      - URP 및 프로젝트 설정
```

## 네임스페이스 규칙

- `WarriorQuest.Characte.Player` - 플레이어 캐릭터 클래스 (참고: "Characte" 오타 있음)
- `WarriorQuest.Character.Interface` - 인터페이스
- `WarriorQuest.InputSystem` - 입력 처리

## 개발 참고사항

- Unity 프로젝트이므로 Unity Editor에서 열어야 함 (독립 C# 솔루션 아님)
- 스크립트는 Unity 컴파일 파이프라인을 통해 컴파일됨
- 입력 액션은 `InputSystem_Actions.inputactions`에 정의되어 있으며 `InputSystem_Actions.cs`가 자동 생성됨
