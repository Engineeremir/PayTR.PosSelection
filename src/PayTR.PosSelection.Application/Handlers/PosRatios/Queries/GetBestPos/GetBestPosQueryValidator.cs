using FluentValidation;

namespace PayTR.PosSelection.Application.Handlers.PosRatios.Queries.GetBestPos
{
    public class GetBestPosQueryValidator : AbstractValidator<GetBestPosQuery>
    {
        public GetBestPosQueryValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Installment)
                .GreaterThan(0)
                .WithMessage("Installment must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency cannot be empty.");

            RuleFor(x => x.CardType)
                .Must(x => string.IsNullOrWhiteSpace(x) || !string.IsNullOrWhiteSpace(x))
                .WithMessage("CardType cannot be empty.");

            RuleFor(x => x.CardBrand)
                .Must(x => string.IsNullOrWhiteSpace(x) || !string.IsNullOrWhiteSpace(x))
                .WithMessage("CardBrand cannot be empty.");
        }
    }
}
