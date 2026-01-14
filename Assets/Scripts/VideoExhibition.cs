using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class VideoExhibition : MonoBehaviour
{
    [Header("展品数据")]
    public string Title;
    public VideoClip VideoContent;
    public Sprite CoverImage; // 视频封面图
    [TextArea] public string Description;

    [Header("解说设置")]
    public bool EnableVoice = true;
    public AudioClip VoiceClip;

    [Header("组件绑定")]
    public Renderer CoverRenderer;    // 用于显示封面的3D物体
    public Renderer OutlineRenderer;  // 用于显示高亮边框的物体
    public TMP_Text TitleLabel;       // 显示标题的3D文本

    [Header("目标场景")]
    public string TargetScene = "VideoContent";

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
        // 1. 保存玩家当前位置和视角 (存入保险箱)
        SavePlayerState();

        // 2. 打包数据发送给 GameData
        GameData.VideoPacket packet = new GameData.VideoPacket();
        packet.Title = this.Title;
        packet.VideoContent = this.VideoContent;
        packet.Description = this.Description;
        packet.AutoPlayVoice = this.EnableVoice;
        packet.VoiceClip = this.VoiceClip;

        GameData.CurrentVideo = packet;

        // 3. 跳转到视频展示场景
        SceneLoading.LoadLevel(TargetScene);
    }

    private void SavePlayerState()
    {
        SwitchViews sv = FindObjectOfType<SwitchViews>();
        if (sv && GameData.Instance)
        {
            Transform p = sv.GetActivePlayerTransform();
            if (p)
            {
                // =========================================================
                // 【统一更新】使用 TempSafeState 保险箱机制
                // =========================================================
                GameData.PlayerStateData safeData = new GameData.PlayerStateData();
                safeData.Position = p.position;
                safeData.Rotation = p.rotation;
                safeData.IsFirstPerson = sv.IsInFirstPerson();
                safeData.HasData = true;

                GameData.Instance.TempSafeState = safeData;

                GameData.Instance.ShouldRestorePosition = false;

                Debug.Log($"[VideoExhibition] 状态已封存。视角: {(safeData.IsFirstPerson ? "第一人称" : "第三人称")}");
            }
        }
    }
}