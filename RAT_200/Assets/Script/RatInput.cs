using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class RatInput : MonoBehaviour
{
    public InputAction Click { get; private set; }

    const string PREFS_KEY = "Rat.ClickBinding";

    void Awake()
    {
        // �⺻: ���콺 ���� ��ư. ���߿� ��Ÿ�ӿ��� �ٸ� ��ư���� �����ε� ����.
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
             // �ʿ� �� ���/���� ��Ʈ�� ���͸�
             //.WithControlsMatching("<Mouse>")      // ���콺�� ���
             .WithControlsExcluding("<Keyboard>")    // Ű���� ����(�ɼ�)
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
