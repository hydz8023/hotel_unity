# hotel_unity 游戏架构分析（程序视角）

> 分析基于 `Assets/Script/` 下实际源码 + `AGENTS.md` / `doc/ui-system.md` 设计约定。
> 结论：**分层清晰、SO 数据驱动、输入互斥设计不错**；主要风险集中在**隐藏运行时依赖（FindObjectOfType）、单例紧耦合、占用态未持久化、旋转不更新占用 footprint**。

---

## 1. 总体架构分层

| 层级 | 关键类型 | 职责 | 评价 |
| :--- | :--- | :--- | :--- |
| 数据层（SO / 可序列化） | `FurnitureData`、`FurnitureDatabase`、`InnLayoutData`、`UIPanelConfig`、`UIPanelRegistry` | 纯数据容器，无场景态行为 | ✅ 符合约定 |
| 摆放 / 系统层（MonoBehaviour） | `GridSystem`、`FurniturePlacer`、`PlacementPreview`、`CameraController` | 单一运行时职责 | ✅ 大体单一职责 |
| 运行时表现层 | `FurnitureItem` | 实例 ↔ `FurnitureData` 绑定 | ✅ 轻量 |
| 持久化层 | `LayoutSaver` | JSON 存读删 | ⚠️ 全量重写、无版本迁移 |
| UI 层 | `UIManager`（单例）、`UIPanelBase`、各面板 | 面板生命周期 + 输入互斥 | ⚠️ 单例 + 职责偏多 |
| 输入闸门 | `GameInputGate`、`GameInputMode` | 世界输入 / UI 输入互斥 | ✅ 设计亮点 |

依赖方向总体是 **自上而下的单向**：UI/系统层 → 数据层、持久化层。这一点是健康的。

---

## 2. 值得肯定的设计

- **ScriptableObject 作纯数据**：`FurnitureData` / `FurnitureDatabase` / `UIPanelConfig` / `UIPanelRegistry` 都只有数据 + `[CreateAssetMenu]`，无运行时行为，契合"SO 纯数据"原则。
- **UI 数据驱动**：面板由 `UIPanelRegistry`(SO) → `UIPanelConfig`(SO) 索引，`panelId` 为稳定键，设计师可直接在 Inspector 配置，无需改代码。
- **输入互斥机制（`GameInputGate` + `GameInputMode`）**：`FurniturePlacer` / `PlacementPreview` / `CameraController` 都先查 `GameInputGate.AllowsWorldInput`，再处理射线/输入。这是把"UI 打开时暂停世界"从各个脚本里抽出来的干净做法，避免了散落的 `if (uiOpen)` 判断。
- **单一职责基本落实**：`GridSystem` 只做网格数学与占用；`FurniturePlacer` 只做拖拽/落位/删除；`CameraController` 只做相机；`LayoutSaver` 只做 IO。

---

## 3. 关键问题清单（按严重度）

### 🔴 P0 — 正确性 Bug

**① 家具旋转后占用 footprint 不更新**
- `FurniturePlacer.cs:167-176`（`RotateFurniture`）只做 `transform.Rotate(0,90,0)`，但占用检测仍用 `GetGridSizeFromData` 返回的 **固定 (width, depth)**（`FurniturePlacer.cs:294-297`）。
- 对非正方形家具（如 1×2 屏风），旋转 90° 后实际占地变成 2×1，但 `GridSystem.OccupyCells` / `IsPositionAvailable` 仍按 1×2 校验 → **重叠放置 / 误判可用**。
- 修复：旋转后交换 `gridSize.x` 与 `gridSize.y` 再算占用；并且旋转应重新吸附到网格点。

**② 占用态仅存于 `GridSystem` 运行时内存，未持久化**
- `GridSystem.occupiedGrid` 是 `bool[,]`，仅在场景运行期存在（`GridSystem.cs:13,28`）。
- 存档 `InnLayoutData` 只存 `furnitureId / x / z / rotationY`（`InnLayoutData.cs:14-31`）。重载时由 `FurniturePlacer.LoadAllFurniture` 重新 `OccupyCells` 恢复（`FurniturePlacer.cs:275-288`）。
- 风险：若加载顺序异常或中途异常退出，占用态可能与存档漂移；且 `occupiedGrid` 在 `GridSystem.Start()` 之前为 `null`，若有对象在 `Start` 前调用 `IsPositionAvailable` 会 **NRE**（`GridSystem.cs:72`）。

### 🔴 P1 — 架构耦合（反模式）

**③ `PlacementPreview` 用 `FindObjectOfType` 建立隐藏依赖**
- `PlacementPreview.cs:15`：`gridSystem = FindObjectOfType<GridSystem>();`
- `PlacementPreview.cs:84`：`FurniturePlacer placer = FindObjectOfType<FurniturePlacer>();`
- 这是典型的"运行时全局查找"反模式：依赖关系不可见、不能在空场景独立测试、多实例时取错对象。
- 修复：通过 Inspector 序列化字段或 SO 引用注入 `GridSystem` / `FurniturePlacer`（与 `FurniturePlacer` 已持有 `gridSystem` 引用一致）。

**④ 跨系统直接引用 `UIManager.Instance` 单例**
- `FurniturePlacer.cs:59`：`UIManager.Instance.TryCloseTopPopup()`
- `GameInputGate.cs:6-7`：`UIManager.Instance == null || UIManager.Instance.InputMode == ...`
- `UIManager.cs:19`：经典 `Instance` 单例。
- 问题：玩法层（摆放/输入闸门）反向依赖 UI 单例，打破"数据驱动、解耦"目标；单例也让 UI 难以在隔离环境测试。
- 改进方向（符合本项目 SO 优先哲学）：用 **SO 事件通道**（`GameEvent` / `GameEventListener`）替代。例如 `PlacementPreview` 取消时 `Raise` 一个 `CancelPlacementEvent` SO，由 UI 侧监听并决定是否关 Popup；`GameInputGate` 改为订阅一个 `InputModeVariable`（SO `EnumVariable`）。

### 🟠 P2 — 性能 / 一致性

**⑤ 每次放置都全量重存档**
- `FurniturePlacer.SaveAllFurniture()`（`FurniturePlacer.cs:238-257`）每次都重建整个 `InnLayoutData` 并 `LayoutSaver.SaveLayout`（整文件 `File.WriteAllText`）。
- 家具多时，单次移动 = O(n) 序列化 + 一次磁盘写。短期可接受，规模上来应做 **增量写入 / 脏标记 / 异步保存**。

**⑥ 运行时实例存在三份冗余表示**
- `placedFurnitures: List<GameObject>`（运行时真源，`FurniturePlacer.cs:20`）
- 场景中的 GameObject 本身
- JSON 存档（`InnLayoutData`）
- 三者靠手动同步，任何一处漏更新就漂移。建议引入 **`RuntimeSet<FurnitureItem>`（SO）** 作为统一的"当前已放置家具"集合，落位/删除时只维护它 + 网格占用 + 存档，减少手写列表。

**⑦ `PlacedFurniture.instanceId` 是死字段**
- 生成了 `Guid`（`InnLayoutData.cs:28`），但加载时按 `furnitureId` 重建、无回写映射（`FurniturePlacer.cs:275-288`）。要么用 `instanceId` 做稳定映射，要么删掉避免误导。

### 🟡 P3 — 规范 / 可维护性

**⑧ 目录大小写不一致 `Assets/Script/` vs `Assets/script/`**
- `AGENTS.md` 同时写了两种路径；实际文件在 `Assets/Script/`（大写 S）。`doc/ui-system.md` 也自述存在重复。在大小写不敏感文件系统上易踩坑，建议统一并修正文档。

**⑨ 中文枚举 `FurnitureCategory`**
- `FurnitureData.cs:16-23` 用 `桌椅 / 柜台 / 屏风 ...` 作枚举成员名。C# 允许 Unicode 标识符，但序列化、代码生成、跨工具链都不友好。建议改英文枚举 + `InspectorName`/显示名映射。

**⑩ 存档 `version` 字段未被消费**
- `InnLayoutData.version = 1`（`InnLayoutData.cs:11`）但加载时从不读它做迁移。一旦布局模型变更，旧存档会静默损坏。建议读取时按 `version` 走迁移分支。

---

## 4. 运行链路印证（与文档一致）

1. 进入拖拽 → `FurniturePlacer.TrySelectFurniture`（射线命中 → 暂解除占用）
2. `DragFurniture`：`SnapToGrid` 吸附 → `IsPositionAvailable` 校验 → 切材质
3. `StopDragging`：合法则 `OccupyCells` + `FurnitureItem.SavePosition` + `SaveAllFurniture`；非法回退
4. 预览购买：`PlacementPreview` 跟随 → 点击 → `FurniturePlacer.AddFurniture`
5. 持久化：`LayoutSaver` 写 JSON
6. 重载：`LoadAllFurniture` 读 JSON → 重建实例 + 重占网格

链路本身闭合且正确（除 P0-① 旋转 footprint 问题）。

---

## 5. 优先级改进建议

| 优先级 | 动作 | 预期收益 |
| :--- | :--- | :--- |
| 立即 | 修复旋转 footprint（交换 gridSize + 重新吸附） | 消除重叠放置正确性 Bug |
| 立即 | `GridSystem.occupiedGrid` 延迟初始化 / 空守卫 | 消除潜在 NRE |
| 近期 | `PlacementPreview` 改用注入引用，去掉 `FindObjectOfType` | 可测试、依赖可见 |
| 近期 | 引入 SO 事件通道 + `InputModeVariable`，解耦 `UIManager.Instance` | 玩法层不反向依赖 UI |
| 中期 | `RuntimeSet<FurnitureItem>` 统一已放置集合 | 减少三份表示漂移 |
| 中期 | 增量/异步存档 + `version` 迁移 | 性能与向前兼容 |
| 长期 | 目录大小写统一、英文枚举、死字段清理 | 可维护性 |

---

## 6. 结论

架构骨架是**好的**：分层合理、SO 纯数据、输入互斥优雅、单一职责基本落实。主要短板不是"设计方向错"，而是**工程落地细节**：隐藏的 `FindObjectOfType`、单例反向依赖、以及两个会被玩家直接感知的正确性隐患（旋转 footprint、占用态一致性）。按上面 P0/P1 顺序修，能在不改变现有运行行为的前提下显著提升健壮性与可扩展性。
