using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PanoramaExhibition : MonoBehaviour
{
    [Header("全景展品信息")]
    public string PanoramaTitle;
    public VideoClip PanoramaFile;
    public Sprite PanoramaCover;
    [TextArea(5, 10)] public string PanoramaDescriptionText;

    [Header("解说设置")]
    [Tooltip("是否启用语音解说？")]
    public bool enableVoiceover = true; // 【新增】开关
    [Tooltip("全景解说音频")]
    public AudioClip DescriptionAudio;

    [Header("组件设置")]
    public Renderer ContentCover;
    public Renderer outlineRenderer;
    public TMP_Text ShowTitile;

    [Header("跳转场景")]
    public string targetSceneName = "PanoramaContent";

    private void Start()
    {
        if (ShowTitile != null) ShowTitile.text = "《" + PanoramaTitle + "》";
        if (ContentCover != null)
        {
            ContentCover.material.shader = Shader.Find("Unlit/Texture");
            ContentCover.material.mainTexture = PanoramaCover.texture;
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (outlineRenderer != null)
            outlineRenderer.material.color = isActive ? Color.blue : Color.white;
    }

    public void StartDisplay()
    {
        GameDate.PanoramaDate dataPackage = new GameDate.PanoramaDate();
        dataPackage.Title = this.PanoramaTitle;
        dataPackage.panoramaFile = this.PanoramaFile;

        // 【核心逻辑】开关控制音频
        dataPackage.DescriptionAudio = enableVoiceover ? this.DescriptionAudio : null;

        GameDate.CurrentPanoramaDate = dataPackage;

        SavePlayerPosition();

        if (FindObjectOfType<SceneLoding>() != null || System.Type.GetType("SceneLoding") != null)
            SceneLoding.LoadLevel(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }

    private void SavePlayerPosition()
    {
        SwitchViews switchScripts = FindObjectOfType<SwitchViews>();
        if (switchScripts != null)
        {
            Transform activePlayer = switchScripts.GetActivePlayerTransform();
            GameDate.LastPlayerPosition = activePlayer.position;
            GameDate.LastPlayerRotation = activePlayer.rotation;
            GameDate.ShouldRestorePosition = true;
            GameDate.WasFirstPerson = switchScripts.IsInFirstPerson();
        }
    }
}