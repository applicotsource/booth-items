using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDK3.Rendering;
using VRC.SDKBase;

/// <summary>
/// シーンに1つ配置するUIマネージャー。
/// このスクリプトのGameObjectはCanvas(WorldSpace)の親に置くこと。
/// LateUpdateでプレイヤー頭部を追従し、CanvasGroupのalphaでフェード制御する。
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PickupDescriptionUI : UdonSharpBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private RawImage itemImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("追従設定")]
    [Tooltip("視点からの距離 (m)")]
    public float followDistance = 0.8f;
    [Tooltip("水平オフセット (m, 正=右)")]
    public float horizontalOffset = 0.22f;
    [Tooltip("垂直オフセット (m, 正=上)")]
    public float verticalOffset = -0.20f;

    [Header("フラット表示位置")]
    [Tooltip("デスクトップ/モバイル時は画面左上に固定する")]
    public bool pinFlatScreenToTopLeft = true;
    [Tooltip("フラット表示時の画面左端からの余白 (0-1)")]
    public float flatViewportMarginX = 0.04f;
    [Tooltip("フラット表示時の画面上端からの余白 (0-1)")]
    public float flatViewportMarginY = 0.06f;

    [Header("フェード設定")]
    public float fadeSpeed = 5f;

    [Header("フラット表示スケール")]
    [Tooltip("デスクトップ/モバイル時だけ画面描画サイズに合わせて拡縮する")]
    public bool scaleWithScreenCamera = true;
    [Tooltip("この描画高さ(px)を基準にスケール1として扱う")]
    public float referencePixelHeight = 1080f;
    [Tooltip("フラット表示時の最小スケール倍率")]
    public float minScreenScale = 0.75f;
    [Tooltip("フラット表示時の最大スケール倍率")]
    public float maxScreenScale = 1.35f;

    private float _targetAlpha;
    private int _activeCount;
    private Vector3 _baseScale;
    private RectTransform _canvasRect;
    private bool _lastIsFlatScreen;
    private int _lastPixelHeight = -1;
    private float _lastAppliedScale = -1f;

    private void Start()
    {
        _baseScale = transform.localScale;
        ApplyScreenCameraScale();

        if (canvasGroup == null) return;
        _canvasRect = canvasGroup.GetComponent<RectTransform>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    // PickupDescriptionから呼ばれる
    public void Show(Texture2D image, string title, string description)
    {
        _activeCount++;
        ApplyScreenCameraScale();

        if (itemImage != null)
        {
            bool hasImage = image != null;
            itemImage.gameObject.SetActive(hasImage);
            if (hasImage)
                itemImage.texture = image;
        }

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        _targetAlpha = 1f;
    }

    // PickupDescriptionから呼ばれる
    public void Hide()
    {
        _activeCount = Mathf.Max(0, _activeCount - 1);
        if (_activeCount > 0) return;

        _targetAlpha = 0f;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public override void OnVRCCameraSettingsChanged(VRCCameraSettings cameraSettings)
    {
        if (cameraSettings != VRCCameraSettings.ScreenCamera) return;
        ApplyScreenCameraScale();
    }

    private void ApplyScreenCameraScale()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        bool isFlatScreen = player != null && player.IsValid() && !player.IsUserInVR();
        int pixelHeight = 0;

        VRCCameraSettings screenCamera = VRCCameraSettings.ScreenCamera;
        if (screenCamera != null)
            pixelHeight = screenCamera.PixelHeight;

        float scale = 1f;
        if (scaleWithScreenCamera && isFlatScreen && pixelHeight > 0 && referencePixelHeight > 0f)
            scale = Mathf.Clamp(pixelHeight / referencePixelHeight, minScreenScale, maxScreenScale);

        if (isFlatScreen == _lastIsFlatScreen && pixelHeight == _lastPixelHeight && Mathf.Approximately(scale, _lastAppliedScale))
            return;

        _lastIsFlatScreen = isFlatScreen;
        _lastPixelHeight = pixelHeight;
        _lastAppliedScale = scale;

        transform.localScale = _baseScale * scale;
    }

    private void LateUpdate()
    {
        // 完全非表示かつフェード不要なら位置更新スキップ
        if (canvasGroup == null || (canvasGroup.alpha <= 0f && _targetAlpha <= 0f)) return;

        VRCPlayerApi player = Networking.LocalPlayer;
        if (player == null || !player.IsValid()) return;

        VRCPlayerApi.TrackingData head = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        Quaternion rot = head.rotation;

        if (pinFlatScreenToTopLeft && !player.IsUserInVR())
        {
            ApplyFlatScreenTopLeftPosition(head.position, rot);
            return;
        }

        // ローカル空間のオフセットをヘッド回転で変換して配置
        transform.SetPositionAndRotation(
            head.position + rot * new Vector3(horizontalOffset, verticalOffset, followDistance),
            rot
        );
    }

    private void ApplyFlatScreenTopLeftPosition(Vector3 cameraPosition, Quaternion cameraRotation)
    {
        float viewportX = Mathf.Clamp01(flatViewportMarginX);
        float viewportY = 1f - Mathf.Clamp01(flatViewportMarginY);
        VRCCameraSettings screenCamera = VRCCameraSettings.ScreenCamera;
        float verticalFov = 60f;
        float aspect = 16f / 9f;
        if (screenCamera != null)
        {
            verticalFov = screenCamera.FieldOfView;
            if (screenCamera.PixelHeight > 0)
                aspect = (float)screenCamera.PixelWidth / screenCamera.PixelHeight;
        }

        float halfHeight = Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * followDistance;
        float halfWidth = halfHeight * aspect;
        float normalizedX = (viewportX - 0.5f) * 2f;
        float normalizedY = (viewportY - 0.5f) * 2f;

        if (_canvasRect != null && halfWidth > 0f && halfHeight > 0f)
        {
            Vector3 canvasScale = _canvasRect.lossyScale;
            float panelHalfWidth = _canvasRect.rect.width * Mathf.Abs(canvasScale.x) * 0.5f;
            float panelHalfHeight = _canvasRect.rect.height * Mathf.Abs(canvasScale.y) * 0.5f;
            normalizedX = -1f + viewportX * 2f + panelHalfWidth / halfWidth;
            normalizedY = 1f - (1f - viewportY) * 2f - panelHalfHeight / halfHeight;
            normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
            normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);
        }

        Vector3 localOffset = new Vector3(
            normalizedX * halfWidth,
            normalizedY * halfHeight,
            followDistance
        );

        transform.SetPositionAndRotation(cameraPosition + cameraRotation * localOffset, cameraRotation);
    }
}
