using UnityEngine;

public class PlayerReach : MonoBehaviour
{
    [Min(0f)] public float radius = 1.6f;   // 상호작용 가능한 단일 반경(전역)
    public bool horizontalOnly = true;      // 수평 거리만 볼지(Y 무시)
}
