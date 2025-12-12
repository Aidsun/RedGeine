using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera playerCamera;

    [Header("Raycast Settings")]
    [Tooltip("射线的最大检测距离")]
    public float interactionDistance = 5.0f;

    // 目标物体的Tag
    public const string targetTag = "Picture";

    // *** 新增：用于射线检测的 Layer Mask ***
    private int raycastLayerMask;

    void Awake()
    {
        // 获取 Player Layer 的索引
        int playerLayerIndex = LayerMask.NameToLayer("Player");

        // 确保找到了名为 "Player" 的 Layer
        if (playerLayerIndex != -1)
        {
            // 1. 创建一个 Mask，只包含 "Player" 这个 Layer (1 << playerLayerIndex)
            // 2. 使用 ~ 运算符（按位取反），得到一个“除了 Player Layer 以外的所有 Layer”的 Mask
            raycastLayerMask = ~(1 << playerLayerIndex);

            Debug.Log($"Raycast Mask Set: Ignoring Layer '{LayerMask.LayerToName(playerLayerIndex)}'.");
        }
        else
        {
            // 如果没找到 "Player" Layer，则默认检测所有 Layer
            Debug.LogError("Layer 'Player' not found! Raycast might hit Player itself.");
            raycastLayerMask = ~0;
        }
    }

    void Update()
    {
        // 确保摄像机已设置
        if (playerCamera == null)
        {
            // 避免频繁输出错误，只在第一次检测到时输出
            if (Time.frameCount % 60 == 0) // 每隔一秒检查一次
            {
                Debug.LogError("Player Camera reference is missing! Please drag the Main Camera into the slot.");
            }
            return;
        }

        // 1. 定义射线：从摄像机视角的中心点发射
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // ** 绘制调试射线（黄色线条） **
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow);

        // 2. 发射射线并检测（传入 Layer Mask）
        // Raycast 现在只会检测除了 Player Layer 之外的物体
        if (Physics.Raycast(ray, out hit, interactionDistance, raycastLayerMask))
        {
            // 射线击中了物体

            // 检查击中的物体是否是目标（Tag是"Picture"）
            if (hit.collider.gameObject.CompareTag(targetTag))
            {
                // ** 检测成功：正在对准画框 **

                Debug.Log($"【对准成功】: 视角中心正对着画框: {hit.collider.gameObject.name}。");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("--- 已触发交互操作 ---");
                    InteractWithPicture(hit.collider.gameObject);
                }
            }
            else
            {
                // ** 检测失败：击中了其他物体 **
                // 这可能是墙壁、地面或其他非 Picture 标签的物体
                Debug.Log($"【对准失败】: 击中物体: {hit.collider.gameObject.name} (Tag: {hit.collider.gameObject.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})。");
            }
        }
        else
        {
            // ** 检测失败：射线没有击中任何物体 **
            Debug.Log("【对准失败】: 射线没有击中任何物体。");
        }
    }

    private void InteractWithPicture(GameObject picture)
    {
        // 您的实际交互逻辑（例如，打开图片查看界面）将在这里实现
    }
}