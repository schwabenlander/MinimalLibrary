using Microsoft.AspNetCore.Authentication;

namespace MinimalLibrary.Api.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "VerySecret";
}