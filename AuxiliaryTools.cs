using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ElakeMinecraftLoaderCore
{
    /// <summary>
    /// 辅助工具类
    /// </summary>
    public class AuxiliaryTools
    {
        /// <summary>
        /// 获取Java的版本号
        /// </summary>
        /// <remarks>
        /// 通过输入Java路径来获取Java版本号
        /// </remarks>
        /// <param name="JavaPath">Java路径</param>
        /// <returns>版本号</returns>
        public static string GetJavaVersion(string JavaFolderPath)
        {
            // 构建java.exe的完整路径
            string JavaExecutablePath = System.IO.Path.Combine(JavaFolderPath, "bin", "java.exe");
            // 确保java.exe存在
            if (!System.IO.File.Exists(JavaExecutablePath))
            {
                return "未找到Java";
            }
            // 创建一个新的进程,用于启动java.exe
            Process Process = new Process();
            Process.StartInfo.FileName = JavaExecutablePath;
            Process.StartInfo.Arguments = "-version";
            Process.StartInfo.RedirectStandardError = true; // 重定向标准错误输出
            Process.StartInfo.UseShellExecute = false; // 不使用操作系统shell启动
            Process.StartInfo.CreateNoWindow = true; // 不创建窗口
            try
            {
                // 启动进程
                Process.Start();
                // 读取错误输出,其中包含版本信息
                string Output = Process.StandardError.ReadToEnd();
                // 关闭进程
                Process.WaitForExit();
                Process.Close();
                // 解析版本号
                // 输出的格式通常是:java version "21.0.4"
                // 通过查找特定的字符串来提取版本号
                int StartIndex = Output.IndexOf("version \"");
                if (StartIndex != -1)
                {
                    StartIndex += "version \"".Length;
                    int EndIndex = Output.IndexOf("\"", StartIndex);
                    if (EndIndex != -1)
                    {
                        return Output.Substring(StartIndex, EndIndex - StartIndex);
                    }
                }
                throw new InvalidOperationException("无法解析Java版本号");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取Java版本号时发生错误", ex);
            }
        }

        /// <summary>
        /// 获取Java的位数
        /// </summary>
        /// <remarks>
        /// 通过输入Java路径来获取Java的位数(32位或64位)
        /// </remarks>
        /// <param name="JavaPath">Java路径</param>
        /// <returns>位数</returns>
        public static string GetJavaBitness(string JavaFolderPath)
        {
            // 构建java.exe的完整路径
            string JavaExecutablePath = System.IO.Path.Combine(JavaFolderPath, "bin", "java.exe");
            // 确保java.exe存在
            if (!System.IO.File.Exists(JavaExecutablePath))
            {
                return "未找到Java";
            }
            // 创建一个新的进程,用于启动java.exe
            Process Process = new Process();
            Process.StartInfo.FileName = JavaExecutablePath;
            Process.StartInfo.Arguments = "-version";
            Process.StartInfo.RedirectStandardError = true; // 重定向标准错误输出
            Process.StartInfo.UseShellExecute = false; // 不使用操作系统shell启动
            Process.StartInfo.CreateNoWindow = true; // 不创建窗口
            try
            {
                // 启动进程
                Process.Start();
                // 读取错误输出,其中包含版本信息
                string Output = Process.StandardError.ReadToEnd();
                // 关闭进程
                Process.WaitForExit();
                Process.Close();
                // 检查输出中是否包含"64-Bit"字符串
                if (Output.Contains("64-Bit"))
                {
                    return "64";
                }
                else
                {
                    return "32";
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取Java版本号时发生错误", ex);
            }
        }

        /// <summary>
        /// 获取Java的列表
        /// </summary>
        /// <remarks>
        /// 获取Java列表(版本号,位数,路径),使用where指令查找,耗时较长,只返回有效的Java
        /// </remarks>
        /// <returns>Java列表</returns>
        public static List<JavaInfo> GetJavaList()
        {
            List<JavaInfo> JavaList = new List<JavaInfo>();
            string[] Letters = Enumerable.Range('A', 26).Select(x => ((char)x).ToString()).ToArray();
            foreach (string Letter in Letters)
            {
                using (Process Process = new Process())
                {
                    Process.StartInfo.FileName = "cmd.exe";
                    Process.StartInfo.Arguments = $"/c where /R {Letter}:\\ java.exe";
                    Process.StartInfo.RedirectStandardOutput = true; // 重定向标准输出
                    Process.StartInfo.RedirectStandardError = true; // 重定向标准错误输出
                    Process.StartInfo.UseShellExecute = false; // 不使用操作系统shell启动
                    Process.StartInfo.CreateNoWindow = true; // 不创建窗口
                    try
                    {
                        // 启动进程
                        Process.Start();
                        // 读取标准输出,其中包含java.exe的路径
                        string Output = Process.StandardOutput.ReadToEnd();
                        // 关闭进程
                        Process.WaitForExit();

                        // 按行分割输出结果
                        using (StringReader Reader = new StringReader(Output))
                        {
                            string Line;
                            while ((Line = Reader.ReadLine()) != null)
                            {
                                Line = Line.Replace("\\bin\\java.exe", string.Empty);
                                string JavaVersion = GetJavaVersion(Line);
                                string JavaBitness = GetJavaBitness(Line);
                                // 添加到列表中
                                if (JavaVersion == "未找到Java") continue;
                                JavaList.Add(new JavaInfo { Version = JavaVersion, Bitness = JavaBitness, Path = Line });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("发生错误", ex);
                    }
                }
            }
            return JavaList;
        }
    }

    /// <summary>
    /// Java信息类
    /// </summary>
    public class JavaInfo
    {
        public string Version { get; set; }
        public string Bitness { get; set; }
        public string Path { get; set; }
    }
}
