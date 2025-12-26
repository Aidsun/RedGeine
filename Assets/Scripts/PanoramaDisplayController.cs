using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PanoramaDisplayController : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;
    public Material skyboxMaterial;

    [Header("UI 与 音频")]
    public TMP_Text titleText;
    public Button exitButton;
    public AudioSource audioSource; // 解说音频源

    [Header("设置")]
    public string returnSceneName = "Museum_Main";

    private RenderTexture panoramaRT;
    private float currentVideoVolume = 1.0f;
    private float currentDescriptionVolume = 1.0f;
    private bool isPausedByPanel = false;

    void Awake()
    {
        if (SettingPanel.Instance != null)
        {
            SettingPanel.RegisterApplyMethod(ApplyCurrentSettings);
        }
    }

    void OnDestroy()
    {
        if (SettingPanel.Instance != null)
        {
            SettingPanel.UnregisterApplyMethod(ApplyCurrentSettings);
        }
        if (panoramaRT != null)
        {
            panoramaRT.Release();
            Destroy(panoramaRT);
            panoramaRT = null;
        }
    }

    void Start()
    {
        if (SettingPanel.Instance != null)
        {
            ApplyCurrentSettings(SettingPanel.CurrentSettings);
        }

        var data = GameDate.CurrentPanoramaDate;

        if (data != null)
        {
            if (titleText) titleText.text = "《" + data.Title + "》";

            if (videoPlayer && data.PanoramaFile)
            {
                videoPlayer.clip = data.PanoramaFile;
                panoramaRT = new RenderTexture(4096, 2048, 0);
                videoPlayer.targetTexture = panoramaRT;

                if (skyboxMaterial != null)
                {
                    skyboxMaterial.SetTexture("_MainTex", panoramaRT);
                    RenderSettings.skybox = skyboxMaterial;
                }

                videoPlayer.Prepare();
                videoPlayer.Play();
                SetVideoVolume(currentVideoVolume); // 应用视频音量
            }

            if (audioSource != null && data.DescriptionAudio != null)
            {
                audioSource.clip = data.DescriptionAudio;
                audioSource.volume = currentDescriptionVolume; // 应用解说音量
                audioSource.Play();
            }
        }

        if (exitButton) exitButton.onClick.AddListener(OnExitButtonClicked);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ApplyCurrentSettings(SettingPanel.SettingDate settings)
    {
        // 【核心修复】区分读取
        currentVideoVolume = settings.videoVolume;
        currentDescriptionVolume = settings.descriptionVolume;

        // 1. 更新解说音量
        if (audioSource != null)
        {
            audioSource.volume = currentDescriptionVolume;
        }

        // 2. 更新视频音量
        SetVideoVolume(currentVideoVolume);
    }

    // ... (Update, PauseAll, OnExitButtonClicked 等保持原样)

    void Update()
    {
        if (SettingPanel.Instance != null)
        {
            if (SettingPanel.Instance.isPanelActive)
            {
                if (!isPausedByPanel)
                {
                    PauseAll(true);
                    isPausedByPanel = true;
                }
            }
            else
            {
                if (isPausedByPanel)
                {
                    PauseAll(false);
                    isPausedByPanel = false;
                }
            }
        }
    }

    void PauseAll(bool shouldPause)
    {
        if (shouldPause)
        {
            if (videoPlayer && videoPlayer.isPlaying) videoPlayer.Pause();
            if (audioSource && audioSource.isPlaying) audioSource.Pause();
        }
        else
        {
            if (videoPlayer && !videoPlayer.isPlaying) videoPlayer.Play();
            if (audioSource && !audioSource.isPlaying && audioSource.time > 0 && audioSource.time < audioSource.clip.length)
                audioSource.UnPause();
        }
    }

    void SetVideoVolume(float volume)
    {
        if (videoPlayer != null)
        {
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioVolume(i, volume);
            }
        }
    }

    void OnExitButtonClicked()
    {
        if (videoPlayer) videoPlayer.Stop();
        if (audioSource) audioSource.Stop();
        GameDate.ShouldRestorePosition = true;
        if (System.Type.GetType("SceneLoding") != null) SceneLoding.LoadLevel(returnSceneName);
        else SceneManager.LoadScene(returnSceneName);
    }
}