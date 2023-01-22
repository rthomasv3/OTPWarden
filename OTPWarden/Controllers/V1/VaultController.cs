using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTPWarden.Controllers.V1.Contract.Requests;
using OTPWarden.Controllers.V1.Contract.Responses;
using OTPWarden.Models;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Controllers.V1;

[Route("api/v1/[controller]")]
[ApiController]
public sealed class VaultController : ControllerBase
{
    #region Fields

    private readonly IVaultService _vaultService;

    #endregion

    #region Constructor

    public VaultController(IVaultService vaultService)
    {
        _vaultService = vaultService ?? throw new ArgumentNullException(nameof(vaultService));
    }

    #endregion

    #region Public Methods

    [HttpGet]
    public async Task<ActionResult<GetUserVaultApiResponse>> GetUserVault()
    {
        ActionResult<GetUserVaultApiResponse> response = BadRequest();
        Int64.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out long userId);

        if (userId > 0)
        {
            IEnumerable<VaultEntry> userVaultEntries = await _vaultService.GetVaultEntries(userId);

            response = Ok(new GetUserVaultApiResponse()
            {
                UserId = userId,
                VaultEntries = userVaultEntries.Select(x => MapVaultEntry(x))
            });
        }

        return response;
    }

    [HttpPost("create-entry")]
    public async Task<ActionResult<CreateVaultEntryApiResponse>> CreateVaultEntry([FromBody] CreateVaultEntryApiRequest request)
    {
        ActionResult<CreateVaultEntryApiResponse> response = BadRequest();
        Int64.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out long userId);

        if (userId > 0)
        {
            VaultEntry vaultEntry = await _vaultService.CreateVaultEntry(userId, request.Name, request.SecretKey, request.Urls,
                request.Algorithm, request.Counter, request.Digits, request.Notes);

            if (vaultEntry != null)
            {
                response = Ok(new CreateVaultEntryApiResponse()
                {
                    VaultEntry = MapVaultEntry(vaultEntry)
                });
            }
        }

        return response;
    }

    #endregion

    #region Private Methods

    private VaultEntryContent MapVaultEntry(VaultEntry vaultEntry)
    {
        return new VaultEntryContent()
        {
            Algorithm = vaultEntry.Algorithm,
            Counter = vaultEntry.Counter,
            Created = vaultEntry.Created,
            Digits = vaultEntry.Digits,
            Id = vaultEntry.Id,
            Issuer = vaultEntry.Issuer,
            Name = vaultEntry.Name,
            Notes = vaultEntry.Notes,
            Period = vaultEntry.Period,
            SecretKey = vaultEntry.SecretKey,
            Urls = vaultEntry.Urls
        };
    }

    #endregion
}
