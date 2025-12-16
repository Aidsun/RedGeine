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

    void Start()
    {
        InitializeComponents();

        // 1. 先关闭 Input，防止抢夺控制
        fpcRoot.SetActive(false);
        tpcRoot.SetActive(false);

        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ------------------ 【核心逻辑：开局定胜负】 ------------------
        // 在 Start 中直接检查存档，恢复正确的视角，避免与 PlayerInteraction 冲突。
        if (GameDate.ShouldRestorePosition)
        {
            SetViewMode(GameDate.WasFirstPerson);
        }
        else
        {
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

        // 计算摄像机对齐
        GetCameraAlignment(oldPlayer, out Vector3 targetPos, out float targetYaw, out float targetPitch);

        // 应用位置到新角色
        newPlayer.position = targetPos;
        newPlayer.rotation = Quaternion.Euler(0, targetYaw, 0);

        // 反射同步内部变量
        MonoBehaviour targetScript = toFps ? fpcScript : tpcScript;
        SyncInternalVariables(targetScript, targetYaw, targetPitch);

        // 激活新的
        newRoot.SetActive(true);
        if (newInput) ResetInput(newInput);
    }

    // =========================================================
    // 对外接口
    // =========================================================

    // 判断当前是否是第一人称 (检查 fpcRoot 激活状态，这是最准确的)
    public bool IsInFirstPerson()
    {
        if (fpcRoot != null) return fpcRoot.activeSelf;
        return true;
    }

    // 【核心接口】获取当前正在控制的玩家 Transform (子物体)
    public Transform GetActivePlayerTransform()
    {
        if (IsInFirstPerson())
        {
            // 如果是第一人称，返回 FPC 玩家 Transform (例如 PlayerCapsule)
            return fpcPlayer != null ? fpcPlayer : transform;
        }
        else
        {
            // 如果是第三人称，返回 TPC 玩家 Transform (例如 PlayerArmature)
            return tpcPlayer != null ? tpcPlayer : transform;
        }
    }

    public void ForceSwitch(bool toFirstPerson)
    {
        SetViewMode(toFirstPerson);
    }

    // =========================================================
    // 辅助方法
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

    private void SetPrivateField(object target, string fieldName, float value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null) field.SetValue(target, value);
    }

    private void ResetInput(StarterAssetsInputs input)
    {
        input.move = Vector2.zero;
        input.look = Vector2.zero;
        input.jump = false;
        input.sprint = false;
        input.analogMovement = false;
    }
}