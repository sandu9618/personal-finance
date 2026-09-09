using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
  private readonly IAccountService _accountService;

  public AccountController(IAccountService accountService)
  {
    _accountService = accountService;
  }

  [HttpPost]
  public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] AccountRequest request, CancellationToken cancellationToken)
  {
    var userId = GetUserIdFromClaims();
    var response = await _accountService.CreateAccountAsync(request, userId, cancellationToken);
    return CreatedAtAction(nameof(GetAccountById), new { accountId = response.Id }, response);
  }

  [HttpGet]
  public async Task<ActionResult<AccountListResponse>> GetAccounts(CancellationToken cancellationToken)
  {
    var userId = GetUserIdFromClaims();
    var response = await _accountService.GetAccountsAsync(userId, cancellationToken);
    return Ok(response);
  }

  [HttpGet("{accountId}")]
  public async Task<ActionResult<AccountResponse>> GetAccountById(Guid accountId, CancellationToken cancellationToken)
  {
    var userId = GetUserIdFromClaims();
    var response = await _accountService.GetAccountByIdAsync(accountId, userId, cancellationToken);
    if (response == null)
    {
      return NotFound();
    }
    return Ok(response);
  }

  [HttpPut("{accountId}")]
  public async Task<ActionResult<AccountResponse>> UpdateAccount(Guid accountId, [FromBody] AccountRequest request, CancellationToken cancellationToken)
  {
    var userId = GetUserIdFromClaims();
    var response = await _accountService.UpdateAccountAsync(accountId, request, cancellationToken);
    return Ok(response);
  }

  [HttpDelete("{accountId}")]
  public async Task<IActionResult> DeleteAccount(Guid accountId, CancellationToken cancellationToken)
  {
    var userId = GetUserIdFromClaims();
    await _accountService.DeleteAccountAsync(accountId, userId, cancellationToken);
    return NoContent();
  }

  private Guid GetUserIdFromClaims()
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
    ?? User.FindFirst("sub");;
    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
    {
      throw new InvalidOperationException("User ID claim is missing or invalid.");
    }
    return userId;
  }
  
}