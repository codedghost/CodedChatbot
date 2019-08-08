using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class SongRequestMap : EntityTypeConfiguration<SongRequest>
    {
        public override void Map(EntityTypeBuilder<SongRequest> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder) builder, "SongRequests");

            builder.Property(t => t.SongRequestId).HasColumnName("SongRequestId").IsRequired();
            builder.Property(t => t.SongId).HasColumnName("SongId").IsRequired();
            builder.Property(t => t.RequestUsername).HasColumnName("RequestUsername").HasMaxLength(255).IsRequired();

            //builder.HasOne(t => t.Song).WithMany(o => o.SongRequests).HasForeignKey(k => k.SongId);
        }
    }
}
