using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> user)
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();
            user.Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired();
            user.Property(u => u.OtherNames)
                .HasMaxLength(50)
                .IsRequired(false);
            user.Property(u => u.PasswordHash)
                .HasMaxLength(256)
                .IsRequired();
            user.Property(u => u.Email)
                .HasMaxLength(320)
                .IsRequired(); //TODO: can email constraints/validation be better?
            user.Property(u => u.Phone)
                .HasMaxLength(15)
                .IsRequired(false); //TODO: can phone constraints/validation be better?

            //TODO: Make all enums in other entities convert to strings like below
            user.Property(u => u.UserType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            user.Property(u => u.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            user.Property(u => u.CreatedAt)
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            user.Property(u => u.UpdatedAt)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);



            user.HasIndex(u => u.Email).IsUnique();
            //user.HasIndex(u => u.UserName);
        }
    }
}
