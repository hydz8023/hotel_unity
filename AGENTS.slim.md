# AGENTS 精简版（hotel_unity）

## 项目与结构

- Unity 客栈家具摆放项目。
- 主要代码在 `Assets/script/`，相机控制在 `Assets/Script/`，配置在 `ProjectSettings/`。
- 关键模块：网格（`GridSystem`）、摆放（`FurniturePlacer`）、预览（`PlacementPreview`）、相机（`CameraController`）、数据（`FurnitureData`/`FurnitureDatabase`/`InnLayoutData`）、存档（`LayoutSaver`）。

## 核心链路

1. 鼠标/操作输入触发拖拽或新建摆放。
2. 位置吸附到网格（`SnapToGrid`，左下角坐标基准）。
3. 校验可放置（`IsPositionAvailable`）。
4. 材质反馈可放置状态。
5. 确认后同步更新占用格并保存布局。
6. 调试时网格会绘制并显示每格 `(x,z)` 坐标（左下角为 `(0,0)`）。

## 必须遵守

- 不随意改已序列化 public 字段名。
- 改变家具位置/旋转时，必须同步：
  - 网格占用状态；
  - 存档数据（`PlacedFurniture`/布局保存）。
- 取消或回退操作必须恢复占用状态。
- 复用现有摆放链路，避免重复实现射线检测与校验逻辑。
- 网格显示与判定统一使用左下角坐标原点。
- 新运行时摆放逻辑优先放在 `Assets/script/`；相机逻辑放在 `Assets/Script/`。

## 改动后最小验证

- 脚本可编译。
- 家具可：移动、旋转、取消摆放。
- 保存后重进（或重载）布局一致。
- 网格 Gizmos 与坐标标注显示正常。
- 相机 `WASD/QE/滚轮` 生效，且 `Q/E` 围绕视角中心旋转。
