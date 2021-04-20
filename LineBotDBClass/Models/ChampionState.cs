using System;
using System.ComponentModel.DataAnnotations;

namespace LinBotDBClass.Models
{
    public class ChampionState
    {
        [Key]
        public int Id { get; set; }
        public string CpPosition { get; set; }
        public int CpState  { get; set; }
        public string CpName { get; set; }
        public string CpWinrate { get; set; }
        public string CpPickrate { get; set; }
        public string CpTier { get; set; }
    }
}
//若加上virtual 則在LINQ中使用.include()方法時會產生如SQL中JOIN的語法
