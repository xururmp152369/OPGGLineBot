using LineBotDBClass.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinBotDBClass.Models
{
    public class ChampionState
    {
        [Key]
        public int Id { get; set; }
        public string CpName { get; set; }
        public string CpNameEn { get; set; }
        public string CpUri { get; set; }
        public string CpPosition { get; set; }
        public string CpIntroduce { get; set; }
        public string CpNickname { get; set; }

        public virtual ICollection<ChampionTopInfo> ChampionTopInfos { get; set; }
        public virtual ICollection<ChampionMidInfo> ChampionMidInfos { get; set; }
        public virtual ICollection<ChampionAdcInfo> ChampionAdcInfos { get; set; }
        public virtual ICollection<ChampionSupInfo> ChampionSupInfos { get; set; }
        public virtual ICollection<ChampionJunInfo> ChampionJunInfos { get; set; }
    }
}
//若加上virtual 則在LINQ中使用.include()方法時會產生如SQL中JOIN的語法
