using LinBotDBClass;
using LinBotDBClass.Models;
using LineBotDBClass.Models;
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
            using var httpClient = new HttpClient();

            //從 DB 取得所有英雄名稱
            var ChampionStateDictionary = await _db.ChampionState
                //.Where(it => it.ITHomeId == iTHome.Id)
                .ToDictionaryAsync(it => it.CpName);
            var OPGG_CP_Info_url = "http://www.op.gg/champion/statistics";

            var CpInfo = await GetCpInfo(httpClient, OPGG_CP_Info_url);
            foreach (var cpState in CpInfo)
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
                }
                //更新
                else
                {
                    championState.CpName = cpState.CpName;
                    championState.CpNameEn = cpState.CpNameEn;
                    championState.CpPosition = cpState.CpPosition;
                    championState.CpUri = cpState.CpUri;
                }
            }
            await _db.SaveChangesAsync();

            //------------------------以上為新增英雄基本資訊---------------------
            //-----------------已下為新增個別英雄對應路線詳細資訊----------------

            //取得英雄所有資訊
            var ChampionDictionary = await _db.ChampionState.ToListAsync();
            var ChampionTopDictionary = await _db.ChampionTopInfos.ToDictionaryAsync(it => it.CpName);
            var ChampionMidDictionary = await _db.ChampionMidInfos.ToDictionaryAsync(it => it.CpName);
            var ChampionAdcDictionary = await _db.ChampionAdcInfos.ToDictionaryAsync(it => it.CpName);
            var ChampionSupDictionary = await _db.ChampionSupInfos.ToDictionaryAsync(it => it.CpName);
            var ChampionJunDictionary = await _db.ChampionJunInfos.ToDictionaryAsync(it => it.CpName);

            foreach (var cpState in ChampionDictionary)
            {
                string[] strsub = cpState.CpPosition.Split(' ');
                foreach (var str in strsub)
                {
                    switch (str)
                    {
                        case "上路":
                            var TopCpDetail = await GetCpDetail(httpClient, String.Concat(cpState.CpUri, "/top"));
                            var championTop = ChampionTopDictionary.ContainsKey(cpState.CpName) ? ChampionTopDictionary[cpState.CpName] : null;
                            //新增
                            if (championTop == null)
                            {
                                championTop = new ChampionTopInfo
                                {
                                    CpLane = TopCpDetail.CpLane,
                                    CpName = TopCpDetail.CpName,
                                    CpTier = TopCpDetail.CpTier,
                                    CpSummonerSkill = TopCpDetail.CpSummonerSkill,
                                    CpSkill = TopCpDetail.CpSkill,
                                    CpStartItem = TopCpDetail.CpStartItem,
                                    CpCoreItem1 = TopCpDetail.CpCoreItem1,
                                    CpCoreItem2 = TopCpDetail.CpCoreItem2,
                                    CpBoot1 = TopCpDetail.CpBoot1,
                                    CpBoot2 = TopCpDetail.CpBoot2,
                                    CpRune = TopCpDetail.CpRune
                                };
                                _db.ChampionTopInfos.Add(championTop);
                                ChampionTopDictionary.Add(championTop.CpName, championTop);
                            }
                            //更新
                            else
                            {
                                championTop.CpLane = TopCpDetail.CpLane;
                                championTop.CpName = TopCpDetail.CpName;
                                championTop.CpTier = TopCpDetail.CpTier;
                                championTop.CpSummonerSkill = TopCpDetail.CpSummonerSkill;
                                championTop.CpSkill = TopCpDetail.CpSkill;
                                championTop.CpStartItem = TopCpDetail.CpStartItem;
                                championTop.CpCoreItem1 = TopCpDetail.CpCoreItem1;
                                championTop.CpCoreItem2 = TopCpDetail.CpCoreItem2;
                                championTop.CpBoot1 = TopCpDetail.CpBoot1;
                                championTop.CpBoot2 = TopCpDetail.CpBoot2;
                                championTop.CpRune = TopCpDetail.CpRune;
                            }
                            Console.WriteLine(championTop.CpLane + "+" + championTop.CpName + "OK");
                            break;
                        case "中路":
                            var MidCpDetail = await GetCpDetail(httpClient, String.Concat(cpState.CpUri, "/mid"));
                            var championMid = ChampionMidDictionary.ContainsKey(cpState.CpName) ? ChampionMidDictionary[cpState.CpName] : null;
                            //新增
                            if (championMid == null)
                            {
                                championMid = new ChampionMidInfo
                                {
                                    CpLane = MidCpDetail.CpLane,
                                    CpName = MidCpDetail.CpName,
                                    CpTier = MidCpDetail.CpTier,
                                    CpSummonerSkill = MidCpDetail.CpSummonerSkill,
                                    CpSkill = MidCpDetail.CpSkill,
                                    CpStartItem = MidCpDetail.CpStartItem,
                                    CpCoreItem1 = MidCpDetail.CpCoreItem1,
                                    CpCoreItem2 = MidCpDetail.CpCoreItem2,
                                    CpBoot1 = MidCpDetail.CpBoot1,
                                    CpBoot2 = MidCpDetail.CpBoot2,
                                    CpRune = MidCpDetail.CpRune
                                };
                                _db.ChampionMidInfos.Add(championMid);
                                ChampionMidDictionary.Add(championMid.CpName, championMid);
                            }
                            //更新
                            else
                            {
                                championMid.CpLane = MidCpDetail.CpLane;
                                championMid.CpName = MidCpDetail.CpName;
                                championMid.CpTier = MidCpDetail.CpTier;
                                championMid.CpSummonerSkill = MidCpDetail.CpSummonerSkill;
                                championMid.CpSkill = MidCpDetail.CpSkill;
                                championMid.CpStartItem = MidCpDetail.CpStartItem;
                                championMid.CpCoreItem1 = MidCpDetail.CpCoreItem1;
                                championMid.CpCoreItem2 = MidCpDetail.CpCoreItem2;
                                championMid.CpBoot1 = MidCpDetail.CpBoot1;
                                championMid.CpBoot2 = MidCpDetail.CpBoot2;
                                championMid.CpRune = MidCpDetail.CpRune;
                            }
                            Console.WriteLine(championMid.CpLane + "+" + championMid.CpName + "OK");
                            break;
                        case "下路":
                            var AdcCpDetail = await GetCpDetail(httpClient, String.Concat(cpState.CpUri, "/bot"));
                            var championAdc = ChampionAdcDictionary.ContainsKey(cpState.CpName) ? ChampionAdcDictionary[cpState.CpName] : null;
                            //新增
                            if (championAdc == null)
                            {
                                championAdc = new ChampionAdcInfo
                                {
                                    CpName = AdcCpDetail.CpName,
                                    CpLane = AdcCpDetail.CpLane,
                                    CpTier = AdcCpDetail.CpTier,
                                    CpSummonerSkill = AdcCpDetail.CpSummonerSkill,
                                    CpSkill = AdcCpDetail.CpSkill,
                                    CpStartItem = AdcCpDetail.CpStartItem,
                                    CpCoreItem1 = AdcCpDetail.CpCoreItem1,
                                    CpCoreItem2 = AdcCpDetail.CpCoreItem2,
                                    CpBoot1 = AdcCpDetail.CpBoot1,
                                    CpBoot2 = AdcCpDetail.CpBoot2,
                                    CpRune = AdcCpDetail.CpRune
                                };
                                _db.ChampionAdcInfos.Add(championAdc);
                                ChampionAdcDictionary.Add(championAdc.CpName, championAdc);
                            }
                            //更新
                            else
                            {
                                championAdc.CpLane = AdcCpDetail.CpLane;
                                championAdc.CpName = AdcCpDetail.CpName;
                                championAdc.CpTier = AdcCpDetail.CpTier;
                                championAdc.CpSummonerSkill = AdcCpDetail.CpSummonerSkill;
                                championAdc.CpSkill = AdcCpDetail.CpSkill;
                                championAdc.CpStartItem = AdcCpDetail.CpStartItem;
                                championAdc.CpCoreItem1 = AdcCpDetail.CpCoreItem1;
                                championAdc.CpCoreItem2 = AdcCpDetail.CpCoreItem2;
                                championAdc.CpBoot1 = AdcCpDetail.CpBoot1;
                                championAdc.CpBoot2 = AdcCpDetail.CpBoot2;
                                championAdc.CpRune = AdcCpDetail.CpRune;
                            }
                            Console.WriteLine(championAdc.CpLane + "+" + championAdc.CpName + "OK");
                            break;
                        case "輔助":
                            var SupCpDetail = await GetCpDetail(httpClient, String.Concat(cpState.CpUri, "/support"));
                            var championSup = ChampionSupDictionary.ContainsKey(cpState.CpName) ? ChampionSupDictionary[cpState.CpName] : null;
                            //新增
                            if (championSup == null)
                            {
                                championSup = new ChampionSupInfo
                                {
                                    CpName = SupCpDetail.CpName,
                                    CpLane = SupCpDetail.CpLane,
                                    CpTier = SupCpDetail.CpTier,
                                    CpSummonerSkill = SupCpDetail.CpSummonerSkill,
                                    CpSkill = SupCpDetail.CpSkill,
                                    CpStartItem = SupCpDetail.CpStartItem,
                                    CpCoreItem1 = SupCpDetail.CpCoreItem1,
                                    CpCoreItem2 = SupCpDetail.CpCoreItem2,
                                    CpBoot1 = SupCpDetail.CpBoot1,
                                    CpBoot2 = SupCpDetail.CpBoot2,
                                    CpRune = SupCpDetail.CpRune
                                };
                                _db.ChampionSupInfos.Add(championSup);
                                ChampionSupDictionary.Add(championSup.CpName, championSup);
                            }
                            //更新
                            else
                            {
                                championSup.CpLane = SupCpDetail.CpLane;
                                championSup.CpName = SupCpDetail.CpName;
                                championSup.CpTier = SupCpDetail.CpTier;
                                championSup.CpSummonerSkill = SupCpDetail.CpSummonerSkill;
                                championSup.CpSkill = SupCpDetail.CpSkill;
                                championSup.CpStartItem = SupCpDetail.CpStartItem;
                                championSup.CpCoreItem1 = SupCpDetail.CpCoreItem1;
                                championSup.CpCoreItem2 = SupCpDetail.CpCoreItem2;
                                championSup.CpBoot1 = SupCpDetail.CpBoot1;
                                championSup.CpBoot2 = SupCpDetail.CpBoot2;
                                championSup.CpRune = SupCpDetail.CpRune; ;
                            }
                            Console.WriteLine(championSup.CpLane + "+" + championSup.CpName + "OK");
                            break;
                        case "打野":
                            var JunCpDetail = await GetCpDetail(httpClient, String.Concat(cpState.CpUri, "/jungle"));
                            var championJun = ChampionJunDictionary.ContainsKey(cpState.CpName) ? ChampionJunDictionary[cpState.CpName] : null;
                            //新增
                            if (championJun == null)
                            {
                                championJun = new ChampionJunInfo
                                {
                                    CpName = JunCpDetail.CpName,
                                    CpLane = JunCpDetail.CpLane,
                                    CpTier = JunCpDetail.CpTier,
                                    CpSummonerSkill = JunCpDetail.CpSummonerSkill,
                                    CpSkill = JunCpDetail.CpSkill,
                                    CpStartItem = JunCpDetail.CpStartItem,
                                    CpCoreItem1 = JunCpDetail.CpCoreItem1,
                                    CpCoreItem2 = JunCpDetail.CpCoreItem2,
                                    CpBoot1 = JunCpDetail.CpBoot1,
                                    CpBoot2 = JunCpDetail.CpBoot2,
                                    CpRune = JunCpDetail.CpRune
                                };
                                _db.ChampionJunInfos.Add(championJun);
                                ChampionJunDictionary.Add(championJun.CpName, championJun);
                            }
                            //更新
                            else
                            {
                                championJun.CpLane = JunCpDetail.CpLane;
                                championJun.CpName = JunCpDetail.CpName;
                                championJun.CpTier = JunCpDetail.CpTier;
                                championJun.CpSummonerSkill = JunCpDetail.CpSummonerSkill;
                                championJun.CpSkill = JunCpDetail.CpSkill;
                                championJun.CpStartItem = JunCpDetail.CpStartItem;
                                championJun.CpCoreItem1 = JunCpDetail.CpCoreItem1;
                                championJun.CpCoreItem2 = JunCpDetail.CpCoreItem2;
                                championJun.CpBoot1 = JunCpDetail.CpBoot1;
                                championJun.CpBoot2 = JunCpDetail.CpBoot2;
                                championJun.CpRune = JunCpDetail.CpRune;
                            }
                            Console.WriteLine(championJun.CpLane + "+" + championJun.CpName + "OK");
                            break;
                        default:
                            Console.WriteLine(cpState.CpName + " : First Space");
                            break;
                    }
                }
                Thread.Sleep(100);
            }
            await _db.SaveChangesAsync();
        }

        //爬蟲--爬取英雄基本資訊至ChampionState資料表
        private static async Task<List<(string CpName, string CpNameEn, string CpUri, string CpPosition)>> GetCpInfo(HttpClient httpClient, string url)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Cookie", "customLocale=zh_TW");
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
        //爬蟲--爬取英雄各路線詳細資訊至路線資料表
        private static async Task<(string CpLane, string CpName, string CpTier, string CpSummonerSkill, string CpSkill, string CpStartItem,
            string CpCoreItem1, string CpCoreItem2, string CpBoot1, string CpBoot2, string CpRune)> GetCpDetail(HttpClient httpClient, string url)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Cookie", "customLocale=zh_TW");
            
            var html = await httpClient.GetStringAsync(url);
            //Lane Name Tier
            var CpLane = ""; var CpName = ""; var CpTier = "";
            MatchCollection matches_lv1 = Regex.Matches(html, @"(?<=champion-stats-header__position--active)[\s\S]*?([\u4E00-\u9FFF]+)[\s\S]*?(?<=info__name"">)([^<]+)[\s\S]*?(?<=<b>)([^<]+)");
            foreach(Match lv1 in matches_lv1)
            {
                GroupCollection lv1_groups = lv1.Groups;
                CpLane = lv1_groups[1].Value; CpName = lv1_groups[2].Value; CpTier = lv1_groups[3].Value;
            }
            //SummonerSkill
            var CpSummonerSkill = "";
            MatchCollection matches_lv2_split = Regex.Matches(html, @"推薦召喚師法術([\s\S]*?)(?=<\/tbody)");
            MatchCollection matches_lv2 = Regex.Matches(matches_lv2_split[0].Value, @"(?<=#ffc659&#039;&gt;)([^&]+)[\s\S]*?(?<=#ffc659&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)");
            foreach(Match lv2 in matches_lv2)
            {
                GroupCollection lv2_groups = lv2.Groups;
                CpSummonerSkill = lv2_groups[1].Value + " + " + lv2_groups[2] + "   使用率:" + lv2_groups[3];
                break;
            }
            //Skill
            var CpSkill = "";
            MatchCollection matches_lv3_split = Regex.Matches(html, @"推薦殺戮構建([\s\S]*?)(?=overview__table)");
            MatchCollection matches_lv3 = Regex.Matches(matches_lv3_split[0].Value, @"(?<=alt=""""> <span>)([^<]?)[\s\S]*?(?<=alt=""""> <span>)([^<]?)[\s\S]*?(?<=alt=""""> <span>)([^<]?)[\s\S]*?(?<=strong>)(\d+.\d+%)");
            foreach(Match lv3 in matches_lv3)
            {
                GroupCollection lv3_groups = lv3.Groups;
                CpSkill = lv3_groups[1].Value + " > " + lv3_groups[2] + " > " + lv3_groups[3] + "   使用率:" + lv3_groups[4];
            }
            //StartItem CoreItem Boot
            var CpStartItem = ""; var CpCoreItem1 = ""; var CpCoreItem2 = ""; var CpBoot1 = ""; var CpBoot2 = "";
            MatchCollection matches_lv4_split = Regex.Matches(html, @"推薦Item構建([\s\S]*?)(?=<\/tbody)");
            if (CpName == "卡莎碧雅")
            {
                MatchCollection matches_lv4 = Regex.Matches(matches_lv4_split[0].Value, @"(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?推薦構建[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)");
                foreach (Match lv4 in matches_lv4)
                {
                    GroupCollection lv4_groups = lv4.Groups;
                    CpStartItem = lv4_groups[1].Value + " + " + lv4_groups[2] + "   使用率:" + lv4_groups[3];
                    CpCoreItem1 = lv4_groups[4].Value + " + " + lv4_groups[5] + " + " + lv4_groups[6] + "   使用率:" + lv4_groups[7];
                    CpCoreItem2 = lv4_groups[8].Value + " + " + lv4_groups[9] + " + " + lv4_groups[10] + "   使用率:" + lv4_groups[11];
                    CpBoot1 = "此英雄無法購買";
                    CpBoot2 = "此英雄無法購買";
                }
            }
            else 
            {
                MatchCollection matches_lv4 = Regex.Matches(matches_lv4_split[0].Value, @"(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?推薦構建[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?鞋[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc&#039;&gt;)([^&]+)[\s\S]*?(?<=strong>)(\d+.\d+%)");
                foreach (Match lv4 in matches_lv4)
                {
                    GroupCollection lv4_groups = lv4.Groups;
                    CpStartItem = lv4_groups[1].Value + " + " + lv4_groups[2] + "   使用率:" + lv4_groups[3];
                    CpCoreItem1 = lv4_groups[4].Value + " + " + lv4_groups[5] + " + " + lv4_groups[6] + "   使用率:" + lv4_groups[7];
                    CpCoreItem2 = lv4_groups[8].Value + " + " + lv4_groups[9] + " + " + lv4_groups[10] + "   使用率:" + lv4_groups[11];
                    CpBoot1 = lv4_groups[12].Value + "   使用率:" + lv4_groups[13];
                    CpBoot2 = lv4_groups[14].Value + "   使用率:" + lv4_groups[15];
                }
            }
            //Rune
            var CpRune = "";
            MatchCollection matches_lv5_split = Regex.Matches(html, @"tabItem ChampionKeystoneRune-1([\s\S]*?)(?=<\/tbody)");
            MatchCollection matches_lv5 = Regex.Matches(matches_lv5_split[0].Value, @"perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt="")([^""]+)[\s\S]*?(?<=strong>)(\d+.\d+%)");
            foreach(Match lv5 in matches_lv5)
            {
                GroupCollection lv5_groups = lv5.Groups;
                CpRune = lv5_groups[1] + " + " + lv5_groups[2] + " + " + lv5_groups[3] + " + " + lv5_groups[4] + " + " + lv5_groups[5] + " + " + lv5_groups[6] + "   使用率:" + lv5_groups[7];
            }
            return (CpLane, CpName, CpTier, CpSummonerSkill, CpSkill, CpStartItem, CpCoreItem1, CpCoreItem2, CpBoot1, CpBoot2, CpRune);
        }
    }
}
/*
 (?<=champion-stats-header__position--active)[\s\S]*?([\p{Han}]+)[\s\S]*?(?<=champion-stats__list__item)[\s\S]*?([\p{Han}]+)[\s\S]*?(?<=champion-stats__list__item)[\s\S]*?([\p{Han}]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=alt=""> <span>)([^<]?)[\s\S]*?(?<=alt=""> <span>)([^<]?)[\s\S]*?(?<=alt=""> <span>)([^<]?)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?推薦構建[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?鞋[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?(?<=#00cfbc'>)([^<]+)[\s\S]*?(?<=strong>)(\d+.\d+%)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt=")([^"]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt=")([^"]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt=")([^"]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt=")([^"]+)[\s\S]*?perk-page__item--active[\s\S]*?(?<=alt=")([^"]+)[\s\S]*?
 */
