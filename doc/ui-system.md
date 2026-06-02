# UI 管理系统设计（hotel_unity）

> 与摆放系统并列的 UI 生命周期模块。第一阶段已在 v0.2 前落地骨架；后续阶段按版本迭代扩展。

---

## 架构概览

```
UIPanelRegistry (SO)     ──►  panelId → UIPanelConfig
UIPanelConfig (SO)       ──►  prefab / layer / cache / 输入互斥
UIManager (MonoBehaviour)──►  Open / Close / Refresh / Popup 栈
UIPanelBase              ──►  各面板 OnOpen / OnClose / OnRefresh
GameInputGate            ──►  摆放/相机/预览查询 AllowsWorldInput
```

### 目录

```
Assets/script/UI/
├── Core/          UILayer, GameInputMode, UIPanelBase, UIPanelHandle
├── Data/          UIPanelConfig, UIPanelRegistry, HUDData, DailyReportData
├── Panels/        HUDPanel, DailyReportPanel
├── UIManager.cs
├── UIPanelRuntimeFactory.cs   # 无 Prefab 时生成最小 UI
├── GameInputGate.cs
└── UIDemoController.cs        # F1 刷新 HUD / F2 打开日结（开发用）
```

### Canvas 层级（完整规划）

| UILayer | Sort 意图 | 第一阶段 | 典型用途 |
| :--- | :--- | :--- | :--- |
| Background | 0 | 未启用 | 全屏背景、场景内嵌 UI |
| HUD | 50 | **已启用** | 银两、满意度、常驻信息 |
| Normal | 100 | 未启用 | 侧边栏、风闻簿常驻页 |
| Popup | 200 | **已启用** | 日结、商店、招聘弹窗 |
| Top | 300 | 未启用 | Toast、加载遮罩 |

---

## 第一阶段（已实现）

**目标**：HUD + 日结 Popup 的最小可用框架，与摆放/相机输入互斥。

### 已实现能力

- [x] `UIManager`：`Open` / `Close` / `Refresh` / `CloseAll` / `TryCloseTopPopup`
- [x] `UIPanelConfig` + `UIPanelRegistry`（无 Registry 时使用内置 HUD/日结配置）
- [x] `UIPanelBase` 生命周期：`OnOpen` / `OnClose` / `OnRefresh`
- [x] 两层节点：`HUDLayer` + `PopupLayer` + Popup 遮罩
- [x] `cacheOnClose`：关闭时 Hide 而非 Destroy（HUD、日结默认缓存）
- [x] `GameInputMode`：`pauseGameplay=true` 的面板打开时切换为 `UIOnly`
- [x] `GameInputGate`：`FurniturePlacer` / `PlacementPreview` / `CameraController` 已接入
- [x] Esc：优先关闭顶层 Popup；无 Popup 时再取消拖拽/预览
- [x] 内置面板：`HUDPanel`、`DailyReportPanel`（无 Prefab 时运行时生成）
- [x] `UIDemoController`：F1 刷新 HUD，F2 打开日结（演示数据）

### 场景接入

1. 在场景中创建空物体，挂载 `UIManager`（可选挂 `UIDemoController`）。
2. （可选）创建 `UIPanelRegistry` SO，填入 `HUD` / `DailyReport` 的 `UIPanelConfig` 与 Prefab。
3. 运行后 HUD 自动打开；经营系统就绪后调用：

```csharp
UIManager.Instance.Refresh(UIManager.PanelHUD, new HUDData(silver, satisfaction));
UIManager.Instance.Open(UIManager.PanelDailyReport, new DailyReportData(day, income, expense, satisfaction));
```

### 面板 ID 约定

| panelId | 类型 | pauseGameplay | 说明 |
| :--- | :--- | :--- | :--- |
| `HUD` | HUD | false | 与摆放并存 |
| `DailyReport` | Popup | true | 日结，挡输入 |

---

## 第二阶段（v0.3–v0.4 预估）

**目标**：完整 Layer + 与摆放/预览深度互斥；商店等高频面板。

### 计划扩展

- [ ] 启用 `Background` / `Normal` / `Top` 层节点与 Sort Order 策略
- [ ] Popup 栈完善：非栈顶 Close、CloseAllPopup
- [ ] 按面板配置 `cacheOnClose`（低频大面板 Destroy）
- [ ] `FurnitureShop` 面板 + 对接 `PlacementPreview.StartPlacement`
- [ ] `ManualServicePanel`（v0.2 手动 fallback UI 正式化）
- [ ] `UIManager.OnPanelOpened/Closed` 订阅方：暂停/恢复日循环
- [ ] 可选：EventSystem 与 UI Layer 射线分区（避免点 UI 触发家具射线）

### 新增 panelId（预估）

| panelId | Layer | 首现版本 |
| :--- | :--- | :--- |
| `ManualService` | Popup | v0.2 |
| `FurnitureShop` | Popup | v0.4 |
| `RumorBook` | Normal | v0.3 |

---

## 第三阶段（v0.5–v1.0 预估）

**目标**：内容面板增多后的性能与体验。

### 计划扩展

- [ ] Addressables 异步加载（接口与 `Open` 保持不变）
- [ ] `Top` 层：Toast、`LoadingOverlay`
- [ ] 面板转场（淡入淡出 / 缩放，可选 DOTween）
- [ ] 对象池：高频 Instantiate 的面板（如订单气泡）
- [ ] 设置、夜间阶段、势力/对手情报等大面板 `cacheOnClose=false`
- [ ] 与存档分离：UI 不持久化，仅展示 `EconomyService` / `DayCycleService` 数据

### 新增 panelId（预估）

| panelId | Layer | 首现版本 |
| :--- | :--- | :--- |
| `FactionStatus` | HUD/Normal | v0.5 |
| `OpponentIntel` | Popup | v0.5 |
| `NightPhase` | Popup | v1.0 |
| `Settings` | Popup | v1.0 |

---

## 设计原则（与 AGENTS.md 一致）

1. **ScriptableObject 只存配置**，打开/关闭逻辑只在 `UIManager` 与 `UIPanelBase` 子类。
2. **`panelId` 稳定**，与 `furnitureId` 同级，改名需迁移说明。
3. **每个 Open 必须有对称 Close**，避免孤儿实例。
4. **经营规则不进 UI**：面板只展示/转发操作，数据由后续 Service 提供。
5. **不改变布局不变式**：UI 模块不直接改网格占用与 `LayoutSaver` 数据。

---

## 参考

- 版本排期：[TimeLine.md](../TimeLine.md)
- 工程约定：[AGENTS.md](../AGENTS.md)
