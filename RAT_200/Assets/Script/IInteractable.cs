using UnityEngine;

// ������Ʈ�� ��ȣ�ۿ� �� �ʿ��� ��� �������̽�
public interface IInteractable
{
    string DisplayName { get; }
    float RequiredDistance { get; }       // �ʿ� ���� �Ÿ�(���)
    bool CanInteract(PlayerInteractor i); // ���� üũ(���� ���� ��)
    void Interact(PlayerInteractor i);    // ����
    Transform AsTransform();              // ��ġ ����
}