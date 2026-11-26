# 项目长期记忆 —— Unity2022_TapAwayPuzzle

## 渠道定位
- 只做**抖音小游戏**渠道（StarkSDK / TTSDK）。不再引入 AdMob、Unity Ads、Unity IAP、AppLovin 等其他平台 SDK。
- SDK 目录：`Assets/Plugins/ByteGame/com.bytedance.starksdk`（v6.9.7）+ `com.bytedance.bgdt`（构建工具）。

## 广告架构（不要绕开）
`UI 层 → AdsControl（统一入口）→ AdManager（平台调度，含默认广告位兜底）→ TikTokPlatform（TT 接口实现）`
- 激励视频：`AdsControl.Instance.ShowRewardAd(onSuccess, onFail)`，抖音是点击时拉起，无需预加载。
- 插屏：`DelayShowInterAd` → `AdManager.ShowInterstitialAd()`；抖音侧有 15s 冷启动 + 30s 间隔限制，不满足会静默跳过。
- `IsRewardedVideoAvailable()` 只判断 AdManager 是否就绪。
- 场景里没挂 AdManager 时，AdsControl 会自动 AddComponent 并同步 `appId/videoAdIds/interAdIds`。

## 支付
- `IAPManager` 是**抖音虚拟支付占位桩**，不依赖 com.unity.purchasing。购买直接回调 FAIL。
- 接入时只改 `IAPManager.BuyProductID()` 内部，`ShopView`/`SettingView` 调用方不用动。

## 环境坑
- 编辑器 `C:\Unity\engine\2022.3.62f3c1` 未装 iOS / Android Build Support；`Google.IOSResolver.dll.meta` 已设 `validateReferences: 0`。
- `TikTokPlatform` 外层 `//#if TickTok` 是注释状态，抖音代码在所有平台参与编译，所以 TTSDK 必须常驻。
- 抖音打包走 Unity 菜单的 StarkSDK 工具窗口；打包异常会出现在 `%LOCALAPPDATA%\Unity\Editor\Editor.log`。
- 改 SDK 目录/包清单前先备份到 `.workbuddy/backup/`，SDK 目录用「移出」而不是 `rm -rf`。
