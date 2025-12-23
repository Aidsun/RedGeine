using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// =========================================================
// 第一部分：核心逻辑引擎 (TTSCore)
// =========================================================
public static class TTSCore // ✅ 正确：是静态类，不继承任何东西
{
    // ... (保留您之前的 voiceDisplayNames 等变量) ...
    // 音色列表配置
    public static string[] voiceDisplayNames = new string[]
    {
        "晓晓 (标准声 - 女)",
        "云希 (纪录片 - 男声)",
        "云扬 (新闻主播 - 男)",
        "晓涵 (情感故事 - 女)",
        "晓墨 (优雅艺术 - 女)",
        "云夏 (少年活力 - 男)",
        "晓睿 (知性成熟 - 女)",
        "云健 (体育激昂 - 男)",
        "东北老铁 (小北 - 搞笑)",
    };

    public static string[] voiceIds = new string[]
    {
        "zh-CN-XiaoxiaoNeural",
        "zh-CN-YunxiNeural",
        "zh-CN-YunyangNeural",
        "zh-CN-XiaohanNeural",
        "zh-CN-XiaomoNeural",
        "zh-CN-YunxiaNeural",
        "zh-CN-XiaoruiNeural",
        "zh-CN-YunjianNeural",
        "zh-CN-liaoning-XiaobeiNeural",
    };

    // 公用的 GUI 绘制方法
    public static void DrawTTSGUI(string title, string descriptionText, int selectedVoiceIndex, System.Action<int> onVoiceChanged, System.Action onGenerateClick)
    {
        GUILayout.Space(20);
        GUILayout.Label("🎙️ RedGenie 语音自动生成 (Edge-TTS)", EditorStyles.boldLabel);

        // 选择音色
        int newIndex = EditorGUILayout.Popup("选择音色", selectedVoiceIndex, voiceDisplayNames);
        if (newIndex != selectedVoiceIndex)
        {
            onVoiceChanged(newIndex);
        }

        // 生成按钮
        if (GUILayout.Button("生成/更新 配音 (需联网)", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(descriptionText))
            {
                EditorUtility.DisplayDialog("错误", "描述文本为空，请先在脚本里填写描述！", "OK");
                return;
            }
            onGenerateClick();
        }

        GUILayout.Label("注：生成后会自动保存到 Assets/Resources/Audio/TTS 下", EditorStyles.miniLabel);
    }

    // 公用的生成方法
    public static async void GenerateAudio(string title, string text, int voiceIndex, System.Action<AudioClip> onComplete)
    {
        // 清理文本
        text = text.Replace("\n", " ").Replace("\r", " ").Replace("\"", "“");
        string voice = voiceIds[voiceIndex];

        // 路径处理
        string folderPath = Application.dataPath + "/Resources/Audio/TTS";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        // 文件名使用 标题_音色ID.mp3
        string fileName = $"{title}_{voice}.mp3";
        string fullPath = Path.Combine(folderPath, fileName);
        string assetPath = $"Assets/Resources/Audio/TTS/{fileName}";

        EditorUtility.DisplayProgressBar("正在生成语音", $"正在呼叫 {voiceDisplayNames[voiceIndex]}...", 0.5f);

        bool success = await RunEdgeTTS(text, fullPath, voice);

        EditorUtility.ClearProgressBar();

        if (success)
        {
            AssetDatabase.Refresh();
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

            if (clip != null)
            {
                onComplete(clip); // 回调赋值
                UnityEngine.Debug.Log($"✅ 配音成功！文件：{fileName}");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ 音频生成成功但加载失败，请检查路径。");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("失败", "生成失败！请检查 Python 和 edge-tts 是否安装。", "OK");
        }
    }

    private static async Task<bool> RunEdgeTTS(string text, string outputPath, string voice)
    {
        return await Task.Run(() =>
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "edge-tts";
                process.StartInfo.Arguments = $"--text \"{text}\" --write-media \"{outputPath}\" --voice {voice}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Edge-TTS 运行错误: {e.Message}");
                return false;
            }
        });
    }
}

// =========================================================
// 第二部分：图片画框编辑器
// =========================================================
[CustomEditor(typeof(ImageExhibition))]
public class ImageTTSGenerator : Editor
{
    private int selectedVoiceIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ImageExhibition script = (ImageExhibition)target;

        // 只有当勾选了 enableVoiceover 才显示生成器
        if (script.enableVoiceover)
        {
            TTSCore.DrawTTSGUI(script.ImageTitle, script.ImageDescriptionText, selectedVoiceIndex,
                (index) => selectedVoiceIndex = index,
                () =>
                {
                    TTSCore.GenerateAudio(script.ImageTitle, script.ImageDescriptionText, selectedVoiceIndex, (clip) =>
                    {
                        script.artAudioClip = clip;
                        EditorUtility.SetDirty(script);
                    });
                });
        }
    }
}

// =========================================================
// 第三部分：视频画框编辑器
// =========================================================
[CustomEditor(typeof(VideoExhibition))]
public class VideoTTSGenerator : Editor
{
    private int selectedVoiceIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        VideoExhibition script = (VideoExhibition)target;

        string desc = "";
        if (script != null) try { desc = script.VideoDescriptionText; } catch { }

        // 只有当勾选了 enableVoiceover 才显示生成器
        if (script.enableVoiceover)
        {
            TTSCore.DrawTTSGUI(script.VideoTitle, desc, selectedVoiceIndex,
                (index) => selectedVoiceIndex = index,
                () =>
                {
                    TTSCore.GenerateAudio(script.VideoTitle, desc, selectedVoiceIndex, (clip) =>
                    {
                        script.artAudioClip = clip;
                        EditorUtility.SetDirty(script);
                    });
                });
        }
    }
}

// =========================================================
// 第四部分：全景视频画框编辑器
// =========================================================
[CustomEditor(typeof(PanoramaExhibition))]
public class PanoramaTTSGenerator : Editor
{
    private int selectedVoiceIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PanoramaExhibition script = (PanoramaExhibition)target;

        // 只有当勾选了 enableVoiceover 才显示生成器
        if (script.enableVoiceover)
        {
            TTSCore.DrawTTSGUI(script.PanoramaTitle, script.PanoramaDescriptionText, selectedVoiceIndex,
                (index) => selectedVoiceIndex = index,
                () =>
                {
                    TTSCore.GenerateAudio(script.PanoramaTitle, script.PanoramaDescriptionText, selectedVoiceIndex, (clip) =>
                    {
                        script.DescriptionAudio = clip;
                        EditorUtility.SetDirty(script);
                    });
                });
        }
    }
}