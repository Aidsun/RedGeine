#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// 这是一个编辑器扩展脚本，不会打包到游戏里
[InitializeOnLoad]
public class Bootstrapper
{
    static Bootstrapper()
    {
        // 监听编辑器播放状态变化
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 当按下 Play 按钮，但还没真正开始运行前
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 获取 Build Settings 里的第 0 个场景 (通常是 StartScene)
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogWarning("请先在 File -> Build Settings 里添加场景！");
                return;
            }

            string startScenePath = EditorBuildSettings.scenes[0].path;

            // 告诉编辑器：启动时加载这个场景
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startScenePath);

            Debug.Log($"已自动定向启动场景: {startScenePath}");
        }
    }
}
#endif