// ClickMoveOrInteract_Events.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickMoveOrInteract_Events : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public NavMeshAgent agent;
    public PlayerInteractor player;

    [Header("Masks")]
    public LayerMask groundMask;        // Ground 레이어
    public LayerMask interactableMask;  // Interactable 레이어

    [Header("Distances")]
    public float interactClickRadius = 1.6f; // “주변 일정거리”
    public float sampleMaxDistance = 2f;
    public float maxRayDistance = 200f;

    private NavMeshPath _path;

    void Awake() { _path = new NavMeshPath(); }

    // PlayerInput(Invoke Unity Events)에서 연결할 메서드
    // Inspector의 Gameplay/Click UnityEvent에 이 함수를 드래그해 넣으세요.
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;                          // performed일 때만 처리
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        var mouse = Mouse.current; if (mouse == null) return;
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());

        // 1) Interactable 먼저 검사
        if (Physics.Raycast(ray, out var hitI, maxRayDistance, interactableMask))
        {
            var interactable = hitI.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                float need = Mathf.Max(interactClickRadius, interactable.RequiredDistance);
                if (HorizontalInRange(agent.transform.position, interactable.AsTransform().position, need)
                    && interactable.CanInteract(player))
                {
                    interactable.Interact(player);           // 가까우면 즉시 상호작용
                    return;
                }

                // 멀면 이동으로 폴백(1) : 바닥으로 레이캐스트해서 이동
                if (TryMoveToGroundUnderRay(ray)) return;

                // 폴백(2) : 대상 위치 주변 NavMesh 보정 이동
                Vector3 approx = interactable.AsTransform().position;
                if (NavMesh.SamplePosition(approx, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
                {
                    TrySetPath(navHit.position);
                    return;
                }
            }
        }

        // 2) 일반 이동(Ground)
        TryMoveToGroundUnderRay(ray);
    }

    bool TryMoveToGroundUnderRay(Ray ray)
    {
        if (Physics.Raycast(ray, out var hitG, maxRayDistance, groundMask))
        {
            if (NavMesh.SamplePosition(hitG.point, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
                return TrySetPath(navHit.position);
        }
        return false;
    }

    bool TrySetPath(Vector3 dst)
    {
        if (_path == null) _path = new NavMeshPath();
        if (agent.CalculatePath(dst, _path) && _path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(_path);
            return true;
        }
        // 주변 대체 지점
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

    bool HorizontalInRange(Vector3 a, Vector3 b, float r)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b) <= r;
    }
}
