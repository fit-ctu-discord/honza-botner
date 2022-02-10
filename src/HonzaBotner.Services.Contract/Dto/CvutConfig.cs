namespace HonzaBotner.Services.Contract.Dto;

public class CvutConfig
{
    public const string ConfigName = "Cvut";

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public string? AppBaseUrl { get; set; }
}
