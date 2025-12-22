using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PanoramaDisplayController : MonoBehaviour
{
    [Header("核心组件")]
    public VideoPlayer videoPlayer;

    [Header("天空盒材质")]
    // 这里需要拖入我们接下来创建的那个 PanoramaSky 材质
    public Material skyboxMaterial;

    public string returnSceneName = "Museum_Main";

    // 内部变量
    private RenderTexture panoramaRT; // 动态生成的超清纹理

    void Start()
    {
        var data = GameDate.CurrentPanoramaDate;

        if (data != null)
        {

            if (videoPlayer && data.panoramaFile)
            {
                videoPlayer.clip = data.panoramaFile;

                // -----------------------------------------------------
                // 【核心画质优化】动态创建 4K 渲染纹理
                // -----------------------------------------------------
                // 宽 4096 x 高 2048 是标准全景比例，保证清晰度
                // 深度缓冲区(depth)设为0，因为只是贴图不需要深度
                panoramaRT = new RenderTexture(4096, 2048, 0);

                // 将视频播放器的输出目标设为这张纹理
                videoPlayer.targetTexture = panoramaRT;

                // 将这张纹理赋给天空盒材质的 MainTex 属性
                if (skyboxMaterial != null)
                {
                    skyboxMaterial.SetTexture("_MainTex", panoramaRT);

                    // 强制设置当前场景的天空盒
                    RenderSettings.skybox = skyboxMaterial;
                }

                videoPlayer.Play();
            }
        }
        else
        {
            Debug.LogError("没有全景视频数据！");
        }

        // 全景视频里，通常允许玩家自由转动视角观察，所以鼠标要锁定还是解锁？
        // 如果是用鼠标拖拽视角，通常需要锁定；如果是自动旋转或VR，则不同。
        // 这里我们先解锁鼠标方便点退出按钮。
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        OnExit();
    }

    void OnExit()
    {
        //Esc键退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (videoPlayer) videoPlayer.Stop();

            // 释放渲染纹理内存
            if (panoramaRT != null)
            {
                panoramaRT.Release();
                panoramaRT = null;
            }

            // 恢复默认天空盒(防止影响其他场景，虽然LoadScene会重置，但保险起见)
            //RenderSettings.skybox = null;
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
}