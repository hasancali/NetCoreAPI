using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles
{
    public class DeleteRole
    {
        public class Command : IRequest
        {
            public Command(
                int id)
            {
                Id = id;
            }

            public int Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Unit>
        {
            private readonly IApplicationDbContext _dbContext;

            public Handler(
                IApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Unit> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var role = await _dbContext.Roles.FirstAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

                if (role == null)
                {
                    throw new Exception("Not Found");
                }

                role.Archive();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}