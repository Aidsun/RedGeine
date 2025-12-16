using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageExhibition : MonoBehaviour
{
    [Header("展品信息")]
    public string ImageTitle;
    [TextArea(5, 10)] public string ImageDescriptionText;
    public Sprite ImageSprite;
    public AudioClip artAudioClip;

    [Header("高亮设置")]
    public Renderer outlineRenderer;

    [Header("跳转目标场景")]
    public string targetSceneName = "ImageContent";

    public void SetHighlight(bool isActive)
    {
        if (outlineRenderer != null)
            outlineRenderer.material.color = isActive ? Color.yellow : Color.white;
    }

    public void StartDisplay()
    {
        // 1. 打包数据
        GameDate.ImageDate dataPackage = new GameDate.ImageDate();
        dataPackage.Title = this.ImageTitle;
        dataPackage.DescriptionText = this.ImageDescriptionText;
        dataPackage.ImageShow = this.ImageSprite;
        dataPackage.DescriptionAudio = this.artAudioClip;
        GameDate.CurrentImageData = dataPackage;

        // ------------------ 【核心修改：存子节点的位置】 ------------------

        // 找到 SwitchViews 脚本 (全场景搜索，最保险的方式)
        SwitchViews switchScript = FindObjectOfType<SwitchViews>();

        if (switchScript != null)
        {
            // 【关键】获取真正移动的那个子物体的 Transform
            Transform activePlayer = switchScript.GetActivePlayerTransform();

            // 保存它的世界坐标 (这才是对的！)
            GameDate.LastPlayerPosition = activePlayer.position;
            GameDate.LastPlayerRotation = activePlayer.rotation;
            GameDate.ShouldRestorePosition = true;

            // 保存视角状态
            GameDate.WasFirstPerson = switchScript.IsInFirstPerson();

            Debug.Log($"=== [存档成功] ===");
            Debug.Log($"保存真实坐标: {GameDate.LastPlayerPosition}");
            Debug.Log($"保存视角: {(GameDate.WasFirstPerson ? "第一人称" : "第三人称")}");
        }
        else
        {
            Debug.LogError("【严重错误】找不到 SwitchViews 脚本！无法确认玩家位置！");
        }
        // -----------------------------------------------------------

        // 2. 跳转场景
        // 使用加载器跳转
        if (FindObjectOfType<SceneLoding>() != null || System.Type.GetType("SceneLoding") != null)
        {
            SceneLoding.LoadLevel(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}