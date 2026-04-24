using UnityEngine;
using System;
using System.Collections;

// 사무라이 캐릭터의 이동 / 공격 / 애니메이션을 담당하는 클래스
public class Samurai : MonoBehaviour
{
    // ──────────────────────────────────────────
    // Animator State (int 파라미터 "State")
    //   0 : Idle   - 대기
    //   1 : Attack - 공격
    //   2 : Run    - 이동
    // ──────────────────────────────────────────
    private static readonly int StateParam = Animator.StringToHash("State");

    // Animator에서 "Attack" 상태 이름의 해시값 (종료 감지에 사용)
    private static readonly int AttackStateName = Animator.StringToHash("Attack");

    // ── Inspector 설정값 ──────────────────────
    [Header("이동 설정")]
    public float moveSpeed = 3f;          // 이동 속도
    public float arrivalThreshold = 0.05f; // 목적지 도달 판정 거리

    [Header("이펙트")]
    public Transform fxSlashPoint; // 슬래시 이펙트가 생성될 위치 (칼끝 등)

    // ── 이벤트 ────────────────────────────────
    public event Action AttackStarted; // 공격 시작 시 발생
    public event Action AttackEnded;   // 공격 애니메이션 종료 후 발생

    // ── 내부 참조 / 코루틴 ────────────────────
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _moveCoroutine;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ─────────────────────────────────────────
    // 이동
    // ─────────────────────────────────────────

    // 외부에서 호출하는 이동 메서드
    // 이미 이동 중이면 기존 코루틴을 취소하고 새 목적지로 재시작
    public void MoveTo(Vector3 tpos, Action onArrived = null)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(Move(tpos, onArrived));
    }

    // 실제 이동 코루틴
    // 1. Run 애니메이션 시작
    // 2. 이동 방향에 따라 스프라이트 좌우 반전
    // 3. 목적지까지 MoveTowards로 매 프레임 이동
    // 4. 도착 후 Idle 애니메이션 복귀 → onArrived 콜백 실행
    public IEnumerator Move(Vector3 tpos, Action onArrived = null)
    {
        tpos.z = transform.position.z; // Z축은 현재 위치 유지

        SetState(2); // Run

        // 이동 방향에 따라 스프라이트 반전 (오른쪽 이동이면 그대로, 왼쪽이면 flipX)
        float direction = tpos.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.01f)
            _spriteRenderer.flipX = direction < 0;

        while (Vector3.Distance(transform.position, tpos) > arrivalThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                tpos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = tpos; // 오차 보정: 정확한 목적지로 스냅
        SetState(0); // Idle
        _moveCoroutine = null;
        onArrived?.Invoke();
    }

    // ─────────────────────────────────────────
    // 공격
    // ─────────────────────────────────────────

    // 외부에서 호출하는 공격 메서드
    // 이미 공격 중이면 기존 코루틴을 취소하고 다시 시작
    public void AttackOnce(Action onFinished = null)
    {
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackCoroutine(onFinished));
    }

    // 공격 코루틴
    // 1. Attack 애니메이션 시작 + AttackStarted 이벤트 발생
    // 2. 한 프레임 대기 (Animator가 Attack 상태로 전환되는 시간)
    // 3. normalizedTime >= 1이 될 때까지 대기 (클립 1회 재생 완료)
    // 4. Idle 복귀 → AttackEnded 이벤트 발생 → onFinished 콜백 실행
    private IEnumerator AttackCoroutine(Action onFinished = null)
    {
        SetState(1); // Attack
        AttackStarted?.Invoke();

        yield return null; // Animator 전환 대기

        // Attack 클립이 끝날 때까지 매 프레임 감시
        while (true)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Attack") && info.normalizedTime >= 1f)
                break;
            yield return null;
        }

        SetState(0); // Idle
        _attackCoroutine = null;
        AttackEnded?.Invoke();
        onFinished?.Invoke();
    }

    // ─────────────────────────────────────────
    // 유틸리티
    // ─────────────────────────────────────────

    // Animator의 State 파라미터를 변경해 애니메이션 상태를 전환
    private void SetState(int state)
    {
        if (_animator != null)
            _animator.SetInteger(StateParam, state);
    }
}
