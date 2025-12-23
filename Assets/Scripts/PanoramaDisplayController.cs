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

    [Header("音频组件")]
    public AudioSource audioSource; // 【新增】记得在面板里拖入 AudioSource

    public string returnSceneName = "Museum_Main";
    private RenderTexture panoramaRT;

    void Start()
    {
        var data = GameDate.CurrentPanoramaDate;

        if (data != null)
        {
            if (videoPlayer && data.panoramaFile)
            {
                videoPlayer.clip = data.panoramaFile;
                panoramaRT = new RenderTexture(4096, 2048, 0);
                videoPlayer.targetTexture = panoramaRT;

                if (skyboxMaterial != null)
                {
                    skyboxMaterial.SetTexture("_MainTex", panoramaRT);
                    RenderSettings.skybox = skyboxMaterial;
                }
                videoPlayer.Play();
            }

            // 【新增】播放音频逻辑
            if (audioSource != null && data.DescriptionAudio != null)
            {
                audioSource.clip = data.DescriptionAudio;
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogError("没有全景视频数据！");
        }
    }

    void Update()
    {
        OnExit();
    }

    void OnExit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (videoPlayer) videoPlayer.Stop();

            // 【新增】停止音频
            if (audioSource) audioSource.Stop();

            if (panoramaRT != null)
            {
                panoramaRT.Release();
                panoramaRT = null;
            }

            if (System.Type.GetType("SceneLoding") != null)
                SceneLoding.LoadLevel(returnSceneName);
            else
                SceneManager.LoadScene(returnSceneName);
        }
    }
}