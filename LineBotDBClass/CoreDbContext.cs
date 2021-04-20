using LinBotDBClass.Models;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var mapper = new CoreMapper();

            modelBuilder.Entity<ChampionState>(entity => mapper.Map(entity));
        }
    }
}
//將Model及Mapping加入就能使用
