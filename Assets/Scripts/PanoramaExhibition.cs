using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class PanoramaExhibition : MonoBehaviour
{
    [Header("展品数据")]
    public string Title;
    public VideoClip PanoramaContent; // 全景视频文件
    public Sprite CoverImage;         // 预览封面
    // 全景通常不需要在观看时显示长文本，所以这里只存不传，或者仅用于编辑器预览
    [TextArea] public string DescriptionNote;

    [Header("解说设置")]
    public bool EnableVoice = true;
    public AudioClip VoiceClip;

    [Header("组件绑定")]
    public Renderer CoverRenderer;
    public Renderer OutlineRenderer;
    public TMP_Text TitleLabel;

    [Header("目标场景")]
    public string TargetScene = "PanoramaContent";

    void Start()
    {
        if (TitleLabel) TitleLabel.text = Title;

        if (CoverRenderer && CoverImage)
        {
            CoverRenderer.material.mainTexture = CoverImage.texture;
        }
    }

    public void SetHighlight(bool active)
    {
        if (OutlineRenderer && GameData.Instance)
        {
            OutlineRenderer.material.color = active ? GameData.Instance.HighlightColor : Color.white;
        }
    }

    public void StartDisplay()
    {
        // 1. 保存状态 (存入保险箱)
        SavePlayerState();

        // 2. 打包数据
        GameData.PanoramaPacket packet = new GameData.PanoramaPacket();
        packet.Title = this.Title;
        packet.PanoramaContent = this.PanoramaContent;
        packet.AutoPlayVoice = this.EnableVoice;
        packet.VoiceClip = this.VoiceClip;

        GameData.CurrentPanorama = packet;

        // 3. 跳转
        SceneLoading.LoadLevel(TargetScene);
    }

    private void SavePlayerState()
    {
        // 查找视角控制脚本
        SwitchViews sv = FindObjectOfType<SwitchViews>();

        if (sv && GameData.Instance)
        {
            Transform p = sv.GetActivePlayerTransform();
            if (p)
            {
                // =========================================================
                // 【核心修改】直接存入保险箱 (TempSafeState)
                // =========================================================
                GameData.PlayerStateData safeData = new GameData.PlayerStateData();
                safeData.Position = p.position;
                safeData.Rotation = p.rotation;
                safeData.IsFirstPerson = sv.IsInFirstPerson();
                safeData.HasData = true; // 标记：箱子满了

                GameData.Instance.TempSafeState = safeData;

                // 【重要】确保不要触发普通的恢复逻辑 (防止在全景场景掉入虚空)
                GameData.Instance.ShouldRestorePosition = false;

                Debug.Log($"[Panorama] 状态已安全封存。视角模式: {(safeData.IsFirstPerson ? "第一人称" : "第三人称")}");
            }
        }
    }
}