using LinBotDBClass;
using LinBotDBClass.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LineBotCP
{
    public class LineBotApp : WebhookApplication
    {
        private readonly LineMessagingClient _messagingClient;
        private readonly CoreDbContext _db;

        public LineBotApp(LineMessagingClient lineMessagingClient, CoreDbContext db)
        {
            _messagingClient = lineMessagingClient;
            _db = db;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            var result = null as List<ISendMessage>;

            switch (ev.Message)
            {
                //文字訊息
                case TextEventMessage textMessage:
                    {
                        //頻道Id
                        var channelId = ev.Source.Id;
                        //使用者Id
                        var userId = ev.Source.UserId;
                        
                        //當使用者輸入查詢時（名稱）
                        {
                            var regex = new Regex(@"(?<=查詢[\s])([\S]*)", RegexOptions.IgnoreCase);
                            var match = regex.Match(textMessage.Text);
                            if (match.Success)
                            {
                                //取得第一筆資料
                                var championState = await _db.ChampionState
                                    .Where(it => it.CpName == match.Value)
                                    .FirstOrDefaultAsync();

                                //回傳訊息
                                result = new List<ISendMessage>
                                {
                                    new TextMessage(
                                        "查詢的英雄名為: " + championState.CpName
                                        + "\n查詢的英雄英文名為: " + championState.CpNameEn
                                        + "\n推薦位置為: " + championState.CpPosition)
                                };
                                break;
                            }
                        }
                        //當使用者輸入查詢（路線 名稱）
                        {
                            var regex = new Regex(@"(?<=詳細)[\s]*?([\S]+)[\s]*?([\S]+)", RegexOptions.IgnoreCase);
                            MatchCollection match = regex.Matches(textMessage.Text);
                            foreach (Match res in match)
                            {
                                GroupCollection res_groups = res.Groups;
                                switch (res_groups[1].Value)
                                {
                                    case "上路":
                                        var TopCp = await _db.ChampionTopInfos
                                            .Where(it => it.CpName == res_groups[2].Value)
                                            .FirstOrDefaultAsync();
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage(
                                                "路線: " + TopCp.CpLane
                                                + "\n名稱: " + TopCp.CpName
                                                + "\n階級: " + TopCp.CpTier
                                                + "\n推薦召喚師技能: " + TopCp.CpSummonerSkill
                                                + "\n推薦技能順序: " + TopCp.CpSkill
                                                + "\n推薦出裝↓"
                                                + "\n起始裝備: " + TopCp.CpStartItem
                                                + "\n核心裝備: " + TopCp.CpCoreItem1 + " or " + TopCp.CpCoreItem2
                                                + "\n推薦鞋子: " + TopCp.CpBoot1 + " or " + TopCp.CpBoot2
                                                + "\n推薦推薦符文: " + TopCp.CpRune)
                                        };
                                        break;
                                    case "中路":
                                        var MidCp = await _db.ChampionMidInfos
                                            .Where(it => it.CpName == res_groups[2].Value)
                                            .FirstOrDefaultAsync();
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage(
                                                "路線: " + MidCp.CpLane
                                                + "\n名稱: " + MidCp.CpName
                                                + "\n階級: " + MidCp.CpTier
                                                + "\n推薦召喚師技能: " + MidCp.CpSummonerSkill
                                                + "\n推薦技能順序: " + MidCp.CpSkill
                                                + "\n推薦出裝↓"
                                                + "\n起始裝備: " + MidCp.CpStartItem
                                                + "\n核心裝備: " + MidCp.CpCoreItem1 + " or " + MidCp.CpCoreItem2
                                                + "\n推薦鞋子: " + MidCp.CpBoot1 + " or " + MidCp.CpBoot2
                                                + "\n推薦推薦符文: " + MidCp.CpRune)
                                        };
                                        break;
                                    case "下路":
                                        var AdcCp = await _db.ChampionAdcInfos
                                            .Where(it => it.CpName == res_groups[2].Value)
                                            .FirstOrDefaultAsync();
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage(
                                                "路線: " + AdcCp.CpLane
                                                + "\n名稱: " + AdcCp.CpName
                                                + "\n階級: " + AdcCp.CpTier
                                                + "\n推薦召喚師技能: " + AdcCp.CpSummonerSkill
                                                + "\n推薦技能順序: " + AdcCp.CpSkill
                                                + "\n推薦出裝↓"
                                                + "\n起始裝備: " + AdcCp.CpStartItem
                                                + "\n核心裝備: " + AdcCp.CpCoreItem1 + " or " + AdcCp.CpCoreItem2
                                                + "\n推薦鞋子: " + AdcCp.CpBoot1 + " or " + AdcCp.CpBoot2
                                                + "\n推薦推薦符文: " + AdcCp.CpRune)
                                        };
                                        break;
                                    case "輔助":
                                        var SupCp = await _db.ChampionSupInfos
                                            .Where(it => it.CpName == res_groups[2].Value)
                                            .FirstOrDefaultAsync();
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage(
                                                "路線: " + SupCp.CpLane
                                                + "\n名稱: " + SupCp.CpName
                                                + "\n階級: " + SupCp.CpTier
                                                + "\n推薦召喚師技能: " + SupCp.CpSummonerSkill
                                                + "\n推薦技能順序: " + SupCp.CpSkill
                                                + "\n推薦出裝↓"
                                                + "\n起始裝備: " + SupCp.CpStartItem
                                                + "\n核心裝備: " + SupCp.CpCoreItem1 + " or " + SupCp.CpCoreItem2
                                                + "\n推薦鞋子: " + SupCp.CpBoot1 + " or " + SupCp.CpBoot2
                                                + "\n推薦推薦符文: " + SupCp.CpRune)
                                        };
                                        break;
                                    case "打野":
                                        var JunCp = await _db.ChampionJunInfos
                                            .Where(it => it.CpName == res_groups[2].Value)
                                            .FirstOrDefaultAsync();
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage(
                                                "路線: " + JunCp.CpLane
                                                + "\n名稱: " + JunCp.CpName
                                                + "\n階級: " + JunCp.CpTier
                                                + "\n推薦召喚師技能: " + JunCp.CpSummonerSkill
                                                + "\n推薦技能順序: " + JunCp.CpSkill
                                                + "\n推薦出裝↓"
                                                + "\n起始裝備: " + JunCp.CpStartItem
                                                + "\n核心裝備: " + JunCp.CpCoreItem1 + " or " + JunCp.CpCoreItem2
                                                + "\n推薦鞋子: " + JunCp.CpBoot1 + " or " + JunCp.CpBoot2
                                                + "\n推薦推薦符文: " + JunCp.CpRune)
                                        };
                                        break;
                                    default:
                                        result = new List<ISendMessage>
                                        {
                                            new TextMessage("請重複確認是否有錯字！")
                                        };
                                        break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (result != null)
                await _messagingClient.ReplyMessageAsync(ev.ReplyToken, result);
        }
    }
}
