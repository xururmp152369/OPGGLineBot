using LinBotDBClass;
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
            //var championState = await _db.ChampionState.FirstOrDefaultAsync();

            //Console.WriteLine($"{championState.CpPosition} {championState.CpName}");
        }

        private async Task<List<(string CpName, string CpNameEn, string CpUrl, string CpPosition)>> GetCpInfo(HttpClient httpClient, string url)
        {
            var html = await httpClient.GetStringAsync(url);
            // ""><i) 抓取URL
            var matches = Regex.Matches(html, @"");
            return;
        }
    }
}
/*
 
 */
