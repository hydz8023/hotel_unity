# Demo 范围（玩法验证版）

> 本文档把"先用最小内容验证玩法"的范围定下来。目标不是做缩水版全游戏，而是用最低成本把《策划案》第 1.3 节的三条设计标准**验活**。验不过就说明方向要改，比做完 14 个系统才发现不好玩便宜一百倍。

---

## 1. 这份 demo 要证明什么

只验证三件事，对应第 1.3 节的三条不可妥协标准：

| 标准 | demo 要证明的 |
| :--- | :--- |
| **经营循环优先** | 玩家能跑通"扩建 / 招待 / 经营 / 赚钱"的循环，且每次游玩主轴都是它 |
| **布置即决策** | 家具摆放产生真实取舍与后果（空间冲突、动线、房间标签匹配），不是纯装饰 |
| **稀缺才有意思** | 银两、地盘、时间始终紧张，决策有代价也有反馈，后期不会通胀到"钱没处花" |

任何不属于这三件事的内容，一律不进 demo。

---

## 2. 必须有的内容

复用已做好的网格摆放 + JSON 存档（v0.1），不碰寻路、不碰员工 AI，全程手动操作。

| 内容 | 为什么必须有 | 验证哪条 |
| :--- | :--- | :--- |
| 一个网格房间 + 已建好的摆放 / 存档 | 布置的舞台，技术已就绪、零风险 | 布置即决策 |
| 3 种客人：普通旅人 / 文人 / 侠客，**需求互相冲突** | 文人要"静修"（屏风 + 离嘈杂），侠客要"兵器"（兵器架）；空间小就逼二选一 | 布置即决策 |
| 手动接待 → 满足需求 → 收钱 → 日结盈亏 | 经营循环本身 | 经营循环优先 |
| 4–5 件"有用"家具：床、屏风、兵器架、灶台、桌椅 | 每件都直接改结果，不是纯好看 | 布置即决策 |
| 极简银两进出：进 = 房费 + 餐饮，出 = 买家具 + 扩建 | 制造"钱怎么花"的决断 | 稀缺才有意思 |
| 客人耐心 + 有限空间 | 没对应房间 / 摆不对，客人直接走、钱没了 | 稀缺才有意思 |
| 清晰反馈：满意度气泡、丢客原因、日结报表 | 让每次取舍的后果看得见 | 循环清晰 |

**关键技术约定**：底层走网格（见策划案 4.4.0），不作自由摆放。

---

## 3. 砍掉的内容（这些是皮 / 后期加料，demo 不验）

一份都别进 demo，进了就又滑回范围爆炸：

- 员工 AI 与寻路（NavMesh）
- 势力好感
- 同业暗战（偷菜谱 / 造谣）
- 菜谱研发
- 风闻系统
- 多房型 / 多层
- 难度滑块
- 节日氛围
- 招聘 UI

---

## 4. 关键的"取舍时刻"要发生在前 3 分钟

demo 成败看一个瞬间：比如第一个文人因为没屏风住得不爽走了，玩家当场意识到——**"我摆的东西真的算数，而且空间不够我啥都摆"**。

这一下戳中，demo 就成了；如果玩家摆了半天发现怎么摆结果都一样，第二条标准就没立住。

---

## 5. 失败信号（开做之前先定死）

没有失败定义的 demo 等于白做。下列任一信号出现，说明玩法假设有问题，回去改：

| 信号 | 表现 | 说明什么 |
| :--- | :--- | :--- |
| **A 布置无后果** | 怎么摆结果都差不多，客人满不满意跟布局没关系 | 第二条没立住，布置系统本身要做重 |
| **B 不稀缺** | 钱要么永远花不完、要么卡死翻不了盘 | 第三条失败，经济没张力 |
| **C 循环不清** | 玩家不知道下一步该干嘛、在玩什么 | 第一条失败，核心循环没讲明白 |
| **D 反向（最阴）** | demo 好玩，但完全不靠布置，纯点按钮收钱就爽 | 核心其实不是"布置"，要重新想主轴 |

D 出现尤其要警惕：你以为的钩子不是真钩子，得重新定位核心。

---

## 6. 起步数值（全部 [PLACEHOLDER · 待 playtest 验证]）

以下仅为首版试跑值，每条带 rationale，实测后必调：

| 数值 | 建议起步 | rationale |
| :--- | :--- | :--- |
| 起始银两 | 够买 2–3 件家具 + 1 次小扩建 | 逼前 3 分钟就面临"先摆还是先扩" |
| 网格大小 | 约 8×8，摆满约 6–8 件 | 空间小才能逼冲突 |
| 客人耐心 | 没有对应标签房就走、扣钱 | 把"没布置对"直接变成丢收入 |
| 单局长度 | 1 个"天" ≈ 5–8 分钟，连跑 2–3 天看曲线 | 够短能快速重开试不同摆法 |

---

## 7. 建议流程

1. **纸上先跑**：写代码前，用纸或白板把"客人来 → 摆错房 → 走人 → 亏钱 → 下次调整"这一圈跑两遍，确认逻辑自洽。纸上改一天，build 里改一个月。
2. **只做第 2 节的内容**，严格排除第 3 节。
3. **开做前贴出第 5 节的失败信号**，playtest 时逐条对照。
4. 验活三条标准后，再回头决定下一个系统做什么（大概率回到策划案的阶段规划）。

---

## 8. 程序实施方案（架构映射）

> 目标：把第 2 节的玩法用最小代码量落到现有 `Assets/Script/` 架构上。
> 设计约束（来自 `AGENTS.md`）：SO 只存数据、MonoBehaviour 单一职责、复用既有摆放链路、UI 面板不碰网格/存档、UI 与摆放输入互斥。
> 既有代码问题见 `doc/architecture-analysis.md`（旋转 footprint / NRE / FindObjectOfType / 单例耦合），其中 P0 必须先修。

### 8.1 复用清单（v0.1 已有，尽量零改动）

| 已有模块 | 在 demo 里的角色 | 是否要改 |
| :--- | :--- | :--- |
| `GridSystem` | 网格吸附/占用；demo 设 `gridSize=(8,8)`、加 `entryCell` 与距离查询、扩展开关 | 小幅改 |
| `FurniturePlacer` | 拖拽/落位/删除 + 触发存档；demo 加"放置扣银两"与"旋转交换 footprint" | 改 |
| `PlacementPreview` | 购买预览 → 落位入口；demo 改注入引用，去掉 `FindObjectOfType` | 改 |
| `FurnitureDatabase` | 按 id/category 查家具；demo 直接用 | 不改 |
| `LayoutSaver` / `InnLayoutData` | 布局 JSON 存档；demo 复用 | 不改 |
| `UIManager` / `UIPanelBase` / `HUDPanel` / `DailyReportPanel` | HUD 显银两/需求/耐心、日结报表 | 小幅改 |
| `GameInputGate` / `GameInputMode` | Popup 打开时暂停世界输入 | 不改 |

### 8.2 必须新增的模块

**数据层（纯 SO / 可序列化）**
- `FurnitureTag`（枚举，英文命名，替换中文 `FurnitureCategory` 的用法）：`Lodging`(床/住宿)、`Quiet`(屏风→静修)、`Weapon`(兵器架→兵器)、`Kitchen`(灶台)、`Dining`(桌椅)。在 `FurnitureData` 上新增 `public List<FurnitureTag> providesTags;`（**不重命名既有字段**，向后兼容）。
- `GuestData : ScriptableObject`：`guestTypeId`、`displayName`、`roomFeeMultiplier`、`patience`(s)、`requiredTag`(FurnitureTag)、`failBehavior`(枚举：差评/离场扣钱/闹事扣分)。对应 tuning 的 3 种客人。
- `FloatVariable : ScriptableObject`（通用数值 SO，带 `OnValueChanged`）→ 实例 `SilverVariable`：银两唯一真源，HUD 订阅、经济系统只写它。**不持久化**（符合 ui-system.md："UI 不持久化"）。
- （可选）`InnConfig : ScriptableObject`：集中放 demo 数值（起始银两、房费基准、餐饮单价、扩建成本、静修距离 N、网格大小），从 `doc/demo_tuning.csv` 落地，方便 playtest 调参而不改代码。

**运行时（MonoBehaviour，单一职责）**
- `NeedMatcher`（纯静态逻辑类，无 MonoBehaviour，**可单测**）：`Matches(GuestData, IEnumerable<FurnitureTag> placedTags, int distToEntry)` → 依据 `requiredTag` + 静修距离条件返回是否满足；命中后算收入（`房费基准 × multiplier` + 餐饮 if 灶台&桌椅）。这是布置"算不算数"的核心，必须独立。
- `Guest : MonoBehaviour`：持 `GuestData`；耐心倒计时；到达时向 `InnDirector` 问"当前有无满足条件的家具组合"；满足→收钱(`SilverVariable +=`)、播满意度气泡；不满足且耐心耗尽→按 `failBehavior` 扣钱/差评并离场。
- `InnDirector : MonoBehaviour`：日循环状态机（错峰生成客人→逐个评估→日终触发 `DailyReportPanel`）。维护"今日收入/支出/满意度"供日结；Popup 打开时经 `GameInputGate` 自动暂停。

**UI**
- 扩展 `HUDPanel`：显示 `SilverVariable` 实时值 + 当前客人需求标签 + 耐心条 + 丢客原因浮字。
- `DailyReportPanel` 复用（已有），传入 `DailyReportData(day, income, expense, satisfaction)`。
- 购买/扩建入口：复用 `PlacementPreview.StartPlacement(furniture)` 作买家具入口；扩建做最简按钮（扣 `扩建成本` 并调 `GridSystem` 扩展开关）。不在 demo 做完整商店 UI。

### 8.3 前置修复（来自架构审计，P0 必须先做）

1. **旋转 footprint（P0）**：`FurniturePlacer.RotateFurniture`(:167-176) 旋转 90° 后，占用检测仍用固定 `(width,depth)`（`GetGridSizeFromData` :294-297）。修复：旋转时交换 `gridSize.x/y` 并重吸附到网格；`IsPositionAvailable`/`OccupyCells` 用旋转后尺寸。
2. **占用态 NRE（P0）**：`GridSystem.occupiedGrid` 在 `Start()` 前为 null（:13,28,72）。改在 `Awake()` 初始化或在使用处加空守卫。
3. **隐藏依赖（P1）**：`PlacementPreview` 用 `FindObjectOfType` 取 `GridSystem`/`FurniturePlacer`（:15、:84）。改为 Inspector 注入序列化引用（与 `FurniturePlacer` 已持有 `gridSystem` 引用一致）。demo 中 `PlacementPreview` 是购买入口，必须改干净。

### 8.4 一次"接待→收钱"的执行链路

1. 客人按 `InnConfig` 错峰生成，`Guest` 带 `GuestData` 进场，开始耐心倒计时。
2. `Guest` 问 `InnDirector.GetMatch(guest)` → `InnDirector` 收集 `FurniturePlacer` 当前已放置家具的 `providesTags` + 各家具到 `entryCell` 的距离 → 调 `NeedMatcher.Matches`。
3. 满足：算收入（`房费基准×multiplier` + 餐饮 if 灶台&桌椅）→ `SilverVariable.Value += 收入`；HUD 实时刷新；播满意度气泡。
4. 不满足且耐心耗尽：按 `failBehavior` 差评/扣钱/闹事；HUD 显示丢客原因。
5. 日终：`InnDirector` 汇总 → `UIManager.Open(PanelDailyReport, new DailyReportData(...))`；Popup 打开 → `InputMode=UIOnly` → 世界输入暂停（已有机制）。

> 第 2 步让"摆错/没摆"直接变成"收不到钱"，落地第 1.3 节第二条；第 3 步让银两流动可见，落地第三条。

### 8.5 实现顺序（带可验证里程碑）

- **Phase 0 — 前置修复**：修旋转 footprint + NRE + `PlacementPreview` 注入引用。里程碑：现有摆放/存档行为不变，旋转后占用正确、空场景不崩。
- **Phase 1 — 数据 + 标签匹配（纯逻辑）**：加 `FurnitureTag`、`FurnitureData.providesTags`、`GuestData`、`NeedMatcher` + 单测（构造 placedTags 断言 Match 结果）。里程碑：不进 Unity 也能跑通"文人需静修(屏风+距入口≥N)"判定。
- **Phase 2 — 客人 + 日循环**：`Guest`/`InnDirector` 接入 HUD/`DailyReport`；`entryCell` 距离判定生效。里程碑：跑通一天，文人因无屏风离场扣钱可见。
- **Phase 3 — 经济闭合**：放置扣银两（`AddFurniture` 扣 `price`，不足则拒）、扩建按钮（扣 `扩建成本` 并解锁网格）。里程碑：前 3 分钟被迫"先摆还是先扩"；信号 A/B/C 开始可观测。
- **Phase 4 — Playtest**：对照第 5 节失败信号 A/B/C/D 逐条验证；调 `InnConfig` 数值。

### 8.6 文件清单（预估）

**新增**
- `Assets/Script/FurnitureTag.cs`（枚举）
- `Assets/Script/GuestData.cs`（SO）
- `Assets/Script/NeedMatcher.cs`（纯逻辑 + 单测）
- `Assets/Script/Guest.cs`（MonoBehaviour）
- `Assets/Script/InnDirector.cs`（MonoBehaviour）
- `Assets/Script/Variables/FloatVariable.cs`（通用数值 SO）+ `SilverVariable.asset`
- `Assets/Script/Data/InnConfig.cs`（SO，集中数值）
- `Assets/Script/Tests/NeedMatcherTests.cs`（Editor 单测）

**修改**
- `FurnitureData.cs`：加 `providesTags`（不加不改既有字段）
- `FurniturePlacer.cs`：旋转 footprint、放置扣费、暴露 `IReadOnlyList<FurnitureItem> PlacedItems` 供查询
- `PlacementPreview.cs`：注入 `GridSystem`/`FurniturePlacer` 引用
- `GridSystem.cs`：空守卫、`entryCell`、`GetCellDistance`、`Expand` 接口
- `HUDPanel.cs` / `HUDData.cs`：扩展字段（银两订阅、需求、耐心）

> UI 目录按 `AGENTS.md` 实际路径 `Assets/Script/UI/`。

### 8.7 约定遵守（改动前 self-check）

- [ ] SO 仍只存数据（`GuestData`/`InnConfig`/`FloatVariable` 无场景态）
- [ ] 复用 `FurniturePlacer` 摆放链路，不重写射线/校验
- [ ] 任何改 Transform 的动作仍同步占用 + `LayoutSaver`（放置扣费只是附加，不破坏不变式）
- [ ] 不重命名既有序列化字段（只新增）
- [ ] UI 面板不直接改网格占用/`LayoutSaver`（经济只写 `SilverVariable`）
- [ ] `GameInputGate` 互斥保留

### 8.8 开放问题与决策点

- **静修"距大堂"**：demo 是单网格房间，无独立大堂。建议把逻辑原点左下角 `(0,0)` 设为 `entryCell`，曼哈顿距离 ≥ N 即满足；N 取 tuning 的 [PLACEHOLDER]。待 playtest 定。
- **床的并发上限**：demo "稀缺"靠有限床位。Phase 2 先做成"存在床即满足"，不在意同时住几人；若 playtest 显示不紧张，Phase 4 再加"每床占一个客人槽"的并发上限。
- **扩建实现**：最简 = `GridSystem` 增大 `gridSize`（保留已占用格重建 `occupiedGrid`）；进阶 = 预建大网格只解锁部分。demo 用最简。
- **单例解耦（P1）**：`UIManager.Instance` 单例在 demo 可暂不去掉（范围控制），但 `InnDirector` 应通过事件/SO 而非反向持单例与玩法层通信；列入后续重构。
