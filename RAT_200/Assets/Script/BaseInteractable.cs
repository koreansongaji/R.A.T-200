using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] protected string displayName = "Interact";
    [SerializeField] protected float requiredDistance = 1.2f;

    public string DisplayName => displayName;
    public float RequiredDistance => requiredDistance;

    public virtual bool CanInteract(PlayerInteractor i) => true;
    public abstract void Interact(PlayerInteractor i);
    public Transform AsTransform() => transform;
}