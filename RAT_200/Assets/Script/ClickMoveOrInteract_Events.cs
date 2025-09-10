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
    public LayerMask groundMask;        // Ground ���̾�
    public LayerMask interactableMask;  // Interactable ���̾�

    [Header("Distances")]
    public float interactClickRadius = 1.6f; // ���ֺ� �����Ÿ���
    public float sampleMaxDistance = 2f;
    public float maxRayDistance = 200f;

    private NavMeshPath _path;

    void Awake() { _path = new NavMeshPath(); }

    // PlayerInput(Invoke Unity Events)���� ������ �޼���
    // Inspector�� Gameplay/Click UnityEvent�� �� �Լ��� �巡���� ��������.
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;                          // performed�� ���� ó��
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        var mouse = Mouse.current; if (mouse == null) return;
        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());

        // 1) Interactable ���� �˻�
        if (Physics.Raycast(ray, out var hitI, maxRayDistance, interactableMask))
        {
            var interactable = hitI.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                float need = Mathf.Max(interactClickRadius, interactable.RequiredDistance);
                if (HorizontalInRange(agent.transform.position, interactable.AsTransform().position, need)
                    && interactable.CanInteract(player))
                {
                    interactable.Interact(player);           // ������ ��� ��ȣ�ۿ�
                    return;
                }

                // �ָ� �̵����� ����(1) : �ٴ����� ����ĳ��Ʈ�ؼ� �̵�
                if (TryMoveToGroundUnderRay(ray)) return;

                // ����(2) : ��� ��ġ �ֺ� NavMesh ���� �̵�
                Vector3 approx = interactable.AsTransform().position;
                if (NavMesh.SamplePosition(approx, out var navHit, sampleMaxDistance, NavMesh.AllAreas))
                {
                    TrySetPath(navHit.position);
                    return;
                }
            }
        }

        // 2) �Ϲ� �̵�(Ground)
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
        // �ֺ� ��ü ����
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
        // TODO: ��� ���� �ǵ��
        return false;
    }

    bool HorizontalInRange(Vector3 a, Vector3 b, float r)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b) <= r;
    }
}
