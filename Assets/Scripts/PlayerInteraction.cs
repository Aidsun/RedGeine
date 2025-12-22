using UnityEngine;
using StarterAssets;
using System.Reflection;

public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [Tooltip("交互距离，默认为10")]
    public float interactionDistance = 10.0f;

    // 忽略玩家层名称（防止射线检测到自己）
    private const string ignoreLayerName = "Player";
    // 最终用于射线检测的 LayerMask
    private int finalLayerMask;
    // 上一帧被高亮的展品脚本
    private MonoBehaviour lastFrameItem;

    private void Start()
    {
        // 获取玩家层索引
        int playerLayerIndex = LayerMask.NameToLayer(ignoreLayerName);
        // 计算最终 LayerMask，忽略玩家层
        if (playerLayerIndex != -1) finalLayerMask = ~(1 << playerLayerIndex);
        else finalLayerMask = ~0;
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 检查是否需要恢复玩家位置
        if (GameDate.ShouldRestorePosition)
        {
            RestorePlayerPosition();
        }
    }

    // 恢复玩家上次浏览馆的位置和视角
    void RestorePlayerPosition()
    {
        Debug.Log($"【正在恢复位置】位置：{GameDate.LastPlayerPosition},视角：{GameDate.LastPlayerRotation}");
        // 获取 SwitchViews 脚本
        SwitchViews switchScript = GetComponent<SwitchViews>();

        if (switchScript != null)
        {
            // 获取当前激活玩家对象
            Transform activePlayer = switchScript.GetActivePlayerTransform();
            CharacterController cc = activePlayer.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;

            // 恢复位置和旋转
            activePlayer.position = GameDate.LastPlayerPosition;
            activePlayer.rotation = GameDate.LastPlayerRotation;

            // 修正内部视角旋转     
            float targetYaw = GameDate.LastPlayerRotation.eulerAngles.y;
            SyncInternalYaw(activePlayer.gameObject, targetYaw);

            Physics.SyncTransforms();

            if (cc != null) cc.enabled = true;

            Debug.Log($"【位置恢复成功！】");
        }
        else
        {
            Debug.LogError("PlayerInteraction 未能找到 SwitchViews 组件！");
        }
        // 标记已恢复
        GameDate.ShouldRestorePosition = false;
    }

    private void Update()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        // 从屏幕中心发射射线
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, finalLayerMask))
        {
            // 查找图片展品脚本
            ImageExhibition imgScript = hit.collider.GetComponentInParent<ImageExhibition>();
            // 查找视频展品脚本
            VideoExhibition vidScript = hit.collider.GetComponentInParent<VideoExhibition>();
            // 查找全景视频展品脚本
            PanoramaExhibition pnmScript = hit.collider.GetComponentInParent<PanoramaExhibition>();

            // 展品高亮与交互逻辑
            if (imgScript != null)
            {
                HandleHighlight(imgScript, imgScript.ImageTitle);
                // 按下E键或鼠标左键进行交互
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                    imgScript.StartDisplay();
            }
            else if (vidScript != null)
            {
                HandleHighlight(vidScript, vidScript.VideoTitle);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                    vidScript.StartDisplay();
            }
            else if(pnmScript != null)
            {
                HandleHighlight(pnmScript, pnmScript.PanoramaTitle);
                if(Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                    pnmScript.StartDisplay();
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    // 展品高亮处理（供内部调用）
    void HandleHighlight(MonoBehaviour currentItem, string itemName)
    {
        if (lastFrameItem != currentItem)
        {
            ClearHighlight(); // 先清除旧高亮

            // 设置新高亮
            if (currentItem is ImageExhibition img) img.SetHighlight(true);
            if (currentItem is VideoExhibition vid) vid.SetHighlight(true);
            if (currentItem is PanoramaExhibition pnm) pnm.SetHighlight(true);

            lastFrameItem = currentItem;
            Debug.Log($"您瞄准了: 《{itemName}》，按下E键或左键可进行交互！");
        }
    }

    // 清除高亮（供内部调用）
    private void ClearHighlight()
    {
        if (lastFrameItem != null)
        {
            if (lastFrameItem is ImageExhibition img) img.SetHighlight(false);
            if (lastFrameItem is VideoExhibition vid) vid.SetHighlight(false);
            if (lastFrameItem is PanoramaExhibition pnm) pnm.SetHighlight(false);
            lastFrameItem = null;
        }
    }

    // 同步玩家内部视角旋转（供位置恢复调用）
    private void SyncInternalYaw(GameObject playerObj, float yaw)
    {
        MonoBehaviour controller = null;
        if (playerObj.GetComponent<FirstPersonController>() != null)
            controller = playerObj.GetComponent<FirstPersonController>();
        else if (playerObj.GetComponent<ThirdPersonController>() != null)
            controller = playerObj.GetComponent<ThirdPersonController>();

        if (controller != null)
        {
            // 兼容不同控制器的内部Yaw字段
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