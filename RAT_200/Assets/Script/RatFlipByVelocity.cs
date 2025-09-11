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

        // 카메라의 오른쪽을 기준으로 좌/우 결정
        float dot = Vector3.Dot(cam.transform.right, v.normalized);
        sr.flipX = (dot < 0f); // 취향대로 반전 부호 조정
    }
}