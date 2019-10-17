using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class StreamStatusMap : EntityTypeConfiguration<StreamStatus>
    {
        public override void Map(EntityTypeBuilder<StreamStatus> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder) builder, "StreamStatuses");

            builder.Property(t => t.StreamStatusId).HasColumnName("StreamStatusId").IsRequired();
            builder.Property(t => t.BroadcasterUsername).HasColumnName("BroadcasterUsername").IsRequired();
            builder.Property(t => t.IsOnline).HasColumnName("IsOnline").IsRequired();
        }
    }
}