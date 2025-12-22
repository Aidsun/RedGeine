using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PanoramaExhibition : MonoBehaviour
{
    [Header("全景展品信息")]

    [Tooltip("全景视频标题")]
    public string PanoramaTitle;
    [Tooltip("全景视频文件")]
    public VideoClip PanoramaFile;
    [Tooltip("全景封面")]
    public Sprite PanoramaCover;

    [Header("组件设置")]
    [Tooltip("展示封面组件(使用Quad模型)")]
    public Renderer ContentCover;
    [Tooltip("高亮组件")]
    public Renderer outlineRenderer;
    [Tooltip("标题显示组件")]
    public TMP_Text ShowTitile;

    [Header("跳转场景")]
    [Tooltip("跳转场景名称")]
    public string targetSceneName  = "PanoramaContent";

    private void Start()
    {
        //检测展品信息是否绑定——会警告
        if (PanoramaTitle == null) Debug.LogWarning("有一个全景视频画框没有设置标题！");
        if(PanoramaFile == null) Debug.LogWarning($"《{PanoramaTitle}》的【全景视频文件】未设置！");
        if(PanoramaCover == null) Debug.LogWarning($"《{PanoramaTitle}》的【全景封面】未设置！");
        //组件检测是否绑定——会报错
        if (ContentCover == null) Debug.LogError($"《{PanoramaTitle}》的【展示封面组件】未设置！");
        if (outlineRenderer == null) Debug.LogError($"《{PanoramaTitle}》的【高亮组件】未设置！");
        if(ShowTitile == null) Debug.LogError($"《{PanoramaTitle}》的【标题显示组件】未设置！");

        //标题初始化
        ShowTitile.text = "《"+PanoramaTitle+"》";
        //初始化封面
        ContentCover.material.shader = Shader.Find("Unlit/Texture");//强制转换Shader
        ContentCover.material.mainTexture = PanoramaCover.texture;//赋贴图
    }

    //全景画框高亮函数,供别人调用
    public void SetHighlight(bool isActive)
    {
        
        //设置统一的颜色高亮显示
        //outlineRenderer.material.color = isActive ? GameDate.CurrentHighColor.isActiveColor : GameDate.CurrentHighColor.unActiveColor;
        outlineRenderer.material.color = isActive?Color.blue:Color.white;
    }
    //开始播放函数,供别人调用
    public void StartDisplay()
    {
        //打包当前全景画框的数据
        GameDate.PanoramaDate dataPackage = new GameDate.PanoramaDate();
        dataPackage.Title = this.PanoramaTitle;
        dataPackage.panoramaFile = this.PanoramaFile;
        //发送到全局GameDate
        GameDate.CurrentPanoramaDate = dataPackage;
        //保存位置和视角
        SavePlayerPosition();
        //跳转到全景视频展示场景，需要借用Loding场景进行缓冲
        //先在系统中查找SceneLoding脚本是否存在
        if (FindObjectOfType<SceneLoding>() != null || System.Type.GetType("SceneLoding") != null)
        {
            SceneLoding.LoadLevel(targetSceneName);
        }
        //如果不存在，直接跳转到全景视频展示场景，并且提醒没有加载场景
        else
        {
            SceneManager.LoadScene(targetSceneName);
            Debug.LogWarning("当前系统中没有加载SceneLoding脚本，无法使用加载场景进行缓冲！");
        }
    }
    private void SavePlayerPosition()
    {
        //寻找SwitchViews脚本
        SwitchViews switchScripts = FindObjectOfType<SwitchViews>();
        if(switchScripts == null)
        {
            Debug.LogError("请检查【浏览馆场景】下的【Player节点】是否挂载【SwitchViews脚本】并且配置好面板！");
        }
        else
        {
            //获取当前活动的玩家人称的位置信息
            Transform activePlayer = switchScripts.GetActivePlayerTransform();
            //保存信息
            GameDate.LastPlayerPosition = activePlayer.position;
            GameDate.LastPlayerRotation = activePlayer.rotation;
            //确认需要恢复位置
            GameDate.ShouldRestorePosition = true;
            //保存当前人称状态
            GameDate.WasFirstPerson = switchScripts.IsInFirstPerson();
            //位置、视角、人称状态存档成功提示！
            Debug.Log($"【全景视频】存档成功！");

        }
    }
}
