using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class InfoCommandMap : EntityTypeConfiguration<InfoCommand>
    {
        public override void Map(EntityTypeBuilder<InfoCommand> builder)
        {
            builder.ToTable("InfoCommands");

            builder.HasKey(t => t.InfoCommandId);

            builder.Property(t => t.InfoCommandId).HasColumnName("InfoCommandId").IsRequired();
            builder.Property(t => t.InfoText).HasColumnName("InfoText").IsRequired();
            builder.Property(t => t.InfoHelpText).HasColumnName("InfoHelpText").IsRequired();

            builder.HasMany(infoCommand => infoCommand.InfoCommandKeywords)
                .WithOne(infoCommandKeyword => infoCommandKeyword.InfoCommand)
                .HasForeignKey(infoCommandKeyword => infoCommandKeyword.InfoCommandId);
        }
    }
}
