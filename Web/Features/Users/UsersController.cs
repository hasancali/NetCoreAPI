using System.Net;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Auth.Authorization;
using Application.Common.ErrorHandling;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Features.Users;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Features.Users
{
    [ApiVersion("1")]
    [Route("api/v{version:ApiVersion}/users")]
    public class UsersController
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(
            IMediator mediator,
            ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [HasPermission(
            Permissions.ReadUsers,
            Permissions.EditUsers)]
        [ProducesResponseType(typeof(UserDetailsDto), 200)]
        public async Task<object> Get(
            ListResourceParameters resourceParams)
        {
            return await _mediator.Send(new UserList.Query(resourceParams));
        }

        [HttpPut]
        public async Task<UserDetailsDto> Edit(
            [FromBody] UpdateUser.Command command)
        {
            if (!_currentUserService.IsAllowed(Permissions.EditUsers))
                throw new HttpException(HttpStatusCode.Forbidden);

            return await _mediator.Send(command);
        }


        [HttpDelete("{id}")]
        public async Task Delete(
            int id)
        {
            _currentUserService.Authorize(Permissions.EditUsers);
            await _mediator.Send(new DeleteUser.Command(id));
        }
    }
}