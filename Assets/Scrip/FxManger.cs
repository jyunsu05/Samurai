using UnityEngine;

// 게임 내 모든 이펙트 생성을 담당하는 싱글톤 매니저
// 씬에 오브젝트 하나를 만들고 이 스크립트를 붙여서 사용
public class FxManger : MonoBehaviour
{
    // 싱글톤 인스턴스 - 어디서든 FxManger.Instance로 접근 가능
    public static FxManger Instance { get; private set; }

    // ── Inspector 설정값 ──────────────────────
    [Header("이펙트 프리팹")]
    public GameObject fxSlashPrefab; // 슬래시 이펙트 프리팹

    private void Awake()
    {
        // 씬에 FxManger가 이미 존재하면 중복 생성 방지 후 자신을 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ─────────────────────────────────────────
    // 이펙트 생성 메서드
    // ─────────────────────────────────────────

    // 지정한 위치(point)에 슬래시 이펙트 프리팹을 인스턴스화
    // point: 이펙트가 생성될 Transform (사무라이의 fxSlashPoint)
    public void PlaySlash(Transform point)
    {
        if (fxSlashPrefab == null)
        {
            Debug.LogError("FxManger: fxSlashPrefab이 Inspector에서 연결되지 않았습니다.");
            return;
        }
        if (point == null)
        {
            Debug.LogError("FxManger: fxSlashPoint가 null입니다.");
            return;
        }

        Debug.Log($"FX 생성 위치: {point.position}");
        Instantiate(fxSlashPrefab, point.position, point.rotation);
    }
}
