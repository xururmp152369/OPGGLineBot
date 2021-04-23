using LinBotDBClass;
using LinBotDBClass.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
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

                        //當使用者輸入訂閱時
                        {
                            // regex => \s 符合任何空白字元。 \S 符合任何非空白字元。
                            var regex = new Regex(@"(?<=新增[\s])([\S]*)", RegexOptions.IgnoreCase);
                            var match = regex.Match(textMessage.Text);
                            if (match.Success)
                            {
                                //新增一筆資料
                                var championState = new ChampionState
                                {
                                    CpName = $"{match.Value}",
                                    CpNameEn = "Test",
                                    CpUrl = "http://test",
                                    CpPosition = "上路"
                                };
                                _db.ChampionState.Add(championState);
                                await _db.SaveChangesAsync();

                                //回傳訊息
                                result = new List<ISendMessage>
                                {
                                    new TextMessage("新增成功!!")
                                };
                                break;
                            }
                        }
                        //當使用者輸入查詢時
                        {
                            var regex = new Regex(@"^查詢[\s]*$", RegexOptions.IgnoreCase);
                            if (regex.IsMatch(textMessage.Text))
                            {
                                //取得第一筆資料
                                var championState = await _db.ChampionState.FirstOrDefaultAsync();

                                //回傳訊息
                                result = new List<ISendMessage>
                                {
                                    new TextMessage(JsonConvert.SerializeObject(championState))
                                };
                                break;
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
