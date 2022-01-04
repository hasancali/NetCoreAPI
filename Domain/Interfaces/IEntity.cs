using System;

namespace Domain.Interfaces
{
    public interface IEntity
    {
        int Id { get; }
        DateTime CreatedAt { get; }
        string CreatedBy { get; set; }
        DateTime UpdatedAt { get; }
        string UpdatedBy { get; set; }
        bool Archived { get; }
        void Archive();
        void UnArchive();
    }
}