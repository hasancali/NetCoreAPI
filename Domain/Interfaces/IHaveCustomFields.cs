using Domain.Common;

namespace Domain.Interfaces
{
    public interface IHaveCustomFields
    {
        public CustomField[] CustomFields { get; set; }
    }
}