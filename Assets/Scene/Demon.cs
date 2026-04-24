using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Demon : MonoBehaviour
{
    public int    maxHp  = 100;
    public Slider hpSlider;

    private int  currentHp;
    private bool isDead = false;

    private Animator animator;

    private static readonly int StateParam = Animator.StringToHash("State");
    private const int StateIdle  =  0;
    private const int StateHit   = -1;
    private const int StateDeath =  1;

    void Awake()
    {
        animator  = GetComponent<Animator>();
        currentHp = maxHp;

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHp;
            hpSlider.value    = maxHp;
        }
    }

    public void Hit(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"Demon HP: {currentHp}/{maxHp}");

        if (hpSlider != null)
            hpSlider.value = currentHp;

        if (currentHp <= 0)
        {
            currentHp = 0;
            StartCoroutine(DeathCoroutine());
        }
        else
        {
            StartCoroutine(HitCoroutine());
        }
    }

    private IEnumerator HitCoroutine()
    {
        animator.SetInteger(StateParam, StateHit);

        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        animator.SetInteger(StateParam, StateIdle);
    }

    private IEnumerator DeathCoroutine()
    {
        isDead = true;
        animator.SetInteger(StateParam, StateDeath);

        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        Debug.Log("Demon Dead");
        isDead    = false;
        currentHp = maxHp;
        animator.SetInteger(StateParam, StateIdle);

        if (hpSlider != null)
            hpSlider.value = maxHp;
    }
}
