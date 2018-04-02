using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class SongMap : EntityTypeConfiguration<Song>
    {
        public override void Map(EntityTypeBuilder<Song> builder)
        {
            builder.ToTable("Songs");

            builder.HasKey(t => t.SongId);

            builder.Property(t => t.SongId).HasColumnName("SongId").IsRequired();
            builder.Property(t => t.SongName).HasColumnName("SongName").IsRequired();
            builder.Property(t => t.SongArtist).HasColumnName("SongArtist").IsRequired();

            //builder.HasMany(t => t.SongRequests).WithOne(o => o.Song).HasForeignKey(k => k.SongId);
        }
    }
}
