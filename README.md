# NetCoreAPI
- [.NET Core 3.0](https://github.com/dotnet/core)
- [Fluent Validation](https://github.com/JeremySkinner/FluentValidation)
- [MediatR](https://github.com/jbogard/MediatR)
- [Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore).
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for Swagger
- [ASP.NET Core JWT Bearer Authentication](https://github.com/aspnet/Security) for [JWT](https://jwt.io/) authentication with support for [refresh tokens](https://tools.ietf.org/html/rfc6749#section-1.5).
- [Serilog](https://github.com/serilog/serilog) for logging
- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit) to add rate limiting functionality
- [Sieve](https://github.com/Biarity/Sieve) to add paging, sorting and filtering functionality 

# Features

#### Jwt and Refresh Tokens

Authentication in Exelor is done by issuing an access token with the users claims in it. You'll need to login to the application with a username and password and if successful, you'll get an access token that is valid
for 15 minutes along with a refresh token that is valid for 2 days. You'll get a 401 response with a `Token-Expired` header when the Jwt token is no longer valid. You can ask for a new token from the refreshtoken endpoint.

#### Authorisation

By default all routes in Exelor needs to be authorised. If you don't want a specific route to authorised, say registering a new user, you need to add `[AllowAnonymous]` attribute to that route. Exelor supports permissions based authorisation, the access token that is issued contains the permissions claims which is used for authorisation.

You can authorise an action in 3 different ways
- Attribute based authorisation: You can add `HasPermission` attribute to controllers/actions and provide a list of permission which has access to the controllers/actions

<pre lang="csharp">
[HttpGet]
<b>[HasPermission(Permissions.ReadUsers, Permissions.EditUsers)]</b>
public async Task<List<UserDetailsDto>> Get(
	SieveModel sieveModel)
{
	return await _mediator.Send(new UserList.Query(sieveModel));
}
</pre>

- By checking if the user has a permission by calling `IsAllowed`

<pre lang="csharp">
[HttpPut]
public async Task<UserDetailsDto> Edit(
	[FromBody] UpdateUser.Command command)
{
	if (<b>!_currentUser.IsAllowed(Permissions.EditUsers)</b>)
		throw new HttpException(HttpStatusCode.Forbidden);
	return await _mediator.Send(command);
}
</pre>

- By validating a permission against the user (this throws an exception if the user doesn't have the permission in question)
<pre lang="csharp">
[HttpDelete("{id}")]
public async Task Delete(
	int id)
{
	<b>_currentUser.Authorize(Permissions.EditUsers);</b>
	await _mediator.Send(new DeleteUser.Command(id));
}
</pre>


#### Paging, Sorting and Filtering

- You can use paging, sorting and filtering by using the Sieve model on Get endpoints which supports the following params (you can read more about Sieve [here](https://github.com/Biarity/Sieve))
```curl
GET /GetPosts

?sorts=     LikeCount,CommentCount,-created         // sort by likes, then comments, then descendingly by date created 
&filters=   LikeCount>10, Title@=awesome title,     // filter to posts with more than 10 likes, and a title that contains the phrase "awesome title"
&page=      1                                       // get the first page...
&pageSize=  10                                      // ...which contains 10 posts

```

#### Data Shaping
- You can request the fields that you are interested in and only those fields are returned in the response
```curl
GET /Roles

?fields=     Id, Name         // Only returns the Id and Name values
```
<pre lang="JSON">
[
  {
    "Id": 1,
    "Name": "HR"
  },
  {
    "Id": 2,
    "Name": "Project Manager"
  }
]
</pre>

# Local Building

- ~~Install [.NET Core SDK](https://dotnet.microsoft.com/download)~~
- ~~Go to exelor folder and run `dotnet restore` and `dotnet build`~~
- ~~Add and run migrations~~
  - ~~Install `ef` tool by running `dotnet tool install --global dotnet-ef`~~
  - ~~Run `dotnet ef migrations add Init` and then `dotnet ef database update`~~
- ~~Run `dotnet run` to start the server at `http://localhost:5000/`~~
- ~~You can view the API reference at `http://localhost:5000/swagger`~~
- ~~Login using `{ "userName": "hasan",  "password": "test" }` for ReadUsers permission and `{  "userName": "hasancali",  "password": "test" }` for SuperUser permission~~


# Run using docker

- Add and run migrations
  - Install `ef` tool by running `dotnet tool install --global dotnet-ef`
  - Run `dotnet ef migrations add "Init" --project Infrastructure --startup-project Web --output-dir Persistence\Migrations`
- Go to the root folder of the project and run `docker-compose up`
- You can view the API reference at `http://localhost:5000/swagger`
- Login using `{ "userName": "hasan",  "password": "test" }` for ReadUsers permission and `{  "userName": "hasancali",  "password": "test" }` for SuperUser permission

