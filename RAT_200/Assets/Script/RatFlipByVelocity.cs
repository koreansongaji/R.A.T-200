using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SpriteRenderer))]
public class RatFlipByVelocity : MonoBehaviour
{
    public NavMeshAgent agent;
    public Camera cam;
    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        if (!agent || !cam) return;
        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude < 0.0001f) return;

        // ī�޶��� �������� �������� ��/�� ����
        float dot = Vector3.Dot(cam.transform.right, v.normalized);
        sr.flipX = (dot < 0f); // ������ ���� ��ȣ ����
    }
}