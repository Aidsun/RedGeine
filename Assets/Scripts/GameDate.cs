using UnityEngine;
using UnityEngine.Video;

public class GameDate : MonoBehaviour
{
    // 图片数据包
    public class ImageDate
    {
        public string Title;
        public string DescriptionText;
        public Sprite ImageShow;
        public AudioClip DescriptionAudio;
    }
    public static ImageDate CurrentImageData;

    // 视频数据包
    public class VideoDate
    {
        public string Title;
        public VideoClip VideoFile;
        public string DescriptionText;
        public AudioClip DescriptionAudio;
    }
    public static VideoDate CurrentVideoDate;

    // 全景视频数据包
    public class PanoramaDate
    {
        public string Title;
        public VideoClip panoramaFile;
        public AudioClip DescriptionAudio; // 【新增】补上音频槽位
    }
    public static PanoramaDate CurrentPanoramaDate;

    // 高亮颜色数据包
    public class HighColor
    {
        public Color unActiveColor = Color.blue;
        public Color isActiveColor = Color.white;
    }
    public static HighColor CurrentHighColor;

    // 归档数据
    public static Vector3 LastPlayerPosition;
    public static Quaternion LastPlayerRotation;
    public static bool ShouldRestorePosition = false;
    public static bool WasFirstPerson = true;
}