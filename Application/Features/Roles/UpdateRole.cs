using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ErrorHandling;
using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles
{
    public class UpdateRole
    {
        public class Command : IRequest<RoleDto>
        {
            private Command() {}

            public Command(
                int id,
                string name,
                List<Permissions> permissions)
            {
                Id = id;
                Name = name;
                Permissions = permissions;
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public List<Permissions> Permissions { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
                RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(50);
            }
        }

        public class Handler : IRequestHandler<Command, RoleDto>
        {
            private readonly IApplicationDbContext _dbContext;

            public Handler(
                IApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<RoleDto> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                
                if (await _dbContext.Roles.AnyAsync(
                    x => x.Name == request.Name && x.Id != request.Id,
                    cancellationToken))
                {
                    throw new HttpException(
                        HttpStatusCode.BadRequest,
                        new
                        {
                            Error = $"There is already a role with name {request.Name}."
                        });
                }

                var role = await _dbContext.Roles.FirstAsync(
                    x => x.Id == request.Id,
                    cancellationToken);
                role.Name = request.Name;
                role.AddPermissions(request.Permissions);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new RoleDto(
                    role.Id,
                    role.Name,
                    string.Join(
                        ", ",
                        role.PermissionsInRole));
            }
        }
    }
}