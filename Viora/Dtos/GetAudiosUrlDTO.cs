using Microsoft.AspNetCore.Mvc;

namespace Viora.Dtos
{
    public class GetAudiosUrlDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Url { get; set; }
       
    }
}
