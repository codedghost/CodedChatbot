using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class GiveawayEntryMap : EntityTypeConfiguration<GiveawayEntry>
    {
        public override void Map(EntityTypeBuilder<GiveawayEntry> builder)
        {
            builder.ToTable("GiveawayEntries");

            builder.HasKey(t => t.GiveawayEntryId);

            builder.Property(t => t.GiveawayEntryId).HasColumnName("GiveawayEntryId").IsRequired();
            builder.Property(t => t.Username).HasColumnName("Username").IsRequired();
            builder.Property(t => t.EntryTime).HasColumnName("EntryTime").IsRequired();
        }
    }
}
