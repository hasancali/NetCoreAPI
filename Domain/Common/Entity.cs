using System;

namespace Domain.Common
{
    public class Entity
    {
        protected Entity() { }

        public int Id { get; set; }
        [DoNotAudit]
        public DateTime CreatedAt { get; set; }
        [DoNotAudit]
        public string CreatedBy { get; set; }
        [DoNotAudit]
        public DateTime UpdatedAt { get; set; }
        [DoNotAudit]
        public string UpdatedBy { get; set; }

        public bool Archived { get; set; }
        
        public void Archive()
        {
            Archived = true;
        }

        public void UnArchive()
        {
            Archived = false;
        }
    }
}