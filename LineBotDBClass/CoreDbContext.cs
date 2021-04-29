using LinBotDBClass.Models;
using LineBotDBClass.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinBotDBClass
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options)
                : base(options)
        {
        }

        public DbSet<ChampionState> ChampionState { get; set; }
        public DbSet<ChampionTopInfo> ChampionTopInfos { get; set; }
        public DbSet<ChampionMidInfo> ChampionMidInfos { get; set; }
        public DbSet<ChampionAdcInfo> ChampionAdcInfos { get; set; }
        public DbSet<ChampionSupInfo> ChampionSupInfos { get; set; }
        public DbSet<ChampionJunInfo> ChampionJunInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var mapper = new CoreMapper();

            modelBuilder.Entity<ChampionState>(entity => mapper.Map(entity));
            modelBuilder.Entity<ChampionTopInfo>(entity => mapper.Map(entity));
            modelBuilder.Entity<ChampionMidInfo>(entity => mapper.Map(entity));
            modelBuilder.Entity<ChampionAdcInfo>(entity => mapper.Map(entity));
            modelBuilder.Entity<ChampionSupInfo>(entity => mapper.Map(entity));
            modelBuilder.Entity<ChampionJunInfo>(entity => mapper.Map(entity));
        }
    }
}
//將Model及Mapping加入就能使用
