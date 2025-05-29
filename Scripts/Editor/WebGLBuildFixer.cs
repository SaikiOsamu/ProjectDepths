using UnityEngine;
using UnityEditor;

public class WebGLBuildFixer : MonoBehaviour
{
    [MenuItem("Tools/Fix WebGL Build Settings")]
    public static void FixWebGLSettings()
    {
        Debug.Log("开始修复WebGL构建设置...");
        
        // 设置WebGL平台为当前构建目标
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        
        // 修复WebGL Player设置
        PlayerSettings.WebGL.memorySize = 128; // 增加初始内存到128MB
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None; // 禁用异常支持以提高性能
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip; // 启用Gzip压缩
        PlayerSettings.WebGL.wasmArithmeticExceptions = false; // 禁用WebAssembly算术异常
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm; // 使用WebAssembly目标
        PlayerSettings.WebGL.threadsSupport = false; // 禁用线程支持
        PlayerSettings.WebGL.decompressionFallback = false; // 禁用解压缩回退
        
        // 设置内存管理
        PlayerSettings.WebGL.initialMemorySize = 128;
        PlayerSettings.WebGL.maximumMemorySize = 1024; // 增加最大内存到1GB
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Linear;
        
        // 优化图形设置
        PlayerSettings.colorSpace = ColorSpace.Linear; // 使用线性颜色空间
        PlayerSettings.stripEngineCode = true; // 启用引擎代码剥离
        
        // 设置脚本编译选项
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_Standard_2_0);
        
        Debug.Log("WebGL构建设置修复完成！");
        Debug.Log("请查看Player Settings中的WebGL设置以确认更改。");
        
        // 提示用户接下来的步骤
        if (EditorUtility.DisplayDialog("设置已更新", 
            "WebGL构建设置已自动修复！\n\n建议接下来：\n1. 清理Library和Temp文件夹\n2. 重新构建项目\n3. 检查URP设置是否适合WebGL", 
            "了解", "取消"))
        {
            Debug.Log("请手动删除Library和Temp文件夹，然后重新打开项目。");
        }
    }
    
    [MenuItem("Tools/Check WebGL Compatibility")]
    public static void CheckWebGLCompatibility()
    {
        Debug.Log("检查WebGL兼容性...");
        
        // 检查当前质量设置
        string[] qualityNames = QualitySettings.names;
        int currentQuality = QualitySettings.GetQualityLevel();
        
        Debug.Log($"当前质量级别: {qualityNames[currentQuality]}");
        
        // 检查渲染管线
        var pipeline = QualitySettings.renderPipeline;
        if (pipeline != null)
        {
            Debug.Log($"使用渲染管线: {pipeline.name}");
        }
        else
        {
            Debug.Log("使用内置渲染管线");
        }
        
        // 检查WebGL特定设置
        Debug.Log($"WebGL内存大小: {PlayerSettings.WebGL.memorySize}MB");
        Debug.Log($"WebGL异常支持: {PlayerSettings.WebGL.exceptionSupport}");
        Debug.Log($"WebGL压缩格式: {PlayerSettings.WebGL.compressionFormat}");
        
        Debug.Log("兼容性检查完成！请查看Console日志获取详细信息。");
    }
} 