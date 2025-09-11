using UnityEngine;

public class SphereColorInteractable : BaseInteractable
{
    [SerializeField] Renderer targetRenderer;
    [SerializeField] bool usePropertyBlock = true;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (usePropertyBlock) _mpb = new MaterialPropertyBlock();
        // ★ requiredDistance는 이제 무시됨(플레이어 리치만 사용)
    }

    public override bool CanInteract(PlayerInteractor i) => true;

    public override void Interact(PlayerInteractor i)
    {
        SetRandomColor();
    }

    void SetRandomColor()
    {
        if (!targetRenderer) return;
        Color c = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);

        if (usePropertyBlock && _mpb != null)
        {
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            targetRenderer.SetPropertyBlock(_mpb);
        }
        else
        {
            var m = targetRenderer.material;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.color = c;
        }
    }
}
