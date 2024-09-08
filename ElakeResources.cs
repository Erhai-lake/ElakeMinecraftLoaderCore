using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElakeMinecraftLoaderCore
{
    /// <summary>
    /// 资源类
    /// </summary>
    public class ElakeResources
    {
        /// <summary>
        /// Minecraft版本Json数据
        /// </summary>
        public static string VersionJson;

        /// <summary>
        /// 自动选择延迟更低的源
        /// </summary>
        /// <remarks>
        /// 自动选择 MoJang 或 BMCLAPI
        /// </remarks>
        /// <returns>异步返回更块的源</returns>
        public static async Task<string> AutomaticallySelectSource()
        {
            // 源链接列表
            string[] URLs = new string[]
            {
                "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
                "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json"
            };
            long LowerDelayForMojang = long.MaxValue;
            long LowerDelayForBMCLAPI = long.MaxValue;
            using (HttpClient Client = new HttpClient())
            {
                foreach (var URL in URLs)
                {
                    Stopwatch Stopwatch = Stopwatch.StartNew();
                    try
                    {
                        HttpResponseMessage Response = await Client.GetAsync(URL);
                        Response.EnsureSuccessStatusCode();
                        Stopwatch.Stop();
                        if (URL == URLs[0])
                        {
                            LowerDelayForMojang = Stopwatch.ElapsedMilliseconds;
                        }
                        else
                        {
                            LowerDelayForBMCLAPI = Stopwatch.ElapsedMilliseconds;
                        }
                    }
                    catch
                    {
                        // 默认选择 MoJang
                        return "MoJang";
                    }
                }
            }
            return LowerDelayForMojang < LowerDelayForBMCLAPI ? "MoJang" : "BMCLAPI";
        }

        /// <summary>
        /// 初始化源
        /// </summary>
        /// <remarks>
        /// 将版本信息写入VersionJson
        /// </remarks>
        /// <param name="Source">源(MoJang,BMCLAPI)</param>
        /// <returns>成功返回true</returns>
        public static async Task<bool> InitializeSource(string Source = "MoJang")
        {
            string URL;
            if (Source == "MoJang")
            {
                URL = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
            }
            else
            {
                URL = "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json";
            }
            string Response = await ElakeAuxiliaryTools.GETRequest(URL);
            if (string.IsNullOrEmpty(Response) || Response.Contains("HttpRequestException"))
            {
                return false;
            }
            VersionJson = Response;
            return true;
        }

        /// <summary>
        /// 获取最新的快照版本
        /// </summary>
        /// <remarks>
        /// 获取最新的快照版本
        /// </remarks>
        /// <param name="SourceJson">源Json,如果为空,使用VersionJson</param>
        /// <returns>版本号</returns>
        public static string GetNewSnapshot(string SourceJson = null)
        {
            try
            {
                if (SourceJson == null)
                {
                    SourceJson = VersionJson;
                }
                using (JsonDocument Doc = JsonDocument.Parse(SourceJson))
                {

                    JsonElement Root = Doc.RootElement;
                    JsonElement LatestSnapshot = Root.GetProperty("latest").GetProperty("snapshot");
                    return LatestSnapshot.GetString();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取最新的正式版本
        /// </summary>
        /// <remarks>
        /// 获取最新的正式版本
        /// </remarks>
        /// <param name="SourceJson">源Json,如果为空,使用VersionJson</param>
        /// <returns>版本号</returns>
        public static string GetNewRelease(string SourceJson = null)
        {
            try
            {
                if (SourceJson == null)
                {
                    SourceJson = VersionJson;
                }
                using (JsonDocument Doc = JsonDocument.Parse(SourceJson))
                {
                    JsonElement Root = Doc.RootElement;
                    JsonElement LatestSnapshot = Root.GetProperty("latest").GetProperty("release");
                    return LatestSnapshot.GetString();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取版本列表
        /// </summary>
        /// <remarks>
        /// 获取版本列表
        /// </remarks>
        /// <param name="SourceJson">源Json,如果为空,使用VersionJson</param>
        /// <param name="Release">是否获取正式版本,默认获取</param>
        /// <param name="Snapshot">是否获取快照版本,默认获取</param>
        /// <param name="Old">是否获取远古版本,默认获取</param>
        /// <returns>版本列表</returns>
        public static List<VersionInfoList> GetVersionList(string SourceJson = null, bool Release = true, bool Snapshot = true, bool Old = true)
        {
            try
            {
                if (SourceJson == null)
                {
                    SourceJson = VersionJson;
                }
                List<VersionInfoList> VersionList = new List<VersionInfoList>();
                using (JsonDocument Doc = JsonDocument.Parse(SourceJson))
                {
                    JsonElement Root = Doc.RootElement;
                    JsonElement LatestSnapshot = Root.GetProperty("versions");
                    // 确保LatestSnapshot是一个数组
                    if (LatestSnapshot.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement Item in LatestSnapshot.EnumerateArray())
                        {
                            string ID = Item.GetProperty("id").GetString();
                            string Type = Item.GetProperty("type").GetString();
                            string URL = Item.GetProperty("url").GetString();
                            string Time = Item.GetProperty("releaseTime").GetString();
                            string SHA1 = Item.GetProperty("sha1").GetString();
                            if (Type == "release" && !Release) continue;
                            else if (Type == "snapshot" && !Snapshot) continue;
                            else if (Type == "old_beta" && !Old) continue;
                            else if (Type == "old_alpha" && !Old) continue;
                            VersionList.Add(new VersionInfoList
                            {
                                Name = ID,
                                Type = Type,
                                URL = URL,
                                Time = Time,
                                SHA1 = SHA1
                            });
                        }
                    }
                    return VersionList;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 版本信息列表类
    /// </summary>
    /// <remarks>
    /// Name 版本名称
    /// Type 版本类型
    /// URL 版本下载地址
    /// Time 版本发布时间
    /// SHA1 版本文件SHA1值
    /// </remarks>
    public class VersionInfoList
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string URL { get; set; }
        public string Time { get; set; }
        public string SHA1 { get; set; }
    }
}
