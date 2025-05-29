# Boss系统设置指南

## 概述
Boss系统由三个主要脚本组成：
- `BossManager.cs` - 管理Boss生成和永久存在逻辑、动态活动区域设置
- `Boss.cs` - Boss行为逻辑（独立X轴移动、边移动边攻击、双手开火模式）
- `BossBullet.cs` - 弹幕行为（简化版，击中玩家直接死亡，碰到BottomLine自动消失）

## 功能特点
- **游戏开始即存在**：Boss在游戏开始时立即生成，无需等待
- **永久存在**：Boss不会自动销毁，持续威胁玩家
- **智能重生**：如果Boss意外被销毁，系统会自动重新生成
- **手动控制**：支持通过代码手动销毁Boss（如玩家死亡时）
- **动态活动区域**：相对于玩家位置的活动区域，自动跟随玩家移动
- **摄像机视野保持**：Boss始终保持在摄像机视野内，不会因玩家下落而脱离
- **平滑跟随**：Boss的Y轴平滑跟随玩家移动，避免生硬的瞬间跳跃
- **独立X轴移动**：Boss有自己的水平移动模式，不跟随玩家X轴位置
- **边移动边攻击**：Boss在左右移动过程中持续发射弹幕攻击玩家
- **边界停留机制**：Boss到达左右边界时停留3秒，然后改变方向
- **双手开火**：支持左手和右手两个开火点，可配置不同开火模式和弹幕类型
- **致命弹幕**：弹幕击中玩家立即触发死亡，无击中特效
- **弹幕边界清理**：弹幕碰到BottomLine时自动消失，避免永久存在
- **实时可视化**：活动区域随玩家移动实时更新显示

## 动态活动区域系统

### 相对位置设计理念
传统的固定活动区域会导致Boss在玩家下落时脱离摄像机视野。新的动态活动区域系统解决了这个问题：

- **问题**：玩家不断下落 + 摄像机跟随 = Boss脱离视野
- **解决方案**：活动区域相对玩家位置 = Boss始终在视野内

### 偏移量设置（相对于玩家位置）
- **左偏移** (`leftOffset`): Boss可活动的最左侧相对玩家的偏移（如-8）
- **右偏移** (`rightOffset`): Boss可活动的最右侧相对玩家的偏移（如+8）  
- **上偏移** (`topOffset`): Boss可活动的最上方相对玩家的偏移（如+15）
- **下偏移** (`bottomOffset`): Boss可活动的最下方相对玩家的偏移（如+5）

### 动态计算公式
```
实际边界 = 玩家位置 + 偏移量
左边界 = 玩家X + 左偏移(-8) = 玩家左侧8单位
右边界 = 玩家X + 右偏移(+8) = 玩家右侧8单位
上边界 = 玩家Y + 上偏移(+15) = 玩家上方15单位
下边界 = 玩家Y + 下偏移(+5) = 玩家上方5单位
```

### 实时跟随特性
- **智能更新**：活动区域每帧根据玩家位置重新计算
- **无缝跟随**：玩家移动时Boss活动区域同步移动
- **视野保持**：确保Boss始终在摄像机可见范围内
- **攻击位置调整**：攻击位置也相对玩家动态计算

### BossManager动态可视化
```
玩家下落过程中的活动区域变化：

时间T1（玩家在Y=10）:
    ┌─────────────────────────┐ Y=25 (10+15)
    │        🔵             │   
    │       玩家              │   🔵=玩家 🟢=Boss生成点
    │        🟢             │   
    │      [Boss区域]        │
    └─────────────────────────┘ Y=15 (10+5)

时间T2（玩家在Y=0，下落了10单位）:
    ┌─────────────────────────┐ Y=15 (0+15)
    │        🔵             │   
    │       玩家              │   活动区域自动下移10单位
    │        🟢             │   
    │      [Boss区域]        │
    └─────────────────────────┘ Y=5 (0+5)
```

## 活动区域系统

### 统一管理设计
- **场景级配置**：活动区域在BossManager中统一设置，避免重复配置
- **自动传递**：BossManager生成Boss时自动传递活动区域参数
- **可视化预览**：在BossManager上查看和调整活动区域边界
- **向下兼容**：支持不使用活动区域的传统模式

### 边界设置
- **左边界** (`leftBoundary`): Boss可活动的最左侧位置
- **右边界** (`rightBoundary`): Boss可活动的最右侧位置  
- **上边界** (`topBoundary`): Boss可活动的最上方位置
- **下边界** (`bottomBoundary`): Boss可活动的最下方位置

### 功能特性
- **智能限制**：Boss移动和跟随玩家时自动受边界约束
- **攻击位置调整**：如果攻击位置超出边界，自动调整到边界内
- **退场特殊处理**：Boss退场时可以超出右边界
- **实时验证**：自动检查边界设置和攻击位置是否合理
- **可视化显示**：Scene视图中显示黄色边界框和绿色生成点

### BossManager边界可视化
```
    ┌─────────────────────────┐ ← topBoundary
    │                        │   
    │        🟢             │   (绿色圆圈 = Boss生成点)
    │    Boss生成点          │
    │                        │
    │      [Boss区域]        │
    │                        │
    └─────────────────────────┘ ← bottomBoundary
  ↑                         ↑
leftBoundary            rightBoundary
```

## 双手开火系统

### 不同弹幕类型支持
- **左手弹幕** (`leftBulletPrefab`): 左手专用的弹幕预制体
- **右手弹幕** (`rightBulletPrefab`): 右手专用的弹幕预制体
- **独立速度控制**: 左右手弹幕可以设置不同的移动速度

### 射击方向配置
- **垂直射击** (`leftBulletVertical` / `rightBulletVertical`): 弹幕垂直向下射击
- **斜向射击**: 弹幕指向玩家方向，并添加随机角度偏移
- **角度偏移** (`diagonalAngleOffset`): 斜向弹幕的随机角度范围（±度数）

### 开火模式配置
- **交替开火** (`alternatingFire = true`): 左右手轮流开火，使用各自的弹幕类型
- **同时开火** (`simultaneousFire = true`): 双手同时开火，同时发射两种不同弹幕
- **随机开火** (两者都为false): 随机选择左手或右手开火

### 开火点设置
- `leftFirePoint`: 左手开火位置（建议放在Boss左下方）
- `rightFirePoint`: 右手开火位置（建议放在Boss右下方）

### 弹幕参数设置
- `leftBulletSpeed`: 左手弹幕移动速度
- `rightBulletSpeed`: 右手弹幕移动速度
- `leftBulletVertical`: 左手弹幕是否垂直向下（推荐：true）
- `rightBulletVertical`: 右手弹幕是否垂直向下（推荐：false）
- `diagonalAngleOffset`: 斜向弹幕的角度偏移范围（推荐：15度）

## 设置步骤

### 1. 创建Boss弹幕预制体（两种类型）

#### 创建左手弹幕（垂直型）
1. 在场景中创建一个新的GameObject，命名为"LeftBossBullet"
2. 添加以下组件：
   - `SpriteRenderer` - 设置弹幕的视觉外观（建议用圆形或方块，颜色如红色）
   - `Rigidbody2D` - 设置为Kinematic（可选）
   - `Collider2D` - 用于碰撞检测（建议用CircleCollider2D）
   - `BossBullet` 脚本
3. 设置Collider为Trigger
4. 将其拖拽到Prefab文件夹中创建预制体，命名为"LeftBulletPrefab"
5. 删除场景中的临时对象

#### 创建右手弹幕（斜向型）
1. 在场景中创建一个新的GameObject，命名为"RightBossBullet"
2. 添加以下组件：
   - `SpriteRenderer` - 设置弹幕的视觉外观（建议用三角形或箭头，颜色如蓝色）
   - `Rigidbody2D` - 设置为Kinematic（可选）
   - `Collider2D` - 用于碰撞检测（建议用CircleCollider2D）
   - `BossBullet` 脚本
3. 设置Collider为Trigger
4. 将其拖拽到Prefab文件夹中创建预制体，命名为"RightBulletPrefab"
5. 删除场景中的临时对象

### 2. 创建Boss预制体
1. 在场景中创建一个新的GameObject，命名为"Boss"
2. 添加以下组件：
   - `SpriteRenderer` - Boss的视觉外观
   - `Boss`脚本
3. 在Boss下创建两个子对象作为开火点：
   - "LeftFirePoint" - 左手开火位置
   - "RightFirePoint" - 右手开火位置
   - 位置应该在Boss下方左右两侧合适的位置
4. 在Boss脚本中设置：
   - `Left Bullet Prefab` = LeftBulletPrefab预制体
   - `Right Bullet Prefab` = RightBulletPrefab预制体
   - `Left Fire Point` = LeftFirePoint子对象的Transform
   - `Right Fire Point` = RightFirePoint子对象的Transform
   - `Left Bullet Speed` = 左手弹幕速度（建议4-6）
   - `Right Bullet Speed` = 右手弹幕速度（建议4-6）
   - `Left Bullet Vertical` = true（垂直向下）
   - `Right Bullet Vertical` = false（斜向射击）
   - `Diagonal Angle Offset` = 15（斜向弹幕角度偏移）
   - 配置开火模式（见下面的参数说明）
5. 将Boss拖拽到Prefab文件夹中创建预制体
6. 删除场景中的临时对象

### 3. 设置BossManager（重要：活动区域在这里配置）
1. 在主游戏场景中创建一个空的GameObject，命名为"BossManager"
2. 添加`BossManager`脚本
3. 在脚本中设置：
   - `Boss Prefab` = 刚才创建的Boss预制体
   - `Player` = 场景中的玩家对象（或保持空，会自动查找"Player"标签）
   - **配置活动区域**：设置左右上下边界
   - 调整其他参数如生成间隔、位置偏移等

### 4. 确保玩家标签正确
确保玩家对象有"Player"标签，这样弹幕才能正确检测碰撞。

## 参数详细说明

### BossManager参数
- `bossSpawnInterval` - Boss生成间隔（45秒）
- `bossDuration` - Boss存在时间（15秒）
- `bossYOffset` - Boss相对玩家的Y轴偏移（建议8-10）
- `spawnX` - Boss生成相对玩家的X位置偏移（建议0，即玩家正上方）

#### 动态活动区域设置
- `useActivityArea` - 是否启用活动区域限制（推荐：true）
- `leftOffset` - 左边界相对玩家的偏移（建议：-8）
- `rightOffset` - 右边界相对玩家的偏移（建议：8）
- `topOffset` - 上边界相对玩家的偏移（建议：15）
- `bottomOffset` - 下边界相对玩家的偏移（建议：5）
- `showBoundaries` - 是否在Scene视图显示动态边界（推荐：true）

#### Boss生存管理
- Boss默认为永久存在，不会自动销毁
- 可通过`BossManager.Instance.DestroyCurrentBoss()`手动销毁
- 可通过`BossManager.Instance.RestartBossSystem()`重新生成Boss
- 系统会自动检测Boss丢失并重新生成

### Boss参数

#### 移动设置
- `moveSpeed` - Boss移动速度（建议3-5）
- `followSmoothness` - Y轴跟随的平滑度（建议1-5，数值越大跟随越快）
- `waitTimeAtEdge` - 在左右边界停留的时间（默认3秒）

#### 攻击设置
- `bulletPrefab` - 弹幕预制体
- `leftFirePoint` - 左手开火点Transform
- `rightFirePoint` - 右手开火点Transform
- `bulletsPerAttack` - 每次攻击发射的弹幕数量（仅用于特殊攻击）
- `timeBetweenBullets` - 弹幕之间的间隔（仅用于特殊攻击）
- `timeBetweenAttacks` - 移动时攻击间隔（建议0.5-2秒）
- `bulletSpeed` - 弹幕移动速度（建议4-6）

#### 开火模式设置
- `alternatingFire` - 是否交替开火（推荐：true）
- `simultaneousFire` - 是否同时开火（高难度：true）

### BossBullet参数
- `speed` - 弹幕移动速度
- `lifetime` - 弹幕生存时间（防止永久存在，建议10秒）
- `hitPlayerSound` - 击中玩家音效

### BossBullet碰撞系统
- **玩家碰撞**：弹幕击中玩家立即触发死亡，播放击中音效
- **BottomLine碰撞**：弹幕碰到BottomLine（游戏边界）时自动消失
- **生存时间限制**：弹幕在10秒后自动销毁
- **屏幕外销毁**：弹幕离开屏幕可见范围时自动销毁
- **无击中特效**：简化设计，击中后立即销毁，无视觉特效

## 推荐配置

### 基础配置（适合新手）
```
Boss Y轴偏移: 8
Boss移动速度: 3
Y轴跟随平滑度: 2
边界停留时间: 3秒
移动攻击间隔: 1.5秒
动态活动区域: 左-6, 右+6, 上+12, 下+6 (相对玩家)
左手弹幕速度: 4 (垂直向下)
右手弹幕速度: 4 (斜向)
斜向角度偏移: 10度
开火模式: 交替开火
```

### 困难配置（适合挑战）
```
Boss Y轴偏移: 10
Boss移动速度: 4
Y轴跟随平滑度: 3
边界停留时间: 2秒
移动攻击间隔: 1秒
动态活动区域: 左-8, 右+8, 上+15, 下+5 (相对玩家)
左手弹幕速度: 5 (垂直向下)
右手弹幕速度: 6 (斜向)
斜向角度偏移: 15度
开火模式: 同时开火
```

### 专家配置（极限挑战）
```
Boss Y轴偏移: 12
Boss移动速度: 5
Y轴跟随平滑度: 4
边界停留时间: 1.5秒
移动攻击间隔: 0.7秒
动态活动区域: 左-10, 右+10, 上+18, 下+4 (相对玩家)
左手弹幕速度: 6 (垂直向下)
右手弹幕速度: 7 (斜向)
斜向角度偏移: 20度
开火模式: 随机开火
```

## 动态活动区域设计建议

### 偏移量设计原则
- **水平范围**：建议左右各8-10单位，给Boss足够的横向移动空间
- **垂直范围**：上偏移12-18单位，下偏移4-6单位，确保Boss在玩家上方
- **摄像机适配**：确保偏移量不超过摄像机视野范围

### 相对位置优化
```
推荐偏移配置：
- 左偏移: -8 (玩家左侧8单位)
- 右偏移: +8 (玩家右侧8单位)  
- 上偏移: +15 (玩家上方15单位)
- 下偏移: +5 (玩家上方5单位)
```

### 攻击位置相对化
- **左攻击位置**：相对玩家-5单位（leftAttackX = -5）
- **右攻击位置**：相对玩家+5单位（rightAttackX = 5）
- **自动适配**：攻击位置自动跟随玩家移动

## 弹幕类型对比

| 弹幕类型 | 射击方向 | 特点 | 难度 | 视觉效果 | 推荐配置 |
|----------|----------|------|------|----------|----------|
| **左手弹幕** | 垂直向下 | 可预测，稳定威胁 | 中等 | 整齐规律 | 红色圆形，速度适中 |
| **右手弹幕** | 斜向追踪 | 不可预测，动态追踪 | 困难 | 变化丰富 | 蓝色箭头，随机偏移 |

### 射击方向详解
- **垂直射击**：弹幕严格垂直向下，玩家可以通过水平移动躲避
- **斜向射击**：弹幕指向玩家位置，并添加±角度偏移，增加不确定性

### 组合效果
- **交替模式**：左右手轮流，节奏感强，给玩家思考时间
- **同时模式**：双重威胁，垂直+斜向同时攻击，压力极大
- **随机模式**：不可预测，增加游戏变化性

## 开火模式对比

| 模式 | 特点 | 难度 | 视觉效果 | 推荐场景 |
|------|------|------|----------|----------|
| **交替开火** | 左右手轮流，有节奏感 | 中等 | 动感十足 | 通用，最佳平衡 |
| **同时开火** | 双弹幕，密度翻倍 | 困难 | 震撼强烈 | 高难度挑战 |
| **随机开火** | 不可预测，随机性强 | 中高 | 变化丰富 | 增加不确定性 |

## 音效设置
系统支持以下音效（需要在AudioManager中配置）：
- "BossAppear" - Boss出现音效（在BossManager中配置）
- "BossAttack" - Boss攻击音效（在BossBullet中配置，每个弹幕生成时播放）
- "BulletHitPlayer" - 弹幕击中玩家音效（在BossBullet中配置）

### 音效职责分工
- **BossManager**: 负责Boss生成相关音效
- **BossBullet**: 负责弹幕相关音效（发射音效和击中音效）
- **Boss**: 不再处理音效，专注于行为逻辑

### 音效触发时机
- **BossAppear**: Boss生成时触发（SpawnBoss()）
- **BossAttack**: 每个弹幕生成时触发（BossBullet.Start()）
- **BulletHitPlayer**: 弹幕击中玩家时触发（碰撞检测时）

## Boss行为流程
1. **出现**：Boss在玩家上方中央位置生成
2. **移动到左侧**：Boss移动到屏幕左侧攻击位置
3. **左侧攻击**：根据配置的开火模式从左手/右手/双手发射弹幕
4. **移动到右侧**：Boss移动到屏幕右侧攻击位置  
5. **右侧攻击**：根据配置的开火模式发射弹幕
6. **退场**：Boss向屏幕右侧移动直到消失

## 开火点位置建议

### 左右开火点放置
```
    [Boss精灵]
   /           \
LeftFire    RightFire
```

### 推荐相对位置
- **左开火点**: (-1, -1, 0) 相对于Boss
- **右开火点**: (1, -1, 0) 相对于Boss
- **垂直偏移**: -1到-2单位（在Boss下方）
- **水平间距**: 2单位（左右各1单位）

## 测试功能

### 快速生成Boss
- **按G键**：立即生成Boss，无需等待45秒间隔
- **实时测试**：可以快速测试Boss的行为和攻击模式
- **动态边界测试**：验证Boss是否正确跟随玩家移动
- **摄像机跟随测试**：确认Boss始终保持在视野内

### 动态可视化调试
- **实时边界显示**：黄色矩形随玩家移动实时更新
- **多重标记**：蓝色玩家位置、绿色Boss生成点
- **文字标签**：显示"Boss活动区域(跟随玩家)"等信息
- **运行时预览**：即使在Play模式下也能看到动态边界

### 测试流程
1. 在BossManager中设置相对偏移量和生成间隔
2. 运行游戏
3. 移动玩家位置观察活动区域跟随
4. 按G键立即生成Boss
5. 观察Boss的左右移动模式和边界停留
6. 测试Boss的持续攻击功能
7. 验证15秒后Boss是否自动销毁
8. 调整BossManager和Boss中的参数
9. 重新测试验证效果

### 控制台信息
```
"Boss活动区域偏移设置: 左-8, 右8, 上15, 下5 (相对玩家位置)"
"Boss initialized with relative activity area: 偏移 L-8, R8, T15, B5"
"Boss lifetime set to 15 seconds"
"Boss changed state to: WaitingAtRight"
"Boss lifetime expired, destroying..."
```

## 测试建议
- **验证即时生成**：启动游戏后Boss应立即出现
- **测试永久存在**：Boss应持续攻击，不会自动消失
- **重点测试动态跟随**：让玩家下落并观察Boss是否保持在视野内
- **在BossManager中设置偏移**：统一管理Boss的相对活动区域
- **验证动态边界限制**：测试Boss是否正确遵守相对活动区域
- **检查相对攻击位置**：确保Boss能正常到达相对的左右攻击位置
- 先使用交替开火模式测试基本功能
- 确保两个开火点的弹幕都能正确指向玩家
- 测试不同开火模式的视觉效果和难度
- 验证弹幕碰撞检测在两个开火点都正常工作
- **测试玩家移动场景**：确认Boss跟随玩家Y轴移动
- **测试智能重生**：手动删除Boss对象，观察系统是否自动重新生成
- **运行时边界预览**：利用动态Gizmos在Scene视图中观察实时边界
- **测试手动销毁**：调用`BossManager.Instance.DestroyCurrentBoss()`验证手动控制功能

## 注意事项
- **动态活动区域的核心价值**：解决玩家下落时Boss脱离摄像机视野的问题
- **独立X轴移动的重要性**：Boss有自己的攻击模式，不受玩家水平位置影响
- **平滑跟随的重要性**：提升游戏体验，避免Boss移动时的视觉突兀感
- **边移动边攻击设计**：增加游戏挑战性和压迫感
- **所有边界都是相对的**：偏移量相对玩家位置，不是固定世界坐标
- Boss会平滑跟随玩家的Y轴位置，X轴独立移动
- 弹幕击中玩家会立即触发死亡，无视觉特效
- **弹幕边界清理机制**：弹幕碰到BottomLine时自动销毁，确保不会永久存在
- 玩家死亡时会自动停止Boss生成
- **BossManager偏移验证**：系统会检查偏移设置是否合理
- **Boss生存时间管理**：15秒后自动销毁，避免永久存在
- **攻击间隔调节**：根据游戏难度调整攻击频率
- **边界停留时间**：可调整Boss在边界的停留时间
- **G键测试不受游戏状态限制**：即使在攻击间隔内也可生成
- 确保两个开火点都正确分配，否则会有警告提示
- 建议开火点位置不要重叠，保持一定距离
- **活动区域应适合移动模式**：确保左右偏移给Boss足够的移动空间
- 同时开火模式会增加弹幕密度，需要谨慎平衡难度
- **测试时可能生成多个Boss**：注意性能影响
- **Boss预制体无需活动区域配置**：所有参数由BossManager动态传递
- **摄像机跟随兼容**：系统设计完全兼容摄像机跟随玩家的机制
- **独立移动不受玩家影响**：Boss的水平移动模式完全独立于玩家位置

## 扩展建议
- 可以为不同开火点设置不同的子弹类型
- 可以添加开火点的视觉特效（如火花、光芒）
- 可以根据Boss状态动态切换开火模式
- 可以添加开火点的瞄准延迟，增加预判要素
- 可以为左右手添加不同的攻击音效

## 平滑跟随系统

### 设计理念
传统的直接位置设置会导致Boss跟随玩家时出现生硬的瞬间跳跃。新的平滑跟随系统使用插值算法，让Boss的移动更加自然流畅。

### 平滑度参数说明
- **`followSmoothness`**: 控制Boss跟随玩家的平滑程度
  - **数值范围**: 0.5 - 10
  - **推荐值**: 2-3 (平衡流畅性和响应性)
  - **数值越小**: 跟随越慢，更平滑但反应较慢
  - **数值越大**: 跟随越快，响应更灵敏但可能略显生硬

### 不同平滑度效果对比
| 数值 | 效果 | 适用场景 | 描述 |
|------|------|----------|------|
| **0.5-1** | 非常平滑 | 悠闲模式 | Boss缓慢跟随，营造威严感 |
| **1.5-2.5** | 平滑自然 | 推荐设置 | 最佳平衡，既流畅又响应 |
| **3-5** | 快速响应 | 挑战模式 | 跟随较快，增加压迫感 |
| **5+** | 近似瞬移 | 高难度 | 几乎瞬间跟随，高压力 |

### 技术实现
```csharp
// 平滑插值算法
float smoothedY = Mathf.Lerp(currentPos.y, targetY, followSmoothness * Time.deltaTime);
```

### 混合移动策略
- **Y轴（垂直）**: 使用平滑插值跟随玩家
- **X轴（水平）**: 使用线性移动到攻击位置
- **边界限制**: X轴硬性限制，Y轴在插值过程中应用限制

### 平滑跟随测试
- **渐进测试**：从低平滑度(1)开始逐渐增加，观察效果变化
- **玩家移动测试**：让玩家快速上下移动，观察Boss跟随效果
- **边界测试**：测试Boss在活动区域边界时的平滑行为
- **下落测试**：重点测试玩家持续下落时Boss的跟随流畅度

## 注意事项
- **动态活动区域的核心价值**：解决玩家下落时Boss脱离摄像机视野的问题
- **平滑跟随的重要性**：提升游戏体验，避免Boss移动时的视觉突兀感
- **所有边界都是相对的**：偏移量相对玩家位置，不是固定世界坐标
- Boss会平滑跟随玩家的位置，活动区域实时更新
- 弹幕击中玩家会立即触发死亡，无视觉特效
- **弹幕边界清理机制**：弹幕碰到BottomLine时自动销毁，确保不会永久存在
- 玩家死亡时会自动停止Boss生成
- **BossManager偏移验证**：系统会检查偏移设置是否合理
- **Boss攻击位置自动计算**：攻击位置相对玩家位置动态计算
- **动态边界自动调整**：Boss始终被限制在相对玩家的区域内
- **平滑度调节建议**：根据游戏节奏调整，快节奏游戏用较高数值
- **G键测试不受游戏状态限制**：即使在攻击间隔内也可生成
- 确保两个开火点都正确分配，否则会有警告提示
- 建议开火点位置不要重叠，保持一定距离
- **相对位置设计**：确保leftAttackX和rightAttackX在合理的相对范围内
- 同时开火模式会增加弹幕密度，需要谨慎平衡难度
- **测试时可能生成多个Boss**：注意性能影响
- **Boss预制体无需活动区域配置**：所有参数由BossManager动态传递
- **摄像机跟随兼容**：系统设计完全兼容摄像机跟随玩家的机制
- **独立移动不受玩家影响**：Boss的水平移动模式完全独立于玩家位置

## 扩展建议
- 可以为不同开火点设置不同的子弹类型
- 可以添加开火点的视觉特效（如火花、光芒）
- 可以根据Boss状态动态切换开火模式
- 可以添加开火点的瞄准延迟，增加预判要素
- 可以为左右手添加不同的攻击音效 

## Boss行为模式

### 新的移动策略
Boss现在采用独立的X轴移动模式，不再跟随玩家的水平位置：

#### 移动循环
```
1. 从中央开始，向右移动
2. 移动过程中持续攻击玩家
3. 到达右边界，停留3秒（继续攻击）
4. 改变方向，向左移动
5. 移动过程中持续攻击玩家
6. 到达左边界，停留3秒（继续攻击）
7. 改变方向，向右移动
8. 循环往复，直到15秒生存时间结束
```

#### Y轴跟随
- Boss的Y轴仍然平滑跟随玩家位置
- 确保Boss始终保持在玩家上方的固定距离
- 受活动区域上下边界限制

### 状态机设计
- **MovingAndAttacking**：移动并攻击状态
  - Boss向左或向右移动
  - 定期发射弹幕攻击玩家
  - 到达边界时切换到等待状态
  
- **WaitingAtLeft**：在左边界等待状态
  - 停留3秒
  - 继续攻击玩家
  - 时间到后切换为向右移动
  
- **WaitingAtRight**：在右边界等待状态
  - 停留3秒
  - 继续攻击玩家
  - 时间到后切换为向左移动

### 攻击模式
- **持续攻击**：Boss在任何状态下都会持续攻击
- **移动攻击**：移动时根据`timeBetweenAttacks`间隔发射
- **等待攻击**：在边界等待时也继续攻击
- **多开火模式**：支持交替、同时或随机开火

### 边移动边攻击测试
- **移动连续性**: 确认Boss左右移动流畅无卡顿
- **攻击连续性**: 验证Boss在移动和等待时都能攻击
- **边界行为**: 测试Boss在左右边界的停留和转向
- **生存时间**: 确认Boss在15秒后正确销毁

## 注意事项
- **动态活动区域的核心价值**：解决玩家下落时Boss脱离摄像机视野的问题
- **独立X轴移动的重要性**：Boss有自己的攻击模式，不受玩家水平位置影响
- **平滑跟随的重要性**：提升游戏体验，避免Boss移动时的视觉突兀感
- **边移动边攻击设计**：增加游戏挑战性和压迫感
- **所有边界都是相对的**：偏移量相对玩家位置，不是固定世界坐标
- Boss会平滑跟随玩家的Y轴位置，X轴独立移动
- 弹幕击中玩家会立即触发死亡，无视觉特效
- **弹幕边界清理机制**：弹幕碰到BottomLine时自动销毁，确保不会永久存在
- 玩家死亡时会自动停止Boss生成
- **BossManager偏移验证**：系统会检查偏移设置是否合理
- **Boss生存时间管理**：15秒后自动销毁，避免永久存在
- **攻击间隔调节**：根据游戏难度调整攻击频率
- **边界停留时间**：可调整Boss在边界的停留时间
- **G键测试不受游戏状态限制**：即使在攻击间隔内也可生成
- 确保两个开火点都正确分配，否则会有警告提示
- 建议开火点位置不要重叠，保持一定距离
- **活动区域应适合移动模式**：确保左右偏移给Boss足够的移动空间
- 同时开火模式会增加弹幕密度，需要谨慎平衡难度
- **测试时可能生成多个Boss**：注意性能影响
- **Boss预制体无需活动区域配置**：所有参数由BossManager动态传递
- **摄像机跟随兼容**：系统设计完全兼容摄像机跟随玩家的机制
- **独立移动不受玩家影响**：Boss的水平移动模式完全独立于玩家位置

## 扩展建议
- 可以为不同开火点设置不同的子弹类型
- 可以添加开火点的视觉特效（如火花、光芒）
- 可以根据Boss状态动态切换开火模式
- 可以添加开火点的瞄准延迟，增加预判要素
- 可以为左右手添加不同的攻击音效 

[Header("Animation Settings")]
[SerializeField] private float entryAnimationDuration = 2.5f; // 入场动画时长
[SerializeField] private float attackAnimationDuration = 1.5f; // 攻击动画时长

private Animator animator; // 动画控制器

// 动画状态控制
private bool isPlayingEntryAnimation = true; // 是否正在播放入场动画
private float entryAnimationTimer = 0f; // 入场动画计时器

private enum BossState
{
    Entry,              // 入场状态
    MovingAndAttacking, // 移动并攻击
    WaitingAtLeft,      // 在左边界等待
    WaitingAtRight      // 在右边界等待
}

private BossState currentState = BossState.Entry; // 改为Entry开始

void Start()
{
    audioManager = AudioManager.instance;
    if (audioManager == null)
    {
        Debug.LogError("No audio manager found in Boss");
    }
    
    // 获取Animator组件
    animator = GetComponent<Animator>();
    if (animator == null)
    {
        Debug.LogError("No Animator component found on Boss! Please add an Animator component.");
    }
    
    // 自动设置开火模式
    if (leftFirePoint == null || rightFirePoint == null)
    {
        Debug.LogWarning("Missing fire points! Please assign both left and right fire points.");
    }
    
    // 记录Boss生成时间
    gameStartTime = Time.time;
    
    Debug.Log("Boss spawned - will start attacking after 2.5 seconds");
}

void StartEntryAnimation()
{
    isPlayingEntryAnimation = true;
    entryAnimationTimer = 0f;
    currentState = BossState.Entry;
    
    Debug.Log("Boss entry animation started");
    
    // 2.5秒后切换到攻击动画
    StartCoroutine(TransitionToAttackAnimation());
}

IEnumerator TransitionToAttackAnimation()
{
    yield return new WaitForSeconds(entryAnimationDuration);
    
    // 触发攻击动画
    if (animator != null)
    {
        animator.SetTrigger("StartAttack");
        Debug.Log("Boss entry animation finished, starting attack animation");
    }
    
    // 结束入场状态，开始正常的攻击和移动
    isPlayingEntryAnimation = false;
    currentState = BossState.MovingAndAttacking;
    stateTimer = 0f;
}

void Update()
{
    if (player == null) return;
    
    // 检查生存时间
    if (hasLifetimeLimit)
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Debug.Log("Boss lifetime expired, destroying...");
            DestroySelf();
            return;
        }
    }
    
    // 入场动画期间只进行Y轴跟随，不进行攻击和X轴移动
    if (isPlayingEntryAnimation)
    {
        entryAnimationTimer += Time.deltaTime;
        
        // 只进行Y轴平滑跟随
        Vector3 currentPos = transform.position;
        float targetY = player.position.y + yOffset;
        
        // 应用活动区域Y轴限制
        if (useActivityArea)
        {
            GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
            targetY = Mathf.Clamp(targetY, bottom, top);
        }
        
        // 使用平滑插值移动到目标Y位置
        float smoothedY = Mathf.Lerp(currentPos.y, targetY, followSmoothness * Time.deltaTime);
        currentPos.y = smoothedY;
        transform.position = currentPos;
        
        return; // 入场动画期间不执行其他逻辑
    }
    
    // 正常状态下的逻辑（原来的Update内容）
    Vector3 currentPosition = transform.position;
    float targetY2 = player.position.y + yOffset;
    
    // 应用活动区域Y轴限制
    if (useActivityArea)
    {
        GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
        targetY2 = Mathf.Clamp(targetY2, bottom, top);
    }
    
    // 使用平滑插值移动到目标Y位置
    float smoothedY2 = Mathf.Lerp(currentPosition.y, targetY2, followSmoothness * Time.deltaTime);
    currentPosition.y = smoothedY2;
    transform.position = currentPosition;
    
    // Handle state machine (入场动画结束后)
    HandleStateMachine();
    
    // 确保Boss始终在活动区域内
    if (useActivityArea)
    {
        ClampToActivityAreaX();
    }
}

void HandleStateMachine()
{
    // Boss生成后2.5秒内不进行任何操作
    if (Time.time - gameStartTime < 2.5f)
        return;
        
    GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
    
    switch (currentState)
    {
        case BossState.Entry:
            // Entry状态由动画系统处理，这里不需要额外逻辑
            break;
            
        case BossState.MovingAndAttacking:
            // 原来的MovingAndAttacking逻辑
            // ... 现有代码保持不变
            break;
            
        // ... 其他case保持不变
    }
}