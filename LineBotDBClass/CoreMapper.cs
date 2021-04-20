using LinBotDBClass.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinBotDBClass
{
    public class CoreMapper
    {
        public void Map(EntityTypeBuilder<ChampionState> entity)
        {
            entity.ToTable("championstate");
            entity.HasKey(p => p.Id);
        }
    }
}
//設定相關程式（Fluent API）