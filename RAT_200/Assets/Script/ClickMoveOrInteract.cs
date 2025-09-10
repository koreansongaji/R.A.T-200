using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickMoveOrInteract : MonoBehaviour
{
    [Header("Refs")]
    public RatInput input;
    public Camera cam;
    public NavMeshAgent agent;
    public PlayerInteractor player;

    [Header("Masks")]
    public LayerMask groundMask;        // Ground 레이어
    public LayerMask interactableMask;  // Interactable 레이어

    [Header("Distances")]
    public float interactClickRadius = 1.6f; // "주변 일정거리" 기준
    public float sampleMaxDistance = 2f;
    public float maxRayDistance = 200f;

    private NavMeshPath _path;

    void Awake() { _path = new NavMeshPath(); }

    void OnEnable() { input.Click.performed += OnClick; }
    void OnDisable() { input.Click.performed -= OnClick; }

    void OnClick(InputAction.CallbackContext _)
    {
        if (IsUI()) return;

        var mouse = Mouse.current; if (mouse == null) return;
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());

        // 1) Interactable 히트 먼저 확인
        if (Physics.Raycast(ray, out var hitI, maxRayDistance, interactableMask))
        {
            var interactable = hitI.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (IsWithinRange(agent.transform.position, interactable.AsTransform().position,
                                  Mathf.Max(interactClickRadius, interactable.RequiredDistance))
                    && interactable.CanInteract(player))
                {
                    // 충분히 가깝다 → 즉시 상호작용
                    interactable.Interact(player);
                    return;
                }
                // 멀리 있다 → 이동 처리로 폴백(바닥으로 재레이캐스트)
                if (TryMoveToGroundUnderRay(ray)) return;

                // 바닥을 못 찾으면 상호작용 대상 위치 근처 NavMesh로 보정 이동
                Vector3 approx = interactable.AsTransform().position;
                if (NavMesh.SamplePosition(approx, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
                {
                    TrySetPath(navHit.position);
                    return;
                }
            }
        }

        // 2) 일반 이동: Ground 히트
        TryMoveToGroundUnderRay(ray);
    }

    bool TryMoveToGroundUnderRay(Ray ray)
    {
        if (Physics.Raycast(ray, out var hitG, maxRayDistance, groundMask))
        {
            if (NavMesh.SamplePosition(hitG.point, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
            {
                return TrySetPath(navHit.position);
            }
        }
        return false;
    }

    bool TrySetPath(Vector3 dst)
    {
        if (agent.CalculatePath(dst, _path) && _path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(_path);
            return true;
        }
        // 근방 대체 지점 탐색(간단 원형 샘플)
        const float r = 0.8f; const int n = 12;
        for (int i = 0; i < n; i++)
        {
            float ang = (Mathf.PI * 2f) * (i / (float)n);
            Vector3 cand = dst + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * r;
            if (NavMesh.SamplePosition(cand, out var alt, sampleMaxDistance, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(alt.position, _path) && _path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(_path);
                    return true;
                }
            }
        }
        // TODO: 경로 없음 피드백
        return false;
    }

    bool IsUI() =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    // 수평 거리 기준(높이 차 무시)
    bool IsWithinRange(Vector3 a, Vector3 b, float r)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b) <= r;
    }
}
