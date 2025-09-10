using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI; // PointerId
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ClickMoveOrInteract_Events : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public NavMeshAgent agent;
    public PlayerInteractor player;

    [Header("Masks")]
    public LayerMask groundMask;
    public LayerMask interactableMask;

    [Header("Distances")]
    public float interactClickRadius = 1.6f;
    public float sampleMaxDistance = 2f;
    public float maxRayDistance = 200f;

    [Header("UI Raycast (optional but robust)")]
    public bool useManualUIRaycast = true;

    private NavMeshPath _path;
    private bool _queuedClick;
    private Vector2 _queuedScreenPos;

    static readonly List<RaycastResult> _uiHits = new();

    void Awake()
    {
        _path = new NavMeshPath();
    }

    // PlayerInput(Invoke Unity Events)에서 이 메서드를 연결
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        // 콜백 시점에서는 UI가 아직 이번 프레임 업데이트 전이므로, 여기서는 '큐'만 잡아둔다
        var mouse = Mouse.current;
        _queuedScreenPos = mouse != null ? mouse.position.ReadValue() : (Vector2)Input.mousePosition;
        _queuedClick = true;
    }

    void Update()
    {
        if (!_queuedClick) return;
        _queuedClick = false;

        // 1) UI 위인지 최신 상태로 판정
        if (IsPointerOverUI_CurrentFrame(_queuedScreenPos))
            return; // UI 클릭이면 게임 입력 무시

        // 2) 게임 처리: Interactable 우선 → 아니면 Ground 이동
        Ray ray = cam.ScreenPointToRay(_queuedScreenPos);

        // Interactable 먼저
        if (Physics.Raycast(ray, out var hitI, maxRayDistance, interactableMask))
        {
            var interactable = hitI.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                float need = Mathf.Max(interactClickRadius, interactable.RequiredDistance);
                if (HorizontalInRange(agent.transform.position, interactable.AsTransform().position, need)
                    && interactable.CanInteract(player))
                {
                    interactable.Interact(player);
                    return;
                }

                // 멀면 이동 시도 (Ground)
                if (TryMoveToGroundUnderRay(ray)) return;

                // 그래도 못 찾으면 대상 근처 NavMesh 보정
                Vector3 approx = interactable.AsTransform().position;
                if (NavMesh.SamplePosition(approx, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
                {
                    TrySetPath(navHit.position);
                    return;
                }
            }
        }

        // 일반 이동
        TryMoveToGroundUnderRay(ray);
    }

    bool IsPointerOverUI_CurrentFrame(Vector2 screenPos)
    {
        var es = EventSystem.current;
        if (!es) return false;

        var ped = new PointerEventData(es) { position = screenPos };
        _uiHits.Clear();
        es.RaycastAll(ped, _uiHits);   // EventSystem이 등록한 모든 UI Raycaster 대상
        return _uiHits.Count > 0;
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
        if (agent.CalculatePath(dst, _path) && _path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(_path);
            return true;
        }
        // 근방 대체 지점
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
        return false;
    }

    static bool HorizontalInRange(Vector3 a, Vector3 b, float r)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b) <= r;
    }
}
