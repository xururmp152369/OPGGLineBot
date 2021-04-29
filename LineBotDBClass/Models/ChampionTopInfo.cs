using LinBotDBClass.Models;
using System.ComponentModel.DataAnnotations;

namespace LineBotDBClass.Models
{
    public class ChampionTopInfo
    {
        [Key]
        public int Id { get; set; }
        public string CpLane {get; set; }
        public string CpName {get; set; }
        public string CpTier { get; set; }
        public string CpSummonerSkill { get; set; }
        public string CpSkill { get; set; }
        public string CpStartItem { get; set; }
        public string CpCoreItem1 { get; set; }
        public string CpCoreItem2 { get; set; }
        public string CpBoot1 { get; set; }
        public string CpBoot2 { get; set; }
        public string CpRune { get; set; }

        public virtual ChampionState ChampionState { get; set; }
    }
}
