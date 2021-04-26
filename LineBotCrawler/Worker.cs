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
        private string OPGG_CP_Info_url = "http://tw.op.gg/champion/statistics";

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
                        CpUrl = cpState.CpUrl
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
                    championState.CpUrl = cpState.CpUrl;
                    Console.WriteLine("update success");
                }
            }
            await _db.SaveChangesAsync();

            //var championState = new ChampionState
            //{
            //    CpName = "測試",
            //    CpNameEn = "Test",
            //    CpUrl = "http://test",
            //    CpPosition = "上路"
            //};
            //_db.ChampionState.Add(championState);
            //await _db.SaveChangesAsync();
            ////可以用 Console.WriteLine 輸出 Log
            //Console.WriteLine("DB!!");
        }

        private async Task<List<(string CpName, string CpNameEn, string CpUrl, string CpPosition)>> GetCpInfo(HttpClient httpClient, string url)
        {
            var html = await httpClient.GetStringAsync(url);
            // ""><i) 抓取URL
            var matches = Regex.Matches(html, @"(?<=data-champion-name="")([^""]*)[\s\S]*?(?<=data-champion-key="")([^""]*)[\s\S]*?(?<=href="")([^""]*)[\s\S]*?champion-index__champion-item__positions""><div class=""champion-index__champion-item__position"">(.*?)<\/a>");
            return matches.ToList().Select(it =>
            {
                var CpName = it.Groups[1].Value.Trim();
                var CpNameEn = it.Groups[2].Value.Trim();
                var CpUrl = it.Groups[3].Value.Trim();
                var CpPosition = "";
                MatchCollection matches_lane = Regex.Matches(it.Groups[4].Value, @"(?<=span>)([\u4e00-\u9fa5]+)");
                foreach (Match m in matches_lane)
                    CpPosition = String.Concat(CpPosition, String.Concat(" ", m.Value.Trim()));
                Console.WriteLine(CpName + "+" + CpNameEn + "+" + CpUrl + "+" + CpPosition);
                return (CpName, CpNameEn, CpUrl, CpPosition);
            })
            .ToList();
        }
    }
}
/*
 (?<=data-champion-name="")([^""]*)[\s\S]*?(?<=data-champion-key="")([^""]*)[\s\S]*?(?<=href="")([^""]*)
 */
