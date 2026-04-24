using System;
using System.Collections;
using UnityEngine;

public class Samurai : MonoBehaviour
{
    public float moveSpeed   = 3f;
    public int   attackPower = 5;

    public Action onAttackStart;
    public Action onAttackEnd;

    private Animator animator;
    private bool isAttacking = false;

    private static readonly int StateParam = Animator.StringToHash("State");
    private const int StateIdle   = 0;
    private const int StateAttack = 1;
    private const int StateRun    = 2;

    public Transform fxPoint; 

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Attack()
    {
        if (isAttacking) return;
        StartCoroutine(AttackCoroutine());
    }

    public void Move(Vector3 targetPosition)
    {
        StartCoroutine(MoveCoroutine(targetPosition));
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        animator.SetInteger(StateParam, StateAttack);
        onAttackStart?.Invoke();

        // 애니메이션이 Attack 상태로 전환될 때까지 한 프레임 대기
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        animator.SetInteger(StateParam, StateIdle);
        isAttacking = false;
        onAttackEnd?.Invoke();
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        animator.SetInteger(StateParam, StateRun);

        while (Mathf.Abs(transform.position.x - targetPosition.x) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        animator.SetInteger(StateParam, StateIdle);
    }
}
