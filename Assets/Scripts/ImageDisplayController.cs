using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ImageDisplayController : MonoBehaviour
{
    public TMP_Text titleText;
    public Image contentImage;
    public TMP_Text descriptionText;
    public Image backgroundRenderer;

    private bool isPaused = false;

    void Start()
    {
        // 强制解锁并显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameData.Instance && backgroundRenderer)
            backgroundRenderer.sprite = GameData.Instance.GetRandomContentBG();

        if (GameData.CurrentImage != null)
        {
            var data = GameData.CurrentImage;
            if (titleText) titleText.text = data.Title;
            if (contentImage) contentImage.sprite = data.ImageContent;
            if (descriptionText) descriptionText.text = data.Description;

            // 路由解说 DesAudio
            if (data.AutoPlayVoice && data.VoiceClip != null && AudioManager.Instance && AudioManager.Instance.DesSource)
            {
                var des = AudioManager.Instance.DesSource;
                des.clip = data.VoiceClip;

                // 【核心修改】启动延迟播放协程
                StartCoroutine(DelayPlayVoice(des));
            }
        }
    }

    // 【修改】将延迟时间改为 3.0 秒
    IEnumerator DelayPlayVoice(AudioSource source)
    {
        yield return new WaitForSeconds(3.0f);

        // 播放前检查是否处于暂停状态，防止在设置面板打开时突然播放
        if (!isPaused && source && source.clip)
        {
            source.Play();
        }
    }

    void Update()
    {
        // 监听设置面板状态，控制音频暂停
        if (SettingPanel.Instance && AudioManager.Instance && AudioManager.Instance.DesSource)
        {
            var des = AudioManager.Instance.DesSource;
            bool panelOpen = SettingPanel.Instance.isPanelActive;

            if (panelOpen && !isPaused)
            {
                if (des.isPlaying) des.Pause();
                isPaused = true;
            }
            else if (!panelOpen && isPaused)
            {
                // 面板关闭时，恢复播放
                if (des.clip != null) des.UnPause();
                isPaused = false;

                // 【双重保险】防止鼠标丢失
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}