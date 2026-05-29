# AGENTS 指南（hotel_unity）

## 项目概览

该仓库是一个 Unity 项目，核心目标是实现客栈场景中的网格化家具摆放系统。

当前顶层结构：

- `Assets/`：游戏资源与玩法脚本。
- `ProjectSettings/`：Unity 工程级配置资源。
- `.vscode/`：本地编辑器与调试配置。
- `hotel_unity.slnx`：C# 工具链使用的解决方案文件（自动生成）。

## 代码组织方式

当前玩法代码主要在 `Assets/script/`，相机控制在 `Assets/Script/`，并且已经形成较清晰的职责分层：

- **摆放与交互逻辑**
  - `GridSystem.cs`：网格吸附、占用检测、占用写入。
  - `FurniturePlacer.cs`：拖拽/移动/落位/删除流程，状态切换与存档触发。
  - `PlacementPreview.cs`：购买时预览流程（预览物跟随/校验/放置/取消）。
- **相机控制**
  - `CameraController.cs`（`Assets/Script/`）：`WASD` 平移、`Q/E` 水平旋转、滚轮缩放；旋转时绕当前视角中心点公转。
- **数据模型**
  - `FurnitureData.cs`：家具定义（`ScriptableObject`）。
  - `FurnitureDatabase.cs`：家具库索引与按分类查询。
  - `FurnitureItem.cs`：运行时实例与 `FurnitureData` 的绑定组件。
  - `InnLayoutData.cs`：可序列化布局模型（`InnLayoutData` + `PlacedFurniture`）。
- **持久化**
  - `LayoutSaver.cs`：基于 JSON 的保存/读取/删除入口，路径在 `Application.persistentDataPath` 下。

## 当前运行链路

1. 玩家进入新建摆放或拖拽已有家具流程。
2. 位置通过 `GridSystem.SnapToGrid` 吸附到网格点（基于左下角原点换算）。
3. 通过 `GridSystem.IsPositionAvailable` 校验可用性。
4. 依据校验结果切换材质，提供可视化反馈。
5. 确认放置后，由 `FurniturePlacer` 更新占用格并调用 `LayoutSaver` 持久化。
6. 调试时 `GridSystem.OnDrawGizmos` 会绘制网格线，并在每个格子中心显示 `(x,z)` 坐标（左下角为 `(0,0)`）。

## 需要保持的约定

- `ScriptableObject` 类保持为纯数据容器，避免引入场景态行为。
- `MonoBehaviour` 类聚焦单一运行时职责（网格、放置、预览等）。
- 任何会改变家具 Transform 的功能，都必须同步维护网格占用与存档数据。
- 优先复用既有摆放链路，不重复实现射线检测与可放置校验逻辑。
- 公共 Inspector 字段默认保持稳定，如需改名要附带迁移方案。
- 网格坐标约定统一以左下角为逻辑原点；显示与判定必须使用同一坐标基准。

## 本仓库代理改动规则

在此项目中新增或修改代码时，遵循以下规则：

1. **遵守 Unity 序列化约束**
   - 不要随意重命名已序列化的公共字段。
   - 优先采用增量式改动；若必须重命名，需提供兼容/迁移说明。
2. **保持摆放不变式**
   - 任何放置动作必须同时更新占用状态与持久化数据。
   - 取消/回退路径必须正确恢复占用状态。
3. **复用现有数据契约**
   - `furnitureId` 是主要检索键。
   - `PlacedFurniture` 是运行时实例的持久化单元。
4. **保持文件归类一致**
   - 运行时摆放逻辑统一放在 `Assets/script/`。
   - 相机控制逻辑统一放在 `Assets/Script/`（注意目录大小写）。
   - 新增数据类按用途选择可序列化类或 `ScriptableObject`。
5. **改动后验证**
   - 检查修改脚本的编译错误。
   - 若改动摆放流程，至少验证：移动、旋转、取消、保存、重载。
   - 若改动网格可视化，确认 Gizmos 与格子坐标显示正常。
   - 若改动相机，确认 `WASD/QE/滚轮` 均生效，且旋转枢轴符合预期。

## 后续可选重构建议

- 在 `Assets/script/` 下引入子目录：
  - `Placement/`（`GridSystem`、`FurniturePlacer`、`PlacementPreview`）
  - `Data/`（`FurnitureData`、`FurnitureDatabase`、`InnLayoutData`）
  - `Persistence/`（`LayoutSaver`）
  - `Runtime/`（`FurnitureItem`）

该重构可提升后续扩展性，同时不改变现有运行行为。

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **hotel_unity** (376 symbols, 558 relationships, 15 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/hotel_unity/context` | Codebase overview, check index freshness |
| `gitnexus://repo/hotel_unity/clusters` | All functional areas |
| `gitnexus://repo/hotel_unity/processes` | All execution flows |
| `gitnexus://repo/hotel_unity/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
