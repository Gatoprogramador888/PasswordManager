using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.API.Extensions;
using PasswordManager.API.Models;
using PasswordManager.Application.Interfaces;
using PasswordManager.Application.Vault;

namespace PasswordManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // todos los endpoints del vault requieren JWT válido
public sealed class VaultController(
    GetVaultQuery getVaultQuery,
    GetVaultCountQuery getVaultCountQuery,
    AddEntryCommand addEntryCommand,
    UpdateEntryCommand updateEntryCommand,
    DeleteEntryCommand deleteEntryCommand,
    IIdempotencyStore idempotencyStore) : ControllerBase
{
    // GET api/vault
    [System.Obsolete("Ahora se usara un endpoint por paginacion")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var entries = await getVaultQuery.HandleAsync(userId, CancellationToken.None);

        return Ok(entries);
    }

    // GET api/vault/pagination?min=0&max=10
    [HttpGet("pagination")]
    public async Task<IActionResult> RetrieveViaPagination([FromQuery]int min,[FromQuery]int max,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var entries = await getVaultQuery.HandleAsync(userId, min, max, CancellationToken.None);

        return Ok(entries);
    }

    // GET api/vault/Count
    [HttpGet("Count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var count = await getVaultCountQuery.HandleAsync(userId, CancellationToken.None);
        return Ok(count);
    }

    // POST api/vault
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] AddVaultEntryRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (idempotencyKey is not null && await idempotencyStore.WasProcessedAsync(idempotencyKey, ct))
            return Ok(new { message = "Ya procesado." });

        var response = await addEntryCommand.HandleAsync(
            userId,
            new(request.Name, request.EncryptedBlob, request.Iv),
            ct);

        return CreatedAtAction(
            nameof(RetrieveViaPagination),
            new { min = 0, max = 10 },
            new { id = response.Id, totalCount = response.TotalCount });
    }

    // PUT api/vault/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVaultEntryRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (idempotencyKey is not null && await idempotencyStore.WasProcessedAsync(idempotencyKey, ct))
            return Ok(new { message = "Ya procesado." });

        var updated = await updateEntryCommand.HandleAsync(
            id,
            userId,
            new(request.Name, request.EncryptedBlob, request.Iv),
            ct);

        return updated ? NoContent() : NotFound(new { code = "NOT_FOUND", message = "Entrada no encontrada." });
    }

    // DELETE api/vault/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var deleted = await deleteEntryCommand.HandleAsync(id, userId, ct);

        return deleted.IsDeleted ? Ok(new { totalCount = deleted.TotalCount }) : NotFound(new { code = "NOT_FOUND", message = "Entrada no encontrada." });
    }
}