using UnityEngine;

public class PlayerReach : MonoBehaviour
{
    [Min(0f)] public float radius = 1.6f;   // ��ȣ�ۿ� ������ ���� �ݰ�(����)
    public bool horizontalOnly = true;      // ���� �Ÿ��� ����(Y ����)
}
