# WebGL构建失败修复指南

## 问题诊断
您的Unity项目WebGL构建失败，主要原因可能包括：

### 1. WebGL内存设置过低
当前设置：
- Initial Memory: 32MB
- Maximum Memory: 2048MB

### 2. URP管线配置问题
项目使用了多个质量级别的URP管线资产

## 修复步骤

### 步骤1: 调整WebGL Player设置
1. 打开 `File` → `Build Settings`
2. 选择 `WebGL` 平台
3. 点击 `Player Settings`
4. 在 `Publishing Settings` 中调整：
   - **Initial Memory Size**: 改为 `128` MB
   - **Maximum Memory Size**: 改为 `512` MB (如果项目较大可设为1024MB)
   - **Memory Growth Mode**: 设为 `Linear`

### 步骤2: 优化构建设置
在 `Player Settings` → `WebGL Settings` 中：
- **Exception Support**: 设为 `None` (发布版本)
- **Compression Format**: 设为 `Gzip` 或 `Brotli`
- **WebAssembly Big Int**: 禁用
- **WebAssembly SIMD**: 禁用 (除非特别需要)

### 步骤3: 优化URP设置
1. 检查 `Assets/Settings/Very Low_PipelineAsset.asset`
2. 确保以下设置适合WebGL：
   - **MSAA**: 设为 `Disabled` 或 `2x`
   - **HDR**: 禁用
   - **Shadow Distance**: 降低到10-15
   - **Shadow Cascades**: 设为1

### 步骤4: 清理并重新构建
1. 删除 `Library` 文件夹
2. 删除 `Temp` 文件夹  
3. 重新打开Unity项目
4. 尝试重新构建WebGL

### 步骤5: 检查代码兼容性
确保您的脚本没有使用WebGL不支持的功能：
- 线程相关代码
- 文件系统访问
- 某些System.IO操作

## 额外建议

### 性能优化
- 减少贴图大小和质量
- 使用WebGL兼容的着色器
- 限制同时播放的音频数量

### 如果问题仍然存在
1. 查看Unity Console中的详细错误信息
2. 检查是否有第三方插件不兼容WebGL
3. 尝试创建一个最小化的测试场景进行构建测试

## 常见错误代码解决方案

如果看到类似 `wasm compilation failed` 的错误：
- 降低代码复杂度
- 禁用IL2CPP代码优化
- 检查是否有递归调用或无限循环

如果看到内存相关错误：
- 增加Initial Memory Size
- 优化资源使用
- 减少同时加载的资源数量 