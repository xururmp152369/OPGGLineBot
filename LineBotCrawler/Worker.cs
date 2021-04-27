using LinBotDBClass;
using LinBotDBClass.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LineBotCrawler
{
    public class Worker : IHostedService
    {
        private readonly IHostApplicationLifetime _lifeTime;
        private readonly CoreDbContext _db;
        private string OPGG_CP_Info_url = "http://www.op.gg/champion/statistics";

        public Worker(IHostApplicationLifetime lifeTime, CoreDbContext db)
        {
            _lifeTime = lifeTime;
            _db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                //使用新執行續執行
                ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                //可以用 Console.WriteLine 輸出 Log
                Console.WriteLine("Finish!!");
                //結束後關閉視窗
                _lifeTime.StopApplication();
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync()
        {
            using var httpClient = new HttpClient();

            //從 DB 取得所有英雄名稱
            var ChampionStateDictionary = await _db.ChampionState
                //.Where(it => it.ITHomeId == iTHome.Id)
                .ToDictionaryAsync(it => it.CpName);

            var CpInfo = await GetCpInfo(httpClient, OPGG_CP_Info_url);
            foreach(var cpState in CpInfo)
            {
                var championState = ChampionStateDictionary.ContainsKey(cpState.CpName)
                    ? ChampionStateDictionary[cpState.CpName] : null;
                //新增
                if (championState == null)
                {
                    championState = new ChampionState
                    {
                        CpName = cpState.CpName,
                        CpNameEn = cpState.CpNameEn,
                        CpPosition = cpState.CpPosition,
                        CpUri = cpState.CpUri
                    };
                    _db.ChampionState.Add(championState);
                    ChampionStateDictionary.Add(championState.CpName, championState);
                    Console.WriteLine("insert success");
                }
                //更新
                else
                {
                    championState.CpName = cpState.CpName;
                    championState.CpNameEn = cpState.CpNameEn;
                    championState.CpPosition = cpState.CpPosition;
                    championState.CpUri = cpState.CpUri;
                    Console.WriteLine("update success");
                }
            }
            await _db.SaveChangesAsync();

            //var championState = new ChampionState
            //{
            //    CpName = "測試",
            //    CpNameEn = "Test",
            //    CpUri = "http://test",
            //    CpPosition = "上路"
            //};
            //_db.ChampionState.Add(championState);
            //await _db.SaveChangesAsync();
            ////可以用 Console.WriteLine 輸出 Log
            //Console.WriteLine("DB!!");
        }

        private async Task<List<(string CpName, string CpNameEn, string CpUri, string CpPosition)>> GetCpInfo(HttpClient httpClient, string url)
        {
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh_TW,zh;q=0.9");
            var html = await httpClient.GetStringAsync(url);
            
            var matches = Regex.Matches(html, @"(?<=data-champion-name="")([^""]*)[\s\S]*?(?<=data-champion-key="")([^""]*)([\s\S]*?)(?=<\/div><\/div)");
            return matches.ToList().Select(it =>
            {
                var CpName = it.Groups[1].Value;
                var CpNameEn = it.Groups[2].Value;
                var CpUri = "";
                var CpPosition = "";

                MatchCollection matches_rip = Regex.Matches(it.Groups[3].Value, @"(?<=icon-)([^@]*)?");

                if(matches_rip.Count != 0)
                {
                    CpUri = "No Data";
                    CpPosition = "No Data";
                }
                else
                {
                    MatchCollection matches_uri = Regex.Matches(it.Groups[3].Value, @"(?<=href="")([^""]*)[\s\S]*?(?<=champion-index__champion-item__positions"">)([\s\S]*)?");
                    foreach (Match uri in matches_uri)
                    {
                        GroupCollection uri_groups = uri.Groups;
                        CpUri = String.Concat("http://www.op.gg", uri_groups[1]);

                        MatchCollection matches_lane = Regex.Matches(uri_groups[2].Value, @"(?<=span>)([\u4E00-\u9FFF]+)");
                        foreach (Match lane in matches_lane)
                            CpPosition = String.Concat(CpPosition, String.Concat(" ", lane.Value));
                    }
                }
                return (CpName, CpNameEn, CpUri, CpPosition);
            })
            .ToList();
        }

    }
}
/*
 (?<=data-champion-name=")([^"]*)[\s\S]*?(?<=data-champion-key=")([^"]*)[\s\S]*?(?<=href=")([^"]*)[\s\S]*?champion-index__champion-item__positions"><div class="champion-index__champion-item__position">(.*?)<\/a>
 */
