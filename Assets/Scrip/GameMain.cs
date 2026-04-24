using UnityEngine;
using UnityEngine.UI;

// 게임 전체 흐름을 제어하는 메인 클래스
// - 버튼 이벤트 등록
// - 사무라이 이동 / 공격 호출
public class GameMain : MonoBehaviour
{
    // ── Inspector 연결 ────────────────────────
    public Button attackButton; // 공격 버튼 UI
    public Samurai samurai;     // 사무라이 오브젝트

    void Start()
    {
        // Inspector 연결 누락 체크
        if (samurai == null) { Debug.LogError("GameMain: samurai가 Inspector에서 연결되지 않았습니다."); return; }
        if (attackButton == null) { Debug.LogError("GameMain: attackButton이 Inspector에서 연결되지 않았습니다."); return; }

        // 게임 시작 시 버튼 비활성화 (사무라이가 도착할 때까지 사용 불가)
        attackButton.interactable = false;

        // 공격 버튼 클릭 시 동작 등록
        attackButton.onClick.AddListener(() =>
        {
            attackButton.interactable = false; // 공격 중 중복 클릭 방지

            // FxManger를 통해 슬래시 이펙트 생성
            FxManger.Instance.PlaySlash(samurai.fxSlashPoint);

            // 공격 애니메이션 실행 → 종료 후 버튼 다시 활성화
            samurai.AttackOnce(() =>
            {
                attackButton.interactable = true;
            });
        });

        // 공격 시작 / 종료 이벤트 구독 (필요한 로직으로 교체 가능)
        samurai.AttackStarted += () => Debug.Log("Attack Started");
        samurai.AttackEnded += () => Debug.Log("Attack Ended");

        // 게임 시작 시 사무라이를 (0, 0, 0)으로 이동
        // 도착하면 공격 버튼 활성화
        samurai.MoveTo(Vector3.zero, () =>
        {
            attackButton.interactable = true;
        });
    }

    void Update()
    {

    }
}
