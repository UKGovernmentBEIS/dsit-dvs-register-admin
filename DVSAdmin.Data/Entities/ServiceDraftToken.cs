using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data.Entities
{
    [Index(nameof(TokenId))]
    [Index(nameof(Token))]
    public class ServiceDraftToken 
    {
        public ServiceDraftToken() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
       
        public string TokenId { get; set; }
        public string Token {  get; set; }
        [ForeignKey("ServiceDraft")]
        public int ServiceDraftId { get; set; }
        public ServiceDraft ServiceDraft { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}