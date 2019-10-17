using System;

using CoreCodedChatbot.Database.Context.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoreCodedChatbot.Database.Context.Interfaces
{
    public interface IChatbotContext : IDisposable
    {
        DbSet<Setting> Settings { get; set; }
        DbSet<SongRequest> SongRequests { get; set; }
        DbSet<Song> Songs { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<SongGuessingRecord> SongGuessingRecords { get; set; }
        DbSet<SongPercentageGuess> SongPercentageGuesses { get; set; }
        DbSet<InfoCommand> InfoCommands { get; set; }
        DbSet<InfoCommandKeyword> InfoCommandKeywords { get; set; }
        DbSet<StreamStatus> StreamStatuses { get; set; }

        int SaveChanges();

        EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
            where TEntity : class;
    }
}