using UnityEngine;
using TMPro;

public class ImageExhibition : MonoBehaviour
{
    [Header("数据配置")]
    public string Title;
    public Sprite ImageContent;
    [TextArea] public string Description;
    public bool EnableVoice = true;
    public AudioClip VoiceClip;

    [Header("组件")]
    public Renderer CoverRenderer;
    public Renderer OutlineRenderer;
    public TMP_Text TitleLabel;

    void Start()
    {
        if (TitleLabel) TitleLabel.text = Title;
        if (CoverRenderer && ImageContent)
        {
            CoverRenderer.material.mainTexture = ImageContent.texture;
        }
    }

    public void SetHighlight(bool active)
    {
        if (OutlineRenderer && GameData.Instance)
            OutlineRenderer.material.color = active ? GameData.Instance.HighlightColor : Color.white;
    }

    public void StartDisplay()
    {
        // 1. 保存当前状态 (存入保险箱)
        SaveState();

        // 2. 打包数据
        GameData.ImagePacket packet = new GameData.ImagePacket();
        packet.Title = this.Title;
        packet.ImageContent = this.ImageContent;
        packet.Description = this.Description;
        packet.AutoPlayVoice = this.EnableVoice;
        packet.VoiceClip = this.VoiceClip;

        // 3. 存入全局
        GameData.CurrentImage = packet;

        // 4. 跳转
        SceneLoading.LoadLevel("ImageContent");
    }

    private void SaveState()
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

                // 禁用自动恢复，防止进入展示场景时发生意外逻辑
                GameData.Instance.ShouldRestorePosition = false;

                Debug.Log($"[ImageExhibition] 状态已封存。视角: {(safeData.IsFirstPerson ? "第一人称" : "第三人称")}");
            }
        }
    }
}