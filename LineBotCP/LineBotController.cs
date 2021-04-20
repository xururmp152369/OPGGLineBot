using LinBotDBClass;
using Line.Messaging;
using LineBotCP.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LineBotCP
{
    [Route("api/linebot")]
    public class LineBotController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;
        private readonly LineBotConfig _lineBotConfig;
        private readonly ILogger _logger;
        private readonly CoreDbContext _db;

        public LineBotController(IServiceProvider serviceProvider,
            LineBotConfig lineBotConfig,
            ILogger<LineBotController> logger,
            CoreDbContext db)
        {
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _httpContext = _httpContextAccessor.HttpContext;
            _lineBotConfig = lineBotConfig;
            _logger = logger;
            _db = db;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Post()
        {
            try
            {
                var events = await _httpContext.Request.GetWebhookEventsAsync(_lineBotConfig.channelSecret);
                var lineMessagingClient = new LineMessagingClient(_lineBotConfig.accessToken);

                var lineBotApp = new LineBotApp(lineMessagingClient, _db);
                await lineBotApp.RunAsync(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(JsonConvert.SerializeObject(ex));
            }
            return Ok();
        }
    }
}
