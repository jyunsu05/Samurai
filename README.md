# Samurai 2D

Unity 2D 픽셀아트 사무라이 프로젝트입니다.  
이펙트 적용 방법과 AI(Cursor)를 게임 개발에 어떻게 활용하는지 학습하기 위해 제작되었습니다.

---

## 프로젝트 목표

- 2D 픽셀아트 캐릭터의 **이동 / 공격 / 애니메이션 전환** 구현
- **코루틴(Coroutine)** 기반 비동기 흐름 제어 학습
- **이펙트 매니저(싱글톤 패턴)** 를 통한 VFX 관리 구조 설계
- **이벤트(C# event)** 를 활용한 느슨한 결합 설계 학습
- AI 코딩 어시스턴트(Cursor)를 실무에 활용하는 방법 학습

---

## 개발 환경

| 항목 | 버전 |
|------|------|
| Unity | 2022.3 LTS 이상 |
| Render Pipeline | URP (Universal Render Pipeline) 2D |
| 언어 | C# |
| IDE | Cursor / Visual Studio |
| 플랫폼 | PC / Android |

---

## 프로젝트 구조

```
Assets/
├── Animation/
│   └── Samurai/
│       ├── IDLE_0.controller       # Animator Controller
│       ├── Idle.anim               # 대기 애니메이션
│       ├── SamuraiRun.anim         # 이동 애니메이션
│       ├── SamuraiAttack.anim      # 공격 애니메이션
│       └── Samurai.prefab          # 사무라이 프리팹
├── Prefabs/
│   └── fxSlashPrefab.prefab        # 슬래시 이펙트 프리팹
├── Scrip/
│   ├── Samurai.cs                  # 사무라이 캐릭터 제어
│   ├── GameMain.cs                 # 게임 흐름 제어
│   └── FxManger.cs                 # 이펙트 싱글톤 매니저
├── FREE_Samurai 2D Pixel Art v1.2/ # 사무라이 스프라이트 에셋
├── NamuFX/                         # 슬래시 VFX 에셋
├── Free Pixel Art Forest/          # 배경 에셋
├── Sword Combat Sound Effects/     # 검 효과음 에셋
├── Settings/                       # URP 렌더러 설정
└── GameScene.unity                 # 메인 씬
```

---

## 스크립트 상세 설명

### `Samurai.cs` — 캐릭터 제어

사무라이 캐릭터의 **이동**, **공격**, **애니메이션 전환**을 담당합니다.

#### Animator State (int 파라미터 `"State"`)

| 값 | 상태 | 설명 |
|----|------|------|
| 0 | Idle | 대기 |
| 1 | Attack | 공격 |
| 2 | Run | 이동 |

#### Inspector 설정값

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| `moveSpeed` | float | 3 | 이동 속도 |
| `arrivalThreshold` | float | 0.05 | 목적지 도달 판정 거리 |
| `fxSlashPoint` | Transform | - | 슬래시 이펙트 생성 위치 (칼끝) |

#### 이벤트

| 이벤트 | 발생 시점 |
|--------|-----------|
| `AttackStarted` | 공격 애니메이션 시작 직후 |
| `AttackEnded` | 공격 애니메이션 완전 종료 후 Idle 복귀 시 |

#### 주요 메서드

```csharp
// 목적지로 이동 (코루틴). 도착 시 onArrived 콜백 실행
public void MoveTo(Vector3 tpos, Action onArrived = null)

// 공격 1회 실행 (코루틴). 애니메이션 종료 후 onFinished 콜백 실행
public void AttackOnce(Action onFinished = null)
```

#### 이동 흐름

```
MoveTo(tpos) 호출
  → Run 애니메이션 시작
  → 이동 방향에 따라 스프라이트 좌우 반전
  → Vector3.MoveTowards로 매 프레임 이동
  → 목적지 도달
  → Idle 애니메이션 복귀
  → onArrived 콜백 실행
```

#### 공격 흐름

```
AttackOnce() 호출
  → Attack 애니메이션 시작
  → AttackStarted 이벤트 발생
  → 한 프레임 대기 (Animator 전환 시간)
  → normalizedTime >= 1 감시 (클립 1회 재생 완료 대기)
  → Idle 애니메이션 복귀
  → AttackEnded 이벤트 발생
  → onFinished 콜백 실행
```

---

### `FxManger.cs` — 이펙트 싱글톤 매니저

게임 내 모든 VFX 생성을 책임지는 **싱글톤** 클래스입니다.  
씬에 오브젝트를 하나 만들고 이 스크립트를 붙여서 사용합니다.

#### 싱글톤 패턴 동작 원리

- `Awake()`에서 `Instance`가 이미 존재하면 자신(`this`)을 제거
- 씬 전체에 단 하나의 인스턴스만 유지
- 어디서든 `FxManger.Instance.PlaySlash(...)` 로 접근 가능

#### Inspector 설정값

| 필드 | 타입 | 설명 |
|------|------|------|
| `fxSlashPrefab` | GameObject | 슬래시 이펙트 프리팹 |

#### 주요 메서드

```csharp
// 지정한 Transform 위치에 슬래시 이펙트를 생성
public void PlaySlash(Transform point)
```

---

### `GameMain.cs` — 게임 흐름 제어

게임 전체의 시작 흐름과 버튼 이벤트를 관리합니다.

#### Inspector 설정값

| 필드 | 타입 | 설명 |
|------|------|------|
| `attackButton` | Button | 공격 버튼 UI |
| `samurai` | Samurai | 사무라이 오브젝트 |

#### 전체 게임 흐름

```
게임 시작 (Start)
  → attackButton 비활성화
  → samurai.MoveTo(Vector3.zero) 호출
      → 사무라이 Run 애니메이션으로 이동
      → 도착 후 Idle 전환
      → attackButton 활성화

attackButton 클릭
  → attackButton 비활성화 (중복 클릭 방지)
  → FxManger.Instance.PlaySlash() — 슬래시 이펙트 생성
  → samurai.AttackOnce() 호출
      → Attack 애니메이션 재생
      → 종료 후 attackButton 다시 활성화
```

---

## Unity Inspector 설정 가이드

### 1. GameMain 오브젝트

| 슬롯 | 연결 대상 |
|------|-----------|
| `Attack Button` | Canvas 하위 버튼 오브젝트 |
| `Samurai` | Hierarchy의 Samurai 오브젝트 |

### 2. Samurai 오브젝트

| 슬롯 | 연결 대상 |
|------|-----------|
| `Fx Slash Point` | Samurai 하위 빈 오브젝트 (칼끝 위치) |

> Samurai 하위에 `Create Empty`로 빈 오브젝트를 만들고 칼끝 위치에 배치한 뒤 연결합니다.

### 3. FxManger 오브젝트

| 슬롯 | 연결 대상 |
|------|-----------|
| `Fx Slash Prefab` | `Assets/Prefabs/fxSlashPrefab.prefab` |

### 4. Animator Controller 설정 확인

`Assets/Animation/Samurai/IDLE_0.controller` 에서 아래를 확인하세요.

| Parameter | 타입 | 전환 조건 |
|-----------|------|-----------|
| `State` | int | 0 = Idle, 1 = Attack, 2 = Run |

> **주의:** `SamuraiAttack.anim` 클립의 **Loop Time을 반드시 꺼야** 공격 종료 감지가 정상 동작합니다.

---

## 사용된 에셋

| 에셋 | 용도 |
|------|------|
| FREE Samurai 2D Pixel Art v1.2 | 사무라이 스프라이트 (Idle / Run / Attack / Hurt) |
| NamuFX - Simple Stylized Slash vol2 | 슬래시 VFX |
| Free Pixel Art Forest | 배경 |
| Sword Combat Sound Effects Pack FREE | 검 효과음 |

---

## 학습 포인트

### 코루틴 (Coroutine)
- `IEnumerator` + `StartCoroutine` 으로 비동기 흐름 제어
- `yield return null` 로 매 프레임 진행
- `normalizedTime >= 1f` 로 애니메이션 종료 감지

### 싱글톤 패턴 (Singleton)
- `static Instance` 프로퍼티로 전역 접근
- `Awake()` 에서 중복 인스턴스 제거

### C# 이벤트 (event)
- `public event Action` 으로 느슨한 결합(Loose Coupling) 구현
- 구독(`+=`) / 발행(`?.Invoke()`) 패턴

### 콜백 (Callback)
- `Action` 파라미터로 완료 시점에 외부 로직 실행
- 이동 완료, 공격 완료 후 버튼 UI 제어에 활용
