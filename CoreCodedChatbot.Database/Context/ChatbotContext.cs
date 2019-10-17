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
            : base()
        {
        }

        public ChatbotContext(DbContextOptions<ChatbotContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<SongRequest> SongRequests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SongGuessingRecord> SongGuessingRecords { get; set; }
        public DbSet<SongPercentageGuess> SongPercentageGuesses { get; set; }
        public DbSet<InfoCommand> InfoCommands { get; set; }
        public DbSet<InfoCommandKeyword> InfoCommandKeywords { get; set; }
        public DbSet<StreamStatus> StreamStatuses { get; set; }

        private IConfigurationRoot ConfigRoot { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", true);

            ConfigRoot = builder.Build();
            
            // Reconstructing path for platform independency
            var dbConn = Path.GetFullPath(ConfigRoot["LocalDbLocation"]);

            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite($"FileName={dbConn}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddConfiguration(new SongMap());
            modelBuilder.AddConfiguration(new SongRequestMap());
            modelBuilder.AddConfiguration(new UserMap());
            modelBuilder.AddConfiguration(new SettingMap());
            modelBuilder.AddConfiguration(new SongGuessingRecordMap());
            modelBuilder.AddConfiguration(new SongPercentageGuessMap());
            modelBuilder.AddConfiguration(new InfoCommandMap());
            modelBuilder.AddConfiguration(new InfoCommandKeywordMap());
            modelBuilder.AddConfiguration(new StreamStatusMap());
        }
    }
}
