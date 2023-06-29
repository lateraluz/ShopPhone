﻿using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using ShopPhone.Shared.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace ShopPhone.Client.Auth;


public class AuthenticationService : AuthenticationStateProvider
{
    private readonly ISessionStorageService _SessionStorageService;
    private readonly HttpClient _HttpClient;
    private readonly ClaimsPrincipal _Anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public AuthenticationService(ISessionStorageService sessionStorageService, HttpClient httpClient)
    {
        _SessionStorageService = sessionStorageService;
        _HttpClient = httpClient;
    }

    public static JwtSecurityToken ParseToken(LoginResponseDTO sesionUsuario)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(sesionUsuario.Token);
        return token;
    }

    public async Task Authenticate(LoginResponseDTO? response)
    {
        ClaimsPrincipal claimsPrincipal;

        if (response is not null)
        {
            // Recuperamos los claims desde el token recibido.
            var token = ParseToken(response);

            // Establecemos al objeto HttpClient el token en el header
            _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.Token);

            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(token.Claims.ToList(), "JWT"));

            await _SessionStorageService.SaveStorage("sesion", response);
        }
        else
        {
            claimsPrincipal = _Anonymous;
            await _SessionStorageService.RemoveItemAsync("sesion");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var sesionUsuario = await _SessionStorageService.GetStorage<LoginResponseDTO>("sesion");

        if (sesionUsuario is null)
            return await Task.FromResult(new AuthenticationState(_Anonymous));

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(ParseToken(sesionUsuario).Claims, "JWT"));

        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }
}