using System.IO;

using CoreCodedChatbot.Database.Context.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Database.Context.Models.Mapping;

namespace CoreCodedChatbot.Database.Context
{
    public class ChatbotContext : DbContext, IChatbotContext
    {
        public ChatbotContext()
            :base()
        {
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<SongRequest> SongRequests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<GiveawayEntry> GiveawayEntries { get; set; }

        private IConfigurationRoot ConfigRoot { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");

            ConfigRoot = builder.Build();
            
            // Reconstructing path for platform independency
            var dbConn = Path.GetFullPath(ConfigRoot["LocalDbLocation"]);

            optionsBuilder.UseSqlite($"FileName={dbConn}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddConfiguration(new SongMap());
            modelBuilder.AddConfiguration(new SongRequestMap());
            modelBuilder.AddConfiguration(new UserMap());
            modelBuilder.AddConfiguration(new SettingMap());
            modelBuilder.AddConfiguration(new GiveawayEntryMap());
        }
    }
}
