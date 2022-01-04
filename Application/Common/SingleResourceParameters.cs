using Sieve.Models;

namespace Application.Common
{
    public class SingleResourceParameters
    {
        public int Id { get; set; }
        public string Fields { get; set; }
    }

    public class ListResourceParameters : SieveModel
    {
        public string Fields { get; set; }
    }
}