using FluentValidation;

namespace GameLibraryAPI.Dtos
{
    public class UpdateGameDtoValidator : AbstractValidator<UpdateGameDto>
    {
        public UpdateGameDtoValidator()
        {
            RuleFor(g => g.Name)
                .NotEmpty().WithMessage("Game name is required.")
                .MaximumLength(100).WithMessage("Game name cannot exceed 100 characters.");

            RuleFor(g => g.Genre)
                .NotEmpty().WithMessage("Genre is required.");

            RuleFor(g => g.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(g => g.ReleaseDate)
                .NotEmpty().WithMessage("Release date is required.")
                .Must(BeValidYear).WithMessage("Please enter a valid 4-digit release year.");
        }

        private bool BeValidYear(DateOnly date)
        {
            int year = date.Year;
            return year >= 1950 && year <= DateTime.UtcNow.Year;
        }
    }
}
