using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class VideoDisplayController : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;   // 拖入场景里的 Video Player
    public RawImage displayScreen;    // 拖入用来显示视频的 Raw Image
    public AspectRatioFitter videoFitter; //拖入 Raw Image 上的 AspectRatioFitter 组件

    [Header("UI 信息绑定")]
    public TMP_Text titleText;        // 拖入右侧的标题文本
    public TMP_Text descriptionText;  // 拖入右侧的介绍文本
    public AudioSource desciptionAudio;

    [Header("控制按钮")]
    public Button exitButton;         // 拖入左上角的退出按钮
    public Button pauseButton;
    [Header("设置")]
    public string returnSceneName = "Museum_Main";

    // 内部状态
    private bool isPrepared = false;

    void Start()
    {
        // 1. 读取全局数据
        var data = GameDate.CurrentVideoDate;

        if (data != null)
        {
            // 设置文字内容
            if (titleText) titleText.text = "《"+data.Title+"》";
            if (descriptionText) descriptionText.text = data.DescriptionText;
            if (desciptionAudio) desciptionAudio.clip = data.DescriptionAudio; desciptionAudio.Play();

            // 设置视频并准备播放
            if (videoPlayer && data.VideoFile)
            {
                videoPlayer.clip = data.VideoFile;

                // 监听视频准备完成事件，用于调整宽高比
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.Prepare(); // 开始准备
            }
        }
        else
        {
            Debug.LogError("【错误】没有读取到视频数据，请从浏览馆入口进入！");
        }

        // 2. 绑定退出按钮
        if (exitButton) exitButton.onClick.AddListener(OnExit);

        // 3. 解锁鼠标，确保可以点击退出按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //增加按钮暂停功能
        pauseButton.onClick.AddListener(TogglePlayPause);
    }

    // 视频准备好后的回调
    void OnVideoPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        vp.Play(); // 自动开始播放

        // 【核心】根据视频源的宽高，动态设置 RawImage 的比例
        if (videoFitter != null)
        {
            // 比如 1920/1080 = 1.777
            videoFitter.aspectRatio = (float)vp.width / vp.height;
        }
    }

    void Update()
    {
        // 监听空格键暂停/继续
        if (isPrepared && Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }

    void OnExit()
    {
        // 停止视频
        if (videoPlayer) videoPlayer.Stop();

        // 移除事件监听防止内存泄漏
        videoPlayer.prepareCompleted -= OnVideoPrepared;

        // 返回大厅
        if (System.Type.GetType("SceneLoding") != null)
        {
            SceneLoding.LoadLevel(returnSceneName);
        }
        else
        {
            SceneManager.LoadScene(returnSceneName);
        }
    }
}