namespace HonzaBotner.Discord.Services.Options
{
    public class ButtonOptions
    {
        public static string ConfigName => "ButtonOptions";

        public string VerificationId { get; set; } = "verification-user";
        public string StaffVerificationId { get; set; } = "verification-staff";
        public string StaffRemoveRoleId { get; set; } = "verification-remove-staff";
    }
}
