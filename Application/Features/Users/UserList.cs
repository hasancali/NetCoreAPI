using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace Application.Features.Users
{
    public class UserList
    {
        public class Query : IRequest<object>
        {
            public ListResourceParameters ResourceParams { get; }

            public Query(
                ListResourceParameters resourceParams)
            {
                ResourceParams = resourceParams;
            }
        }

        public class QueryHandler : IRequestHandler<Query, object>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly ISieveProcessor _sieveProcessor;

            public QueryHandler(
                IApplicationDbContext dbContext,
                ISieveProcessor sieveProcessor)
            {
                _dbContext = dbContext;
                _sieveProcessor = sieveProcessor;
            }

            //Implement Paging
            public async Task<object> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var users = _dbContext.Users.AsNoTracking()
                    .Include(x => x.Roles)
                    .ThenInclude(x => x.Role);

                //var count = _sieveProcessor.Apply(message.ResourceParams, users, applyPagination: false).Count();

                var filteredUsers = await _sieveProcessor.Apply(
                    message.ResourceParams,
                    users).ToListAsync(cancellationToken);

                var userDetailsDtos = filteredUsers
                    .Select(
                        user => new UserDetailsDto(
                            user.Id,
                            user.FirstName,
                            user.LastName,
                            user.FullName,
                            user.Email,
                            string.Join(
                                ", ",
                                user.Roles.Select(r => r.Role.Name)),
                            user.CustomFields,
                            new AddressDetailDto(
                                user?.Address?.Address1,
                                user?.Address?.Address2,
                                user?.Address?.Street,
                                user?.Address?.City,
                                user?.Address?.State,
                                user?.Address?.Country,
                                user?.Address?.ZipCode
                            ))
                    );
                return userDetailsDtos.ShapeData(message.ResourceParams.Fields);
            }
        }
    }
}