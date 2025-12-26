using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class VideoDisplayController : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;
    public RawImage displayScreen;
    public AspectRatioFitter videoFitter;

    [Header("UI 信息绑定")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public AudioSource descriptionAudio;  // 解说音频源

    [Header("控制按钮")]
    public Button exitButton;
    public Button pauseButton;

    [Header("设置")]
    public string returnSceneName = "Museum_Main";

    private bool isPrepared = false;
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
    }

    void Start()
    {
        if (SettingPanel.Instance != null)
        {
            ApplyCurrentSettings(SettingPanel.CurrentSettings);
        }

        var data = GameDate.CurrentVideoDate;
        if (data != null)
        {
            if (titleText) titleText.text = "《" + data.Title + "》";
            if (descriptionText) descriptionText.text = data.DescriptionText;

            // 播放解说
            if (descriptionAudio && data.DescriptionAudio)
            {
                descriptionAudio.clip = data.DescriptionAudio;
                descriptionAudio.volume = currentDescriptionVolume; // 应用音量
                descriptionAudio.Play();
            }

            if (videoPlayer && data.VideoFile)
            {
                videoPlayer.clip = data.VideoFile;
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.Prepare();
            }
        }

        if (exitButton) exitButton.onClick.AddListener(OnExitButtonClicked);
        if (pauseButton) pauseButton.onClick.AddListener(TogglePlayPause);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ApplyCurrentSettings(SettingPanel.SettingDate settings)
    {
        // 【核心修复】读取两个不同的音量设置
        currentVideoVolume = settings.videoVolume;
        currentDescriptionVolume = settings.descriptionVolume;

        // 1. 更新解说音量
        if (descriptionAudio != null)
        {
            descriptionAudio.volume = currentDescriptionVolume;
        }

        // 2. 更新视频音量
        if (videoPlayer != null && isPrepared)
        {
            SetVideoPlayerVolume(currentVideoVolume);
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        SetVideoPlayerVolume(currentVideoVolume);
        vp.Play();
        if (videoFitter != null) videoFitter.aspectRatio = (float)vp.width / vp.height;
    }

    void SetVideoPlayerVolume(float volume)
    {
        if (videoPlayer != null)
        {
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioVolume(i, volume);
            }
        }
    }

    // ... (Update, TogglePlayPause, OnExitButtonClicked 保持原样，无需变动)

    void Update()
    {
        if (isPrepared && Input.GetKeyDown(KeyCode.Space)) TogglePlayPause();

        if (SettingPanel.Instance != null)
        {
            if (SettingPanel.Instance.isPanelActive)
            {
                if (!isPausedByPanel)
                {
                    if (videoPlayer.isPlaying) videoPlayer.Pause();
                    if (descriptionAudio && descriptionAudio.isPlaying) descriptionAudio.Pause();
                    isPausedByPanel = true;
                }
            }
            else
            {
                if (isPausedByPanel)
                {
                    if (!videoPlayer.isPlaying) videoPlayer.Play();
                    if (descriptionAudio && !descriptionAudio.isPlaying && descriptionAudio.time > 0)
                        descriptionAudio.UnPause();
                    isPausedByPanel = false;
                }
            }
        }
    }

    void TogglePlayPause()
    {
        if (videoPlayer == null) return;
        if (videoPlayer.isPlaying) videoPlayer.Pause();
        else videoPlayer.Play();
    }

    void OnExitButtonClicked()
    {
        if (videoPlayer) videoPlayer.Stop();
        if (descriptionAudio) descriptionAudio.Stop();
        GameDate.ShouldRestorePosition = true;
        if (System.Type.GetType("SceneLoding") != null) SceneLoding.LoadLevel(returnSceneName);
        else SceneManager.LoadScene(returnSceneName);
    }
}