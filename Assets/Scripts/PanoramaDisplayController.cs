using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections; // 必须引入这个命名空间

public class PanoramaDisplayController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Material skyboxMat;
    public TMP_Text titleText;

    private RenderTexture rt;
    private bool isPaused = false;

    void Start()
    {
        if (GameData.CurrentPanorama != null)
        {
            var data = GameData.CurrentPanorama;
            if (titleText) titleText.text = data.Title;

            if (videoPlayer)
            {
                // 创建RT
                rt = new RenderTexture(4096, 2048, 0);
                videoPlayer.targetTexture = rt;
                if (skyboxMat)
                {
                    skyboxMat.SetTexture("_MainTex", rt);
                    RenderSettings.skybox = skyboxMat;
                }
                videoPlayer.clip = data.PanoramaContent;

                // 路由声音到 VidAudio (视频自带的环境音通常是直接放，或者也想延迟？这里保持视频环境音直接放)
                if (AudioManager.Instance && AudioManager.Instance.VidSource)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    videoPlayer.EnableAudioTrack(0, true);
                    videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.VidSource);
                }
                videoPlayer.Play();
            }

            // 播放解说 DesAudio (这是你要延迟的介绍音频)
            if (data.VoiceClip != null && AudioManager.Instance && AudioManager.Instance.DesSource)
            {
                var des = AudioManager.Instance.DesSource;
                des.clip = data.VoiceClip;

                if (data.AutoPlayVoice)
                {
                    // 【核心修改】改为延迟 3 秒播放
                    StartCoroutine(PlayVoiceWithDelay(des, 3.0f));
                }
            }
        }
    }

    // 【新增】延迟播放协程
    IEnumerator PlayVoiceWithDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 如果当前没有被暂停（例如玩家没有打开设置面板），则播放
        if (!isPaused && source && source.clip != null)
        {
            source.Play();
        }
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }

    void Update()
    {
        if (SettingPanel.Instance)
        {
            bool panelOpen = SettingPanel.Instance.isPanelActive;
            if (panelOpen && !isPaused)
            {
                if (videoPlayer.isPlaying) videoPlayer.Pause();
                if (AudioManager.Instance.DesSource.isPlaying) AudioManager.Instance.DesSource.Pause();
                isPaused = true;
            }
            else if (!panelOpen && isPaused)
            {
                videoPlayer.Play();
                // 恢复播放解说（如果它被暂停了）
                // 注意：这里不需要再 check delay，因为 UnPause 只对已开始播放的音频有效
                if (AudioManager.Instance.DesSource.clip != null) AudioManager.Instance.DesSource.UnPause();
                isPaused = false;
            }
        }
    }
}