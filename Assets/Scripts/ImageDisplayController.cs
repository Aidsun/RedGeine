using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ImageDisplayController : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public TMP_Text imageTitle;
    public Image imageShow;
    public TMP_Text imageDescription;

    [Header("图片解说")]
    public AudioSource imageAudio; // 解说音频源

    [Header("退出设置")]
    public Button exitButton;
    public string returnSceneName = "Museum_Main";

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

        var data = GameDate.CurrentImageData;

        if (data != null)
        {
            if (imageTitle) imageTitle.text = "《" + data.Title + "》";
            if (imageShow && data.ImageFile) imageShow.sprite = data.ImageFile;
            if (imageDescription) imageDescription.text = data.DescriptionText;

            if (imageAudio && data.DescriptionAudio)
            {
                imageAudio.clip = data.DescriptionAudio;
                imageAudio.volume = currentDescriptionVolume; // 应用音量
                imageAudio.Play();
            }
        }

        if (exitButton) exitButton.onClick.AddListener(OnExitButtonClicked);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ApplyCurrentSettings(SettingPanel.SettingDate settings)
    {
        // 【核心修复】只读取解说音量
        currentDescriptionVolume = settings.descriptionVolume;

        if (imageAudio != null)
        {
            imageAudio.volume = currentDescriptionVolume;
        }
    }

    void Update()
    {
        if (SettingPanel.Instance != null)
        {
            if (SettingPanel.Instance.isPanelActive)
            {
                if (imageAudio != null && imageAudio.isPlaying)
                {
                    imageAudio.Pause();
                    isPausedByPanel = true;
                }
            }
            else
            {
                if (isPausedByPanel)
                {
                    if (imageAudio != null) imageAudio.UnPause();
                    isPausedByPanel = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && imageAudio != null && imageAudio.clip != null)
        {
            if (imageAudio.isPlaying) imageAudio.Stop();
            imageAudio.Play();
        }
    }

    void OnExitButtonClicked()
    {
        if (imageAudio) imageAudio.Stop();
        GameDate.ShouldRestorePosition = true;
        if (System.Type.GetType("SceneLoding") != null) SceneLoding.LoadLevel(returnSceneName);
        else SceneManager.LoadScene(returnSceneName);
    }
}