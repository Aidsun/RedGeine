using UnityEngine;
using StarterAssets;
using System.Reflection;

public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [Tooltip("交互距离，默认为10（默认值，会被设置面板覆盖）")]
    public float interactionDistance = 10.0f;

    private const string ignoreLayerName = "Player";
    private int finalLayerMask;
    private MonoBehaviour lastFrameItem;

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

    private void Start()
    {
        int playerLayerIndex = LayerMask.NameToLayer(ignoreLayerName);
        if (playerLayerIndex != -1) finalLayerMask = ~(1 << playerLayerIndex);
        else finalLayerMask = ~0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SettingPanel.Instance != null)
        {
            ApplyCurrentSettings(SettingPanel.CurrentSettings);
        }
    }

    private void ApplyCurrentSettings(SettingPanel.SettingDate settings)
    {
        interactionDistance = settings.interactionDistance;
    }

    private void Update()
    {
        if (SettingPanel.Instance != null && SettingPanel.Instance.isPanelActive)
        {
            ClearHighlight();
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, finalLayerMask))
        {
            ImageExhibition imgScript = hit.collider.GetComponentInParent<ImageExhibition>();
            VideoExhibition vidScript = hit.collider.GetComponentInParent<VideoExhibition>();
            PanoramaExhibition pnmScript = hit.collider.GetComponentInParent<PanoramaExhibition>();

            if (imgScript != null)
            {
                HandleHighlight(imgScript, imgScript.ImageTitle);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) imgScript.StartDisplay();
            }
            else if (vidScript != null)
            {
                HandleHighlight(vidScript, vidScript.VideoTitle);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) vidScript.StartDisplay();
            }
            else if (pnmScript != null)
            {
                HandleHighlight(pnmScript, pnmScript.PanoramaTitle);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) pnmScript.StartDisplay();
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void HandleHighlight(MonoBehaviour currentItem, string itemName)
    {
        if (lastFrameItem != currentItem)
        {
            ClearHighlight();

            // 【新增】播放高亮音效
            if (SettingPanel.Instance != null)
            {
                SettingPanel.Instance.PlayHighlightSound();
            }

            if (currentItem is ImageExhibition img) img.SetHighlight(true);
            if (currentItem is VideoExhibition vid) vid.SetHighlight(true);
            if (currentItem is PanoramaExhibition pnm) pnm.SetHighlight(true);

            lastFrameItem = currentItem;
        }
    }

    private void ClearHighlight()
    {
        if (lastFrameItem != null)
        {
            if (lastFrameItem is ImageExhibition img) img.SetHighlight(false);
            if (lastFrameItem is VideoExhibition vid) vid.SetHighlight(false);
            if (lastFrameItem is PanoramaExhibition pnm) pnm.SetHighlight(false);
            lastFrameItem = null;
        }
    }
}