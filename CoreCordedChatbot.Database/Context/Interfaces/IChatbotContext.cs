using System;

using CoreCodedChatbot.Database.Context.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoreCodedChatbot.Database.Context.Interfaces
{
    public interface IChatbotContext : IDisposable
    {
        DbSet<GiveawayEntry> GiveawayEntries { get; set; }
        DbSet<Setting> Settings { get; set; }
        DbSet<SongRequest> SongRequests { get; set; }
        DbSet<Song> Songs { get; set; }
        DbSet<User> Users { get; set; }

        int SaveChanges();

        EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
            where TEntity : class;
    }
}