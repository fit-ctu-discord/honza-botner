using System.ComponentModel.DataAnnotations;

namespace HonzaBotner.Database
{
    public class Verification
    {
        [Key]
        public ulong UserId { get; set; }
        public string AuthId { get; set; }

        public override string ToString() => $"Discord: <@!{UserId}>, auth hash: {AuthId}";
    }
}
