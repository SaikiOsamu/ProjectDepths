# ButtonManager UI Prefab 系统设置指南

## 概述
升级后的ButtonManager现在支持在密码输入结果时生成UI prefab：
- **正确输入**：生成rewardPrefab (奖励UI)
- **错误输入**：生成punishPrefab (惩罚UI)  
- **超时过期**：生成punishPrefab (惩罚UI)

## 主要功能特性
✅ **双prefab支持**：分别配置奖励和惩罚prefab  
✅ **UI prefab优化**：完全支持Canvas坐标系统  
✅ **自动Canvas检测**：智能查找合适的Canvas  
✅ **测试功能**：按键快速测试效果  
✅ **向下兼容**：支持3D prefab（可选）

## Inspector配置

### Prefab Spawning 设置
```
[Prefab Spawning]
├── Reward Prefab: 正确时生成的UI prefab
├── Punish Prefab: 错误/过期时生成的UI prefab  
├── Spawner Transform: 生成位置标记（可选）
├── Target Canvas: 目标Canvas（留空自动查找）
└── Is UI Prefab: ✅勾选（默认）
```

### Debug/Test 设置
```
[Debug/Test]
├── Enable Test Key: ✅启用测试按键
├── Test Reward Key: R（测试奖励prefab）
└── Test Punish Key: T（测试惩罚prefab）
```

## 设置步骤

### 1. 基础配置
1. **拖拽rewardPrefab**：正确输入时显示的UI（如+100分数显示）
2. **拖拽punishPrefab**：错误/过期时显示的UI（如-50分数显示）
3. **保持`Is UI Prefab`勾选**（默认已勾选）
4. **留空`Target Canvas`**让系统自动查找

### 2. 位置控制（可选）
- **有Spawner位置**：UI prefab会在转换后的坐标位置生成
- **无Spawner位置**：UI prefab会在Canvas中心生成

### 3. 推荐的prefab类型
- **奖励prefab**：绿色+100文字、成功动画、钱币图标等
- **惩罚prefab**：红色警告文字、错误动画、骷髅图标等

## 触发时机

### 🎯 CorrectInput() - 生成rewardPrefab
- 玩家正确输入3位密码
- 同时执行：加100分 + 生成奖励UI
- 播放成功音效和动画

### ❌ WrongInput() - 生成punishPrefab  
- 玩家输入错误数字
- 同时执行：惩罚 + 生成惩罚UI
- 播放错误音效和动画

### ⏰ ExpiredInput() - 生成punishPrefab
- 密码输入超时（默认5秒）
- 同时执行：惩罚 + 生成惩罚UI
- 播放错误音效和动画

## 测试功能

### 快速测试按键
- **按R键**：立即生成rewardPrefab（模拟正确输入）
- **按T键**：立即生成punishPrefab（模拟错误/过期）
- **实时验证**：无需等待密码事件触发

### 调试信息
控制台会显示详细信息：
```
"ButtonManager: Auto-selected Canvas: ..."
"Spawned UI prefab 'RewardPrefab' at canvas position: ..."
"Test reward key pressed! Spawning reward prefab..."
```

## 工作流程

```
密码事件触发
    ↓
玩家输入数字
    ↓
判断结果
    ├── 正确 → CorrectInput() → 加分 + 生成rewardPrefab
    ├── 错误 → WrongInput() → 惩罚 + 生成punishPrefab
    └── 超时 → ExpiredInput() → 惩罚 + 生成punishPrefab
```

## 与ProgressBarController的区别

| 特性 | ProgressBarController | ButtonManager |
|------|---------------------|---------------|
| **触发方式** | 定时30秒 | 密码输入结果 |
| **prefab数量** | 1个（奖励） | 2个（奖励+惩罚） |
| **触发频率** | 固定间隔 | 事件驱动 |
| **测试按键** | F键 | R键+T键 |

## 推荐配置示例

### 基础配置
```
Reward Prefab: +100ScorePrefab
Punish Prefab: DangerWarningPrefab
Spawner Transform: 留空（Canvas中心）
Target Canvas: 留空（自动查找）
Is UI Prefab: ✅勾选
```

### 测试配置
```
Enable Test Key: ✅勾选
Test Reward Key: R
Test Punish Key: T
```

## 常见问题

### Q: prefab生成位置不对？
A: 检查Canvas渲染模式，推荐Screen Space - Overlay

### Q: 想要不同的生成位置？
A: 设置不同的`Spawner Transform`，或者修改prefab的锚点设置

### Q: 如何禁用测试按键？
A: 取消勾选`Enable Test Key`

### Q: 支持3D prefab吗？
A: 支持，取消勾选`Is UI Prefab`即可

## 兼容性
- ✅ 所有现有密码输入逻辑
- ✅ 音效系统
- ✅ 分数系统
- ✅ GameManager.Punish()方法
- ✅ 现有的Canvas设置

现在ButtonManager完全支持UI prefab生成了！使用R和T键可以快速测试效果。 