using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class SongGuessingRecordMap : EntityTypeConfiguration<SongGuessingRecord>
    {
        public override void Map(EntityTypeBuilder<SongGuessingRecord> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder) builder, "SongGuessingRecord");

            builder.Property(t => t.SongGuessingRecordId).HasColumnName("SongGuessingRecordId").IsRequired();
            builder.Property(t => t.SongDetails).HasColumnName("SongDetails").IsRequired();
            builder.Property(t => t.UsersCanGuess).HasColumnName("UsersCanGuess").IsRequired();
            builder.Property(t => t.IsInProgress).HasColumnName("IsInProgress").IsRequired();
            builder.Property(t => t.FinalPercentage).HasColumnName("FinalPercentage");
        }
    }
}
