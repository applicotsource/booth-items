# PickupDescription

展示系 VRChat ワールド向けの Pickup アイテム説明 UI システムです。  
アイテムを手に取ったとき、プレイヤーの視界内に画像とタイトルをフェード表示します。

## 構成スクリプト

| スクリプト | 用途 |
|---|---|
| `PickupDescription.cs` | 各アイテムにアタッチ。Pickup/Drop イベントを UI マネージャーに通知 |
| `PickupDescriptionUI.cs` | シーンに 1 つ配置。Canvas をプレイヤー頭部に追従させて表示制御 |

---

## セットアップ

### 1. UI マネージャーをシーンに配置

以下の階層を作成し、各コンポーネントを設定します。

```
UI (空の GameObject)
│  └ PickupDescriptionUI.cs をアタッチ
│
└── UICanvas
       Canvas        … Render Mode: World Space
       CanvasGroup   … 同 GameObject に追加
       RectTransform … 幅 200, 高さ 250, Scale (0.001, 0.001, 0.001)
       └── Panel (Image で背景)
           ├── RawImage    … AspectRatioFitter 追加推奨
           └── TextMeshProUGUI
```

`PickupDescriptionUI` の Inspector で SerializeField を設定：

| フィールド | セット先 |
|---|---|
| Item Image | `RawImage` コンポーネント |
| Title Text | `TextMeshProUGUI` コンポーネント |
| Canvas Group | `UICanvas` の `CanvasGroup` コンポーネント |

### 2. 各アイテムに PickupDescription をアタッチ

アイテムの GameObject に **`VRC_Pickup`** と **`PickupDescription`** の両方をアタッチし、Inspector を設定します。

| フィールド | 設定内容 |
|---|---|
| Item Image | そのアイテムの説明画像 (Texture2D) |
| Item Title | アイテム名 |
| Ui Manager | シーンの `UI` GameObject |

> **注意**: `VRC_Pickup` がないと `OnPickup` / `OnDrop` が発火しません。

---

## PickupDescriptionUI 設定項目

| フィールド | デフォルト | 説明 |
|---|---|---|
| Follow Distance | `0.8` | 視点からの距離 (m) |
| Horizontal Offset | `0.22` | 水平オフセット (m、正=右) |
| Vertical Offset | `-0.20` | 垂直オフセット (m、正=上) |
| Fade Speed | `5` | フェードの速さ (大きいほど速い) |

位置はプレイヤー頭部 (`TrackingData.Head`) を基準に毎フレーム更新されます。  
デスクトップ・VR・モバイル共通で動作します。

---

## 両手持ち時の挙動

内部カウンターで同時所持数を管理しています。

- アイテムを拾うたびにカウントが増加し、最後に拾ったアイテムの画像を表示
- 片方を手放してもカウントが残っていれば UI は非表示になりません
- 全て手放した時点で UI がフェードアウトします
