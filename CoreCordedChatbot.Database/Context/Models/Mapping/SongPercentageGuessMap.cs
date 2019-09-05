using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class SongPercentageGuessMap : EntityTypeConfiguration<SongPercentageGuess>
    {
        public override void Map(EntityTypeBuilder<SongPercentageGuess> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder) builder, "SongPercentageGuess");

            builder.Property(t => t.SongPercentageGuessId).HasColumnName("SongPercentageGuessId").IsRequired();
            builder.Property(t => t.SongGuessingRecordId).HasColumnName("SongGuessingRecordId").IsRequired();
            builder.Property(t => t.Username).HasColumnName("Username").IsRequired();
            builder.Property(t => t.Guess).HasColumnName("Guess").IsRequired();

            builder.HasOne(pg => pg.SongGuessingRecord)
                .WithMany(sg => sg.SongPercentageGuesses)
                .HasForeignKey(pg => pg.SongGuessingRecordId);
        }
    }
}
