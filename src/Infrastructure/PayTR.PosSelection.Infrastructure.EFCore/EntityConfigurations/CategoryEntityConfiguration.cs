using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;

namespace PayTR.PosSelection.Infrastructure.EFCore.EntityConfigurations
{
    public class PosRatioEntityConfiguration : IEntityTypeConfiguration<PosRatio>
    {
        public void Configure(EntityTypeBuilder<PosRatio> builder)
        {
            builder.ToTable("PosRatios");
            builder.HasKey(t => t.Id);
            builder.Property(b => b.Id).HasColumnType("UUID").ValueGeneratedNever().IsRequired();

            builder.Property(x => x.CreatedOn).IsRequired();

            builder.HasQueryFilter(qf => qf.DeletedOn == null);
        }
    }
}
