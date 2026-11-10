using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinguaContext.Work;

public class WorkConfiguration : IEntityTypeConfiguration<Work>
{
    public void Configure(EntityTypeBuilder<Work> builder)
    {
        builder
            .ToTable("works")
            .HasKey(w => w.Id);
        
        builder
            .Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder
            .Property(w => w.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}