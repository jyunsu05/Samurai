using UnityEngine;
using UnityEngine.UI;

public class GameMain : MonoBehaviour
{
    public Button attackButton;
    public Samurai samurai;
    public Demon demon;
    public Transform fxPoint;

    void Start()
    {
        samurai.onAttackStart += () =>
        {
            Debug.Log("Attack Start");
            attackButton.interactable = false;
        };
        samurai.onAttackEnd += () =>
        {
            Debug.Log("Attack End");
            attackButton.interactable = true;
        };

        samurai.Move(new Vector3(0, -3.3f, 0));

        attackButton.onClick.AddListener(() =>
        {
            samurai.Attack();
            FxManage.Instance.SpawnSlash(fxPoint);
            demon.Hit(samurai.attackPower);
        });
    }
}
