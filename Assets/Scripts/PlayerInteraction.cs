using UnityEngine;
using StarterAssets;
using System.Reflection;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    [Header("设置")]
    public float interactionDistance = 10.0f;
    private const string ignoreLayerName = "Player";
    private int finalLayerMask;
    private ImageExhibition lastFrameItem;

    private void Start()
    {
        int playerLayerIndex = LayerMask.NameToLayer(ignoreLayerName);
        if (playerLayerIndex != -1) finalLayerMask = ~(1 << playerLayerIndex);
        else finalLayerMask = ~0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 如果有存档，启动恢复流程
        if (GameDate.ShouldRestorePosition)
        {
            StartCoroutine(RestorePlayerPosition());
        }
    }

    // --- 恢复真正玩家的位置 ---
    IEnumerator RestorePlayerPosition()
    {
        // 1. 等待一帧，确保 SwitchViews 在 Start() 里已经把视角切好了
        yield return null;

        Debug.Log($">>> [开始恢复] 目标位置: {GameDate.LastPlayerPosition}");

        // 2. 找到 SwitchViews，问它现在谁在跑
        SwitchViews switchScript = GetComponent<SwitchViews>();
        if (switchScript != null)
        {
            // 获取当前激活的子物体（FirstPersonPlayer 或 ThirdPersonPlayer）
            Transform activePlayer = switchScript.GetActivePlayerTransform();

            // 3. 获取该子物体上的 CharacterController
            CharacterController cc = activePlayer.GetComponent<CharacterController>();

            // 4. 关闭 CC -> 移动 -> 开启 CC
            if (cc != null) cc.enabled = false;

            // 【关键】移动的是 activePlayer (子物体)，而不是 transform (父物体)
            activePlayer.position = GameDate.LastPlayerPosition;
            activePlayer.rotation = GameDate.LastPlayerRotation;

            // 5. 反射修复视角 (这里要传给 activePlayer 身上的控制器脚本)
            float targetYaw = GameDate.LastPlayerRotation.eulerAngles.y;
            SyncInternalYaw(activePlayer.gameObject, targetYaw);

            // 强制物理同步
            Physics.SyncTransforms();

            Debug.Log($"[恢复完毕] 玩家 {activePlayer.name} 已移动到: {activePlayer.position}");

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogError("PlayerInteraction 未能找到 SwitchViews 组件，无法恢复位置！");
        }

        GameDate.ShouldRestorePosition = false;
    }

    private void Update()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        Color debugColor = Color.yellow;

        if (Physics.Raycast(ray, out hit, interactionDistance, finalLayerMask))
        {
            ImageExhibition itemScript = hit.collider.GetComponentInParent<ImageExhibition>();
            if (itemScript != null)
            {
                debugColor = Color.red;
                if (lastFrameItem != itemScript)
                {
                    if (lastFrameItem != null) lastFrameItem.SetHighlight(false);
                    itemScript.SetHighlight(true);
                    lastFrameItem = itemScript;
                    Debug.Log($"瞄准了: {itemScript.ImageTitle}");
                }

                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                {
                    itemScript.StartDisplay();
                }
            }
            else { ClearHighlight(); }
        }
        else { ClearHighlight(); }

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, debugColor);
    }

    private void ClearHighlight()
    {
        if (lastFrameItem != null) { lastFrameItem.SetHighlight(false); lastFrameItem = null; }
    }


    // 反射方法现在接收一个具体的 GameObject
    private void SyncInternalYaw(GameObject playerObj, float yaw)
    {
        MonoBehaviour controller = null;

        // 在传入的物体上找控制器
        if (playerObj.GetComponent<FirstPersonController>() != null)
            controller = playerObj.GetComponent<FirstPersonController>();
        else if (playerObj.GetComponent<ThirdPersonController>() != null)
            controller = playerObj.GetComponent<ThirdPersonController>();

        if (controller != null)
        {
            string[] possibleFieldNames = new string[] { "_cinemachineTargetYaw", "CinemachineTargetYaw", "_targetRotation" };
            foreach (var name in possibleFieldNames)
            {
                FieldInfo field = controller.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(controller, yaw);
                    break;
                }
            }
        }
    }
}