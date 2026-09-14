using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Api;

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
    try
    {
      var userId = User.GetUserIdFromClaims();
      var response = await _accountService.CreateAccountAsync(request, userId, cancellationToken);
      return CreatedAtAction(nameof(GetAccountById), new { accountId = response.Id }, response);
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  [HttpGet]
  public async Task<ActionResult<AccountListResponse>> GetAccounts(CancellationToken cancellationToken)
  {
    try
    {
      var userId = User.GetUserIdFromClaims();
      var response = await _accountService.GetAccountsAsync(userId, cancellationToken);
      return Ok(response);
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  [HttpGet("{accountId}")]
  public async Task<ActionResult<AccountResponse>> GetAccountById(Guid accountId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _accountService.GetAccountByIdAsync(accountId, userId, cancellationToken);
      return Ok(response);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpPut("{accountId}")]
  public async Task<ActionResult<AccountResponse>> UpdateAccount(Guid accountId, [FromBody] AccountRequest request, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _accountService.UpdateAccountAsync(accountId, userId, request, cancellationToken);
      return Ok(response);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpDelete("{accountId}")]
  public async Task<IActionResult> DeleteAccount(Guid accountId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      await _accountService.DeleteAccountAsync(accountId, userId, cancellationToken);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}
