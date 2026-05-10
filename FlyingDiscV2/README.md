# Applicotsource Flying Disc System — 使用方法

VRChat 向けのフライングディスク（フリスビー）アセットです。  
UdonSharp を使った物理ベースの飛行シミュレーションを提供します。

---

## 必要環境

- VRChat Creator Companion (VCC)
- UdonSharp
- VRC SDK3

---

## セットアップ手順

### 1. プレハブをシーンに配置する

`Assets/Applicotsource/FlyingDisc/Prefabs` 内の FlyingDisc または FlyingRing の prefab をシーンに配置してください。

### 2. Ground Objects を設定する（必須）

**ここだけは必ず設定してください。** 未設定の場合、ディスクが地面を貫通して落下し続けます。

FlyingDiscBody を選択し、 Inspector の `Ground Collision > Ground Objects` に、着地判定の対象にしたい **コライダーを持つ GameObject** を登録します。

- ワールドの床・地形など、ディスクが着地すべきオブジェクトを追加する

---

## 操作方法

### VR
| 操作 | 内容 |
|------|------|
| 手に取る | トリガー |
| 投げる | 腕を振ってトリガーを離す |

スピンをかけながら投げると揚力が発生し、遠くまで飛びます。

### デスクトップ
| 操作 | 内容 |
|------|------|
| 拾う / 離す | 通常の VRChat Pickup 操作 |
| ディスクを傾ける | J / L キー（左右）、I / K キー（前後）|
| チャージ → 投げる | 右クリック長押し → 離す |

右クリックを長く押すほど速く飛びます（最大 `Desktop Max Charge Time` 秒）。

---

## Inspector パラメータ一覧

設定が必要な場合のみ変更してください。デフォルト値で十分動作します。

### Ground Collision ★必須

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| **Ground Objects** | *(空)* | 着地判定を行うコライダーを持つ GameObject。**必ず設定してください。** |
| Disk Radius | 0.2 m | 着地判定 SphereCast の半径 |

### Marker & Auto-reset

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Landing Marker | *(なし)* | 着地位置に表示するマーカー用 GameObject（任意） |
| Marker Height Offset | 0.1 m | 着地マーカーの表示高さオフセット |
| Auto Reset Time | 20 s | 飛行中・着地後、この秒数が経過すると初期位置に自動リセット |

### Grip

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Right Grip | *(なし)* | VR 右手でピックアップしたときのグリップ基準 Transform |
| Left Grip | *(なし)* | VR 左手でピックアップしたときのグリップ基準 Transform |

### Physics

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Lift Coeff | 1.6 | 揚力の強さ。値を大きくすると浮きやすくなる |
| Lift Precession Scale | 0.5 | 揚力によるジャイロ歳差回転の強さ。カーブの曲がり具合に影響する |
| Drag Coeff | 0.16 | 抗力係数（速度²に乗算）。大きいほど速く減速する |
| Gravity | 9.81 m/s² | 重力加速度 |
| Angular Damping | 0.10 | スピン減衰率（1秒あたりの割合）。大きいほどスピンが早く失われる |
| Reference Spin Rate | 12 rad/s | 揚力が最大になる基準スピン量 |

### Alignment

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Auto Align Speed | 1.0 | スピンが弱いときにディスク法線を上向きに戻す速さ |
| Auto Align Spin Boost | 1.0 | 自動安定化をスピンで抑制するスケール |

### Throw

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Throw Average Time | 0.07 s | 投げ直前の速度・スピンを平均するタイムウィンドウ |
| Throw Speed Scale | 1.9 | 計算した投げ速度に掛けるスケール（飛距離の調整） |
| Angular Velocity Scale | 1.9 | 計算した角速度に掛けるスケール（スピン量の調整） |
| Min Throw Speed | 0.3 m/s | 投げ速度の下限。スピンも下限以下のとき不発になる |
| Min Throw Spin | 1.7 rad/s | 投げスピンの下限。速度も下限以下のとき不発になる |

### Visual

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Disable Visual Rotation | false | チェックするとディスクの回転アニメーションを無効化（物理挙動には影響しない） |

### Desktop Mode

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Desktop Max Charge Time | 2.0 s | 右クリックチャージの最大時間 |
| Desktop Max Speed | 14 m/s | チャージ最大時の投げ速度 |
| Desktop Max Spin | 25 rad/s | チャージ最大時のスピン |
| Desktop Tilt Speed | 90 deg/s | J / L キーの傾き変化速度 |
| Desktop Pitch Speed | 90 deg/s | I / K キーのピッチ変化速度 |
| Desktop Tilt Limit | 70 deg | 傾き・ピッチのクランプ上限 |

---

## よくある問題

**ディスクが地面を貫通する**  
→ `Ground Objects` が未設定、またはコライダーを持たない GameObject を登録しています。

**投げても飛ばない（不発になる）**  
→ 速度・スピンが両方とも閾値（`Min Throw Speed` / `Min Throw Spin`）を下回っています。より素早く腕を振るか、デスクトップモードではチャージ時間を長くしてください。
