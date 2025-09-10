using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class RatInput : MonoBehaviour
{
    public InputAction Click { get; private set; }

    const string PREFS_KEY = "Rat.ClickBinding";

    void Awake()
    {
        // 기본: 마우스 왼쪽 버튼. 나중에 런타임에서 다른 버튼으로 리바인드 가능.
        Click = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");

        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            Click.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PREFS_KEY));
        }
    }

    void OnEnable() => Click.Enable();
    void OnDisable() => Click.Disable();

    public void StartRebind(Action onComplete = null)
    {
        Click.Disable();
        Click.PerformInteractiveRebinding()
             // 필요 시 허용/제외 컨트롤 필터링
             //.WithControlsMatching("<Mouse>")      // 마우스만 허용
             .WithControlsExcluding("<Keyboard>")    // 키보드 제외(옵션)
             .OnComplete(op => {
                 op.Dispose();
                 Click.Enable();
                 var json = Click.SaveBindingOverridesAsJson();
                 PlayerPrefs.SetString(PREFS_KEY, json);
                 PlayerPrefs.Save();
                 onComplete?.Invoke();
             })
             .Start();
    }

    public string BindingDisplay() =>
        Click.bindings.Count > 0 ? Click.bindings[0].ToDisplayString() : "(none)";
}
