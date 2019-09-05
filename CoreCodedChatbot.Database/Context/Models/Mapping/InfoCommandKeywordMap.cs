using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class InfoCommandKeywordMap : EntityTypeConfiguration<InfoCommandKeyword>
    {
        public override void Map(EntityTypeBuilder<InfoCommandKeyword> builder)
        {
            builder.ToTable("InfoCommandKeywords");

            builder.HasKey(t => new {t.InfoCommandId, t.InfoCommandKeywordText});

            builder.Property(t => t.InfoCommandId).HasColumnName("InfoCommandId").IsRequired();
            builder.Property(t => t.InfoCommandKeywordText).HasColumnName("InfoCommandKeywordText").IsRequired();

            builder.HasOne(infoCommandKeyword => infoCommandKeyword.InfoCommand)
                .WithMany(infoCommand => infoCommand.InfoCommandKeywords)
                .HasForeignKey(infoCommandKeyword => infoCommandKeyword.InfoCommandId);
        }
    }
}
