using System;

namespace Domain.Entities.Audit
{
    public class Audit
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }  
        public string UserName { get; set; }  
        public string Table { get; set; }
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}