using Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Application.Common
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(
            IOptions<SieveOptions> options) 
            : base(options)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            mapper.Property<User>(p => p.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(p => p.FirstName)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(p => p.LastName)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(p => p.FullName)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(p => p.Email)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(p => p.LastName)
                .CanFilter()
                .CanSort();
            
            mapper.Property<Role>(p => p.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<Role>(p => p.Name)
                .CanFilter()
                .CanSort();

            return mapper;
        }
    }
}