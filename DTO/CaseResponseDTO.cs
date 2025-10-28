namespace JwtApp.DTO
{
    public class CaseResponseDTO
    {
        public Guid Id { get; set; }
        public string CaseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

