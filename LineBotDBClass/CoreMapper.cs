using LinBotDBClass.Models;
using LineBotDBClass.Models;
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
        public void Map(EntityTypeBuilder<ChampionTopInfo> entity)
        {
            entity.ToTable("championtopinfo");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.ChampionState).WithMany(p => p.ChampionTopInfos)
                .HasForeignKey(p => p.CpName).HasPrincipalKey(p => p.CpName);
        }
        public void Map(EntityTypeBuilder<ChampionMidInfo> entity)
        {
            entity.ToTable("championmidinfo");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.ChampionState).WithMany(p => p.ChampionMidInfos)
                .HasForeignKey(p => p.CpName).HasPrincipalKey(p => p.CpName);
        }
        public void Map(EntityTypeBuilder<ChampionAdcInfo> entity)
        {
            entity.ToTable("championadcinfo");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.ChampionState).WithMany(p => p.ChampionAdcInfos)
                .HasForeignKey(p => p.CpName).HasPrincipalKey(p => p.CpName);
        }
        public void Map(EntityTypeBuilder<ChampionSupInfo> entity)
        {
            entity.ToTable("championsupinfo");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.ChampionState).WithMany(p => p.ChampionSupInfos)
                .HasForeignKey(p => p.CpName).HasPrincipalKey(p => p.CpName);
        }
        public void Map(EntityTypeBuilder<ChampionJunInfo> entity)
        {
            entity.ToTable("championjuninfo");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.ChampionState).WithMany(p => p.ChampionJunInfos)
                .HasForeignKey(p => p.CpName).HasPrincipalKey(p => p.CpName);
        }
    }
}
//設定相關程式（Fluent API）