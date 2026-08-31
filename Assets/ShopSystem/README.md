# ShopSystem

独立的 Unity 商店模块，商品目录、存档、道具扣除、奖励发放和广告展示均支持运行时依赖注入。

## 依赖

- Unity 2022.3 或 Unity 6
- Unity UGUI（`com.unity.modules.ui`）
- TextMeshPro（`com.unity.textmeshpro`）

## 商品配置

在 Project 窗口中选择 `Create/Shop System/Shop Catalog` 创建 `ShopCatalog`，然后将商品配置拖到 `ShopMgr` 的 `Catalog` 字段。

商品使用字符串 ID，可配置多个消耗和多个奖励。`MaxPurchaseCount` 为 0 表示不限购买次数；广告商品使用 `RewardedAd`，并通过 `RequiredAdViews` 设置完成一次领取所需的广告次数。

没有配置资产时也可以运行，但商品列表为空并会输出警告。也可以使用 `Resources/ShopCatalog.asset` 作为默认配置。

### 数据来源优先级

商店会按以下顺序选择当前商品目录，优先级从高到低排列：

1. 最近一次通过 `InjectShopData` 注入的有效数据（列表不为 `null` 且至少包含一个商品）。
2. `ShopMgr` Inspector 中序列化的 `ShopCatalog`。
3. `Resources/ShopCatalog.asset`。
4. 空商品目录，并输出明确警告；系统不会伪造默认商品。

传入 `null` 或空列表会清除运行时注入覆盖，随后按上述第 2 至第 4 项重新选择数据。运行时注入数据会被深拷贝，外部修改原列表或商品对象不会影响商店。切换目录不会自动清除已有存档，购买次数和广告进度仍按商品 ID 保留；如果示例场景中的 `ShopExampleBootstrap` 正在运行，其注入数据会覆盖配置资产，移除该组件后即可恢复配置回退流程。

## 运行时注入

```csharp
using System.Collections.Generic;
using ShopSystem;

var products = new List<ShopProductData>
{
    new ShopProductData(
        "coin_pack",
        "金币礼包",
        null,
        new[] { new ShopRewardData("coin", 100) },
        new[] { new ShopCostData("diamond", 10) },
        ShopPurchaseMethod.Currency,
        0,
        0)
};

shopMgr.InjectShopData(products);
```

注入列表会被深拷贝，宿主后续修改原列表不会影响商店。传入 `null` 或空列表会清除运行时数据并恢复 `ShopMgr` 配置或 Resources 配置。替换目录不会清除已有购买进度。

## 外部服务注入

```csharp
shopMgr.ConfigureServices(
    saveStore,
    inventoryService,
    rewardService,
    adService);
```

需要实现以下接口：

- `IShopSaveStore`：读取和保存 `ShopSaveData`
- `IShopInventoryService`：查询、扣除和退还商品消耗
- `IShopRewardService`：发放商品奖励
- `IShopAdService`：展示激励广告并回调 `ShopAdResult`

未注入时使用 `PlayerPrefsShopSaveStore`、`InMemoryShopInventoryService`、`InMemoryShopRewardService` 和不可用广告服务。内存服务仅适合演示和测试，生产项目应替换为自己的实现。

## 购买与事件

```csharp
ShopPurchaseResult result = shopMgr.TryPurchase("coin_pack");
shopMgr.PurchaseCompleted += OnPurchaseCompleted;
shopMgr.ShopChanged += RefreshShop;
```

普通购买会先校验并扣除消耗，再发放奖励。奖励发放失败时会尝试退还消耗。广告购买返回 `Pending`，广告回调完成后通过 `PurchaseCompleted` 通知最终结果；广告服务未配置时返回 `AdUnavailable`，不会发放奖励。

## UI 使用

`ShopWindow` 使用 `ShopItem` 预制体和 `RectTransform` 内容节点生成列表。`ShopItem` 的购买按钮必须绑定 Unity 原生 `Button`，文本使用 `TMP_Text`，图片使用 `Image`。窗口启用时订阅 `ShopChanged`，运行时重新注入数据后会自动刷新。

## 示例场景

`Examples/ShopExample.unity` 提供了可直接打开的示例界面，包含 `ShopMgr`、`ShopWindow`、`EventSystem`、Canvas 和原生 Button。`ShopExampleBootstrap` 仅用于演示运行时注入两件商品；接入正式项目时可删除该组件，改用自己的 `ShopCatalog` 和服务实现。示例预制体为 `Examples/ShopItemExample.prefab`。

## 存档说明

默认存档使用独立 PlayerPrefs Key `ShopSystem.SaveData.v1` 保存购买次数和广告进度。
