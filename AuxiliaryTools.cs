using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

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
        /// 获取Java信息列表
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
        public static List<JavaInfoList> GetJavaList()
        {
            List<JavaInfoList> JavaList = new List<JavaInfoList>();
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
                                JavaList.Add(new JavaInfoList
                                {
                                    Version = JavaVersion,
                                    Bitness = JavaBitness,
                                    Path = Line
                                });
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

        /// <summary>
        /// 获取CPU信息列表
        /// </summary>
        /// <remarks>
        /// 获取CPU信息列表(品牌,型号,主频)
        /// </remarks>
        /// <returns>CPU信息列表</returns>
        public static string[] GetCpuInfo()
        {
            ManagementClass ManagementClass = new ManagementClass("Win32_Processor");
            ManagementObjectCollection ManagementObjectCollection = ManagementClass.GetInstances();
            List<string> CpuInfoList = new List<string>();
            foreach (ManagementObject ManagementObject in ManagementObjectCollection)
            {
                string Manufacturer = ManagementObject["Manufacturer"].ToString().Trim();
                string Name = ManagementObject["Name"].ToString().Trim();
                // 格式化输出
                string Info = $"{Manufacturer} {Name}";
                // 添加到列表中
                CpuInfoList.Add(Info);
            }
            return CpuInfoList.ToArray();
        }

        /// <summary>
        /// 获取GPU信息列表
        /// </summary>
        /// <remarks>
        /// 获取GPU信息列表(品牌,型号,显存大小(GB))
        /// </remarks>
        /// <returns>GPU信息列表</returns>
        public static string[] GetGpuInfo()
        {
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            List<string> GpuInfoList = new List<string>();
            foreach (ManagementObject ManagementObject in Searcher.Get())
            {
                string Name = ManagementObject["Name"].ToString().Trim();
                string AdapterRAM = ManagementObject["AdapterRAM"].ToString().Trim();
                // 将AdapterRAM从字节数转换为GB
                double AdapterRAMGB = Math.Round(Convert.ToDouble(AdapterRAM) / (1024 * 1024 * 1024), 2);
                // 格式化输出
                string info = $"{Name} {AdapterRAMGB}GB";
                // 添加到列表中
                GpuInfoList.Add(info);
            }
            return GpuInfoList.ToArray();
        }

        /// <summary>
        /// 获取RAM信息列表
        /// </summary>
        /// <remarks>
        /// 获取RAM信息列表(品牌,型号,容量,速度)
        /// </remarks>
        /// <returns>RAM信息列表</returns>
        public static string[] GetRAMInfo()
        {
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            List<string> RAMInfoList = new List<string>();
            foreach (ManagementObject ManagementObject in Searcher.Get())
            {
                string Manufacturer = ManagementObject["Manufacturer"].ToString().Trim();
                string Model = ManagementObject["PartNumber"].ToString().Trim();
                string Capacity = ManagementObject["Capacity"].ToString().Trim();
                string Speed = ManagementObject["Speed"].ToString().Trim();
                // 将容量从字节数转换为GB
                double CapacityGB = Math.Round(Convert.ToDouble(Capacity) / (1024 * 1024 * 1024), 2);
                // 格式化输出
                string info = $"{Manufacturer} {Model} {CapacityGB}GB {Speed}MHz";
                // 添加到列表中
                RAMInfoList.Add(info);
            }
            return RAMInfoList.ToArray();
        }
    }

    /// <summary>
    /// Java信息类
    /// </summary>
    /// <remarks>
    /// Version 版本号
    /// Bitness 位长
    /// Path 路径
    /// </remarks>
    public class JavaInfoList
    {
        public string Version { get; set; }
        public string Bitness { get; set; }
        public string Path { get; set; }
    }
}
