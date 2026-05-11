using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("追従設定")]
    [Tooltip("視点からの距離 (m)")]
    public float followDistance = 0.8f;
    [Tooltip("水平オフセット (m, 正=右)")]
    public float horizontalOffset = 0.22f;
    [Tooltip("垂直オフセット (m, 正=上)")]
    public float verticalOffset = -0.20f;

    [Header("フェード設定")]
    public float fadeSpeed = 5f;

    private float _targetAlpha;
    private int _activeCount;

    private void Start()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    // PickupDescriptionから呼ばれる
    public void Show(Texture2D image, string title)
    {
        _activeCount++;

        if (itemImage != null)
        {
            bool hasImage = image != null;
            itemImage.gameObject.SetActive(hasImage);
            if (hasImage)
                itemImage.texture = image;
        }

        if (titleText != null)
            titleText.text = title;

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

    private void LateUpdate()
    {
        // 完全非表示かつフェード不要なら位置更新スキップ
        if (canvasGroup == null || (canvasGroup.alpha <= 0f && _targetAlpha <= 0f)) return;

        VRCPlayerApi player = Networking.LocalPlayer;
        if (player == null || !player.IsValid()) return;

        VRCPlayerApi.TrackingData head = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        Quaternion rot = head.rotation;

        // ローカル空間のオフセットをヘッド回転で変換して配置
        transform.SetPositionAndRotation(
            head.position + rot * new Vector3(horizontalOffset, verticalOffset, followDistance),
            rot
        );
    }
}
