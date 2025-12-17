using UnityEngine;
using StarterAssets;
using System.Reflection;
// using System.Collections; // 不需要这个了

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

        // 如果有存档，立即恢复 (不再使用协程)
        if (GameDate.ShouldRestorePosition)
        {
            RestorePlayerPosition();
        }
    }

    // --- 【核心修改】改为普通方法，移除延迟 ---
    void RestorePlayerPosition() // 这里的 IEnumerator 改成了 void
    {
        // 1. 删除 yield return null; 
        // 因为 SwitchViews 在 Awake 里已经初始化好了，我们不需要再等一帧了！
        // 这样代码就会在画面渲染的第一帧之前执行完毕。

        Debug.Log($">>> [立即恢复] 目标位置: {GameDate.LastPlayerPosition}");

        // 2. 找到 SwitchViews
        SwitchViews switchScript = GetComponent<SwitchViews>();
        if (switchScript != null)
        {
            Transform activePlayer = switchScript.GetActivePlayerTransform();
            CharacterController cc = activePlayer.GetComponent<CharacterController>();

            // 3. 瞬间瞬移
            if (cc != null) cc.enabled = false;

            activePlayer.position = GameDate.LastPlayerPosition;
            activePlayer.rotation = GameDate.LastPlayerRotation;

            // 4. 同步视角
            float targetYaw = GameDate.LastPlayerRotation.eulerAngles.y;
            SyncInternalYaw(activePlayer.gameObject, targetYaw);

            // 5. 强制物理刷新
            Physics.SyncTransforms();

            if (cc != null) cc.enabled = true;

            Debug.Log($"[恢复完毕] 无缝衔接成功！");
        }
        else
        {
            Debug.LogError("PlayerInteraction 未能找到 SwitchViews 组件！");
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

    private void SyncInternalYaw(GameObject playerObj, float yaw)
    {
        MonoBehaviour controller = null;
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