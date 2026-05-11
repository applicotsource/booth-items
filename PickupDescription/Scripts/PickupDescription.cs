using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PickupDescription : UdonSharpBehaviour
{
    [Header("表示内容")]
    public Texture2D itemImage;
    public string itemTitle = "";

    [Header("UIマネージャー")]
    public PickupDescriptionUI uiManager;

    public override void OnPickup()
    {
        if (uiManager != null)
            uiManager.Show(itemImage, itemTitle);
    }

    public override void OnDrop()
    {
        if (uiManager != null)
            uiManager.Hide();
    }
}
