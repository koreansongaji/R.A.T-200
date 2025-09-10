using UnityEngine;

// 오브젝트와 상호작용 시 필요한 요소 인터페이스
public interface IInteractable
{
    string DisplayName { get; }
    float RequiredDistance { get; }       // 필요 접근 거리(대상별)
    bool CanInteract(PlayerInteractor i); // 상태 체크(열쇠 보유 등)
    void Interact(PlayerInteractor i);    // 실행
    Transform AsTransform();              // 위치 참조
}