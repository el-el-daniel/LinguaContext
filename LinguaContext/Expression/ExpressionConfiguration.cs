using LinguaContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorkEntity = LinguaContext.Work.Work;

namespace LinguaContext.Expression;

public class ExpressionConfiguration : IEntityTypeConfiguration<Expression>
{
    public void Configure(EntityTypeBuilder<Expression> builder)
    {
        builder
            .ToTable("expressions")
            .HasKey(e => e.Id);

        builder
            .Property(e => e.Text)
            .HasMaxLength(300)
            .IsRequired();
        
        builder
            .Property(e => e.Translation)
            .HasMaxLength(1000);
        
        builder
            .Property(e => e.Context)
            .HasMaxLength(2000)
            .IsRequired();
        
        builder
            .HasOne<WorkEntity>()
            .WithMany()
            .HasForeignKey(e => e.WorkId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DbConstraints.ExpressionWorkForeignKey);
    }
}