using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data.Entities
{
    [Index(nameof(TokenId))]
    [Index(nameof(Token))]
    public class ProviderDraftToken 
    {
        public ProviderDraftToken() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
       
        public string TokenId { get; set; }
        public string Token {  get; set; }
        [ForeignKey("ProviderDraft")]
        public int ProviderDraftId { get; set; }
        public ProviderProfileDraft ProviderProfileDraft { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}