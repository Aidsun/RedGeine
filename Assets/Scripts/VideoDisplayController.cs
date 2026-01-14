using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VideoDisplayController : MonoBehaviour
{
    [Header("组件")]
    public VideoPlayer videoPlayer;
    public Image backgroundRenderer;
    public RawImage displayScreen;

    // 滚动文字脚本
    public AutoScrollText scrollingDescription;

    private bool isUserPaused = false;
    private bool isSystemPaused = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // =========================================================
        // 1. 启动 BGM 延迟播放流程 (延迟 3 秒)
        // =========================================================
        StartCoroutine(PlayBGMWithDelay(3.0f));

        if (GameData.CurrentVideo != null)
        {
            var data = GameData.CurrentVideo;

            // 设置文字内容，但先不开始滚，等视频准备好获取到音频时长再滚
            if (scrollingDescription)
            {
                var tmp = scrollingDescription.GetComponent<TMP_Text>();
                // 这里加一点空格防止文字紧贴着边缘
                if (tmp) tmp.text = data.Description;
            }

            // 2. 准备视频
            if (videoPlayer)
            {
                videoPlayer.clip = data.VideoContent;

                // 音频路由
                if (AudioManager.Instance && AudioManager.Instance.VidSource)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    videoPlayer.EnableAudioTrack(0, true);
                    videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.VidSource);
                }

                // 绑定准备完成的回调
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.Prepare();
            }
        }
    }

    // --- BGM 延迟协程 ---
    IEnumerator PlayBGMWithDelay(float delay)
    {
        // 先确保 BGM 是静音或者暂停状态
        if (AudioManager.Instance && AudioManager.Instance.BgmSource)
        {
            AudioManager.Instance.BgmSource.Stop();
        }

        // 等待指定时间
        yield return new WaitForSeconds(delay);

        // 开始播放
        if (AudioManager.Instance && AudioManager.Instance.BgmSource)
        {
            AudioManager.Instance.BgmSource.Play();
            Debug.Log("🎵 BGM 已在第 3 秒开始播放");
        }
    }

    // =========================================================
    // 当视频准备好时调用 (核心逻辑都在这)
    // =========================================================
    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();

        // 1. 获取视频原本的宽和高 (注意：使用 vp.width 而不是 vp.texture.width)
        // vp.width 是视频文件的真实宽度 (例如 1080)
        // vp.texture.width 是渲染容器的宽度 (例如 1920) --> 这就是之前的 bug 所在
        int videoWidth = (int)vp.width;
        int videoHeight = (int)vp.height;

        // 2. 将 UI 大小强行设置为视频原尺寸
        if (displayScreen != null)
        {
            displayScreen.rectTransform.sizeDelta = new Vector2(videoWidth, videoHeight);
            Debug.Log($"✅ [最终修正] 视频原文件尺寸: {videoWidth}x{videoHeight} | 已应用到屏幕");
        }

        // 3. 处理解说与文字同步
        var data = GameData.CurrentVideo;
        float audioDuration = 10f;

        if (data.VoiceClip != null)
        {
            audioDuration = data.VoiceClip.length;
            if (AudioManager.Instance && AudioManager.Instance.DesSource && data.AutoPlayVoice)
            {
                var des = AudioManager.Instance.DesSource;
                des.clip = data.VoiceClip;
                des.Play();
            }
        }

        if (scrollingDescription)
        {
            scrollingDescription.StartScrollingByDuration(audioDuration);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer) videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    void Update()
    {
        // 监听设置面板打开状态
        if (SettingPanel.Instance)
        {
            bool panelOpen = SettingPanel.Instance.isPanelActive;
            if (isSystemPaused != panelOpen)
            {
                isSystemPaused = panelOpen;
                RefreshPlayState();
            }
        }

        // 监听空格暂停
        if (GameData.Instance && Input.GetKeyDown(GameData.Instance.VideoPauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isUserPaused = !isUserPaused;
        RefreshPlayState();
    }

    void RefreshPlayState()
    {
        bool shouldPause = isUserPaused || isSystemPaused;

        // 暂停/恢复 视频
        if (videoPlayer)
        {
            if (shouldPause && videoPlayer.isPlaying) videoPlayer.Pause();
            else if (!shouldPause && !videoPlayer.isPlaying) videoPlayer.Play();
        }

        // 暂停/恢复 解说音频 (DesSource)
        if (AudioManager.Instance && AudioManager.Instance.DesSource)
        {
            AudioSource des = AudioManager.Instance.DesSource;
            if (shouldPause && des.isPlaying) des.Pause();
            else if (!shouldPause && des.clip != null) des.UnPause();
        }

        // 注意：BgmSource 不受暂停键影响，通常 BGM 是背景音，暂停视频时 BGM 继续响是合理的，
        // 如果你也想暂停 BGM，可以在这里加上 BgmSource 的控制。
    }
}