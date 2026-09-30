# 小地图与世界地图

`MiniMapManager` 是基于场景组件的地图控制器，支持图片或相机渲染、2D/3D 模式、玩家位置与朝向同步、缩放/拖动、全屏地图切换及目标图标。

## 主要文件

- `MiniMapManager.cs`：地图相机、目标、渲染与 UI 引用的配置及运行控制。
- `MiniMapIcon.cs`、`MiniMapItem.cs`：地图标记和标记 UI 表现。
- `MiniMapDataStruct.cs`：渲染类型、地图类型等配置枚举。
- `MiniMapMaskHelper.cs`、`WorldSpaceConsult.cs`：遮罩和世界空间辅助。
- `MiniMapUtils.cs`：位置转换、实例查找和编辑器下截图工具。

## 接入

将 `MiniMapManager` 放入场景并在 Inspector 中配置主目标、地图层、相机、Canvas、地图根节点和图标预制体。运行时可通过 `MiniMapManager.Instance.SetMainPlayer(player)` 设置主玩家，并用 `SetMiniMapShow(bool)` 控制显示。

地图遮罩、Layer、相机剔除层和 UI 引用必须相互匹配。`TakeSnapshot(Camera)` 当前只在 Unity Editor 条件下写入截图文件，且要求目标目录已存在；不要将它当作运行时截图 API。
