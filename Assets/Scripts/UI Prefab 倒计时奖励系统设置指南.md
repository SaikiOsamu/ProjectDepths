# UI Prefab 倒计时奖励系统设置指南

## 概述
升级后的ProgressBarController现在完全支持UI prefab的生成，能正确处理Canvas坐标系统和UI层级。

## 主要变更
✅ **UI prefab支持**：自动检测和处理UI prefab  
✅ **Canvas自动查找**：自动找到合适的Canvas作为父对象  
✅ **坐标转换**：世界坐标自动转换为UI坐标  
✅ **向下兼容**：仍然支持3D prefab  

## 设置步骤

### 1. ProgressBarController 配置

#### Prefab Spawning 设置
- `Reward Prefab`: 拖拽你的UI prefab（如+100.prefab）
- `Spawner Transform`: spawner位置标记（可选）
- `Target Canvas`: 目标Canvas（留空自动查找）
- `Is UI Prefab`: ✅勾选（默认已勾选）

### 2. 推荐设置

#### 自动模式（推荐）
1. 将`Is UI Prefab`保持勾选
2. 将`Target Canvas`留空（系统自动查找）
3. 拖拽你的UI prefab到`Reward Prefab`
4. 可选：设置`Spawner Transform`指定生成位置

#### 手动模式
1. 手动拖拽场景中的Canvas到`Target Canvas`
2. 其他设置同上

### 3. 位置控制

#### 有Spawner位置
- 系统会将Spawner的世界坐标转换为Canvas的UI坐标
- UI prefab会在对应位置生成

#### 无Spawner位置
- UI prefab会在Canvas中心生成
- 适合不需要特定位置的情况

## 工作原理

### UI prefab模式 (`Is UI Prefab` = true)
1. **Canvas查找**：自动查找Screen Space Overlay Canvas
2. **生成prefab**：在Canvas下创建UI prefab实例
3. **坐标转换**：世界坐标→屏幕坐标→UI坐标
4. **位置设置**：使用RectTransform.anchoredPosition

### 3D prefab模式 (`Is UI Prefab` = false)
1. **直接生成**：在世界空间直接创建prefab
2. **位置设置**：使用Transform.position

## 调试信息
系统会输出详细的调试信息：
- "Auto-selected Canvas: ..." - 自动选择的Canvas
- "Spawned UI prefab at canvas position: ..." - UI坐标位置
- "Spawned UI prefab at canvas center" - 在中心生成
- "No target canvas found for UI prefab!" - 找不到Canvas

## 常见问题

### Q: UI prefab生成位置不对？
A: 检查Canvas的渲染模式，推荐使用Screen Space - Overlay

### Q: prefab没有出现？
A: 检查Canvas的sorting order，确保UI prefab在正确的层级

### Q: 坐标转换不准确？
A: 确保Camera.main指向正确的相机

### Q: 想要固定在屏幕某个位置？
A: 将`Spawner Transform`留空，prefab会在Canvas中心生成，然后手动调整prefab的锚点

## 测试功能
- **按F键**：立即测试生成UI prefab
- **30秒倒计时**：正常触发（加分+生成prefab）

## 兼容性
- ✅ 现有的+100.prefab（UI prefab）
- ✅ 任何自定义UI prefab  
- ✅ 3D prefab（设置`Is UI Prefab` = false）
- ✅ 所有现有的Canvas设置

## 推荐配置示例
```
Reward Prefab: +100.prefab
Spawner Transform: 留空或指定位置
Target Canvas: 留空（自动查找）
Is UI Prefab: ✅勾选
Enable Test Key: ✅勾选
Test Key: F
```

现在你的UI prefab应该能正确生成在Canvas中了！ 