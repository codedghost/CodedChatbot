using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreCodedChatbot.Database.Context.Models.Mapping
{
    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public override void Map(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("Settings");

            builder.HasKey(t => t.SettingId);

            builder.Property(t => t.SettingId).HasColumnName("SettingId").IsRequired();
            builder.Property(t => t.SettingName).HasColumnName("SettingName").IsRequired();
            builder.Property(t => t.SettingValue).HasColumnName("SettingValue").IsRequired();
        }
    }
}
