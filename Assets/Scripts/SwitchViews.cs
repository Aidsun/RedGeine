using UnityEngine;
using StarterAssets;
using System.Reflection;

public class SwitchViews : MonoBehaviour
{
    [Header("第一人称视角配置")]
    public GameObject fpcRoot;
    public Transform fpcPlayer;
    public Transform fpcCameraRoot;

    [Header("第三人称视角配置")]
    public GameObject tpcRoot;
    public Transform tpcPlayer;
    public Transform tpcCameraRoot;

    [Header("快捷键设置")]
    public KeyCode switchKey = KeyCode.T;
    [Header("默认第一视角")]
    public bool startInFirstPerson = true;

    // 缓存组件引用
    private StarterAssetsInputs fpcInput, tpcInput;
    private MonoBehaviour fpcScript, tpcScript;

    void Awake()
    {
        InitializeComponents();

        // 1. 先关闭 Input，防止抢夺控制
        if (fpcRoot) fpcRoot.SetActive(false);
        if (tpcRoot) tpcRoot.SetActive(false);

        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ------------------ 【核心逻辑：开局定胜负】 ------------------
        // 优先检查是否有存档的恢复请求 (GameDate)
        if (GameDate.ShouldRestorePosition)
        {
            SetViewMode(GameDate.WasFirstPerson);
        }
        else
        {
            // 如果没有恢复请求，则使用 SettingPanel 里的设置 (如果有) 或者默认值
            // 注意：这里我们暂时用 startInFirstPerson，稍后 SettingPanel 会来覆盖它
            SetViewMode(startInFirstPerson);
        }
    }

    void Update()
    {
        // 监听按键切换
        if (Input.GetKeyDown(switchKey))
        {
            // 取反：当前是第一人称，就切第三，反之亦然
            SetViewMode(!IsInFirstPerson());
        }
    }

    // --- 核心切换逻辑 ---
    private void SetViewMode(bool toFps)
    {
        if (fpcRoot == null || tpcRoot == null) return;

        GameObject oldRoot = toFps ? tpcRoot : fpcRoot;
        GameObject newRoot = toFps ? fpcRoot : tpcRoot;
        Transform oldPlayer = toFps ? tpcPlayer : fpcPlayer;
        Transform newPlayer = toFps ? fpcPlayer : tpcPlayer;
        StarterAssetsInputs oldInput = toFps ? tpcInput : fpcInput;
        StarterAssetsInputs newInput = toFps ? fpcInput : tpcInput;

        // 关闭旧的
        if (oldRoot.activeSelf)
        {
            oldRoot.SetActive(false);
            if (oldInput) ResetInput(oldInput);
        }

        // 计算摄像机对齐 (无缝切换的关键)
        GetCameraAlignment(oldPlayer, out Vector3 targetPos, out float targetYaw, out float targetPitch);

        // 应用位置到新角色
        newPlayer.position = targetPos;
        newPlayer.rotation = Quaternion.Euler(0, targetYaw, 0);

        // 反射同步内部变量 (如 Cinemachine 的角度)
        MonoBehaviour targetScript = toFps ? fpcScript : tpcScript;
        SyncInternalVariables(targetScript, targetYaw, targetPitch);

        // 【新增】切换后，确保新角色的移动参数（速度、跳跃）也是最新的
        // 我们从 SettingPanel 获取当前数据并应用 (如果面板存在)
        if (SettingPanel.Instance != null)
        {
            ApplySettingsToCharacter(targetScript, SettingPanel.Instance.settingData);
        }

        // 激活新的
        newRoot.SetActive(true);
        if (newInput) ResetInput(newInput);
    }

    // =========================================================
    // 【关键新增】供 SettingPanel 调用的接口：更新物理参数
    // =========================================================
    public void UpdateCharacterSettings(float moveSpeed, float jumpHeight, float sensitivity)
    {
        // 更新当前正在激活的那个控制器的参数
        MonoBehaviour activeScript = IsInFirstPerson() ? fpcScript : tpcScript;
        if (activeScript != null)
        {
            // 利用反射设置参数
            SetPublicField(activeScript, "MoveSpeed", moveSpeed);
            SetPublicField(activeScript, "SprintSpeed", moveSpeed * 1.5f); // 跑步速度默认设为走路的1.5倍
            SetPublicField(activeScript, "JumpHeight", jumpHeight);

            // 灵敏度处理：第一人称直接用，第三人称通常需要放大倍数 (因为官方代码里单位不同)
            float rotSpeed = IsInFirstPerson() ? sensitivity : sensitivity * 100f;
            SetPublicField(activeScript, "RotationSpeed", rotSpeed);
        }
    }

    // 内部辅助：把 SettingData 应用到指定脚本
    private void ApplySettingsToCharacter(MonoBehaviour script, SettingPanel.SettingDate data)
    {
        if (script == null || data == null) return;

        SetPublicField(script, "MoveSpeed", data.moveSpeed);
        SetPublicField(script, "SprintSpeed", data.moveSpeed * 1.5f);
        SetPublicField(script, "JumpHeight", data.jumpHeight);

        float rotSpeed = IsInFirstPerson() ? data.mouseXSensitivity : data.mouseXSensitivity * 100f;
        SetPublicField(script, "RotationSpeed", rotSpeed);
    }

    // =========================================================
    // 对外接口
    // =========================================================

    public bool IsInFirstPerson()
    {
        if (fpcRoot != null) return fpcRoot.activeSelf;
        return true;
    }

    public Transform GetActivePlayerTransform()
    {
        if (IsInFirstPerson())
        {
            return fpcPlayer != null ? fpcPlayer : transform;
        }
        else
        {
            return tpcPlayer != null ? tpcPlayer : transform;
        }
    }

    public void ForceSwitch(bool toFirstPerson)
    {
        SetViewMode(toFirstPerson);
    }

    // =========================================================
    // 辅助方法 (反射工具)
    // =========================================================

    private void InitializeComponents()
    {
        if (fpcRoot)
        {
            fpcInput = fpcRoot.GetComponentInChildren<StarterAssetsInputs>(true);
            fpcScript = fpcRoot.GetComponentInChildren<FirstPersonController>(true);
        }
        if (tpcRoot)
        {
            tpcInput = tpcRoot.GetComponentInChildren<StarterAssetsInputs>(true);
            tpcScript = tpcRoot.GetComponentInChildren<ThirdPersonController>(true);
        }
    }

    private void GetCameraAlignment(Transform fallbackTransform, out Vector3 pos, out float yaw, out float pitch)
    {
        pos = fallbackTransform.position;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            yaw = mainCam.transform.eulerAngles.y;
            pitch = mainCam.transform.eulerAngles.x;
        }
        else
        {
            yaw = fallbackTransform.eulerAngles.y;
            pitch = 0f;
        }
        if (pitch > 180) pitch -= 360;
    }

    private void SyncInternalVariables(MonoBehaviour script, float yaw, float pitch)
    {
        if (script == null) return;
        SetPrivateField(script, "_cinemachineTargetYaw", yaw);
        SetPrivateField(script, "_cinemachineTargetPitch", pitch);
    }

    // 设置私有字段 (用于相机角度)
    private void SetPrivateField(object target, string fieldName, float value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null) field.SetValue(target, value);
    }

    // 【新增】设置公共字段 (用于 MoveSpeed, JumpHeight 等)
    // 这一步至关重要，因为官方控制器的变量是 Public 的，但我们没有引用它的类，所以用反射写
    // 修改 SwitchViews.cs 中的 SetPublicField 方法
    private void SetPublicField(object target, string fieldName, float value)
    {
        if (target == null) return;
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            // 增加这行日志，如果在控制台看到这个警告，说明官方脚本变量名改了
            Debug.LogWarning($"[SwitchViews] 警告：在 {target.GetType()} 上找不到变量 {fieldName}，设置失败。");
        }
    }

    private void ResetInput(StarterAssetsInputs input)
    {
        if (input == null) return;
        input.move = Vector2.zero;
        input.look = Vector2.zero;
        input.jump = false;
        input.sprint = false;
        input.analogMovement = false;
    }
}