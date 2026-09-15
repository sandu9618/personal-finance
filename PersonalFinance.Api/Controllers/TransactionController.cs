using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Api;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
  private readonly ITransactionService _transactionService;

  public TransactionController(ITransactionService transactionService)
  {
    _transactionService = transactionService;
  }

  [HttpPost]
  public async Task<ActionResult<TransactionResponse>> Createtransaction([FromBody] TransactionRequest request, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _transactionService.CreateTransactionAsync(request, userId, cancellationToken);
      return CreatedAtAction(nameof(GetTransactionById), new {transactionId = response.Id}, response);
    } 
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet]
  public async Task<ActionResult<TransactionListResponse>> GetAllTransactions(CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _transactionService.GetTransactionAsync(userId, cancellationToken);
      return Ok(response);
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet("{transactionId}")]
  public async Task<ActionResult<TransactionResponse>> GetTransactionById(Guid transactionId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _transactionService.GetTransactionByIdAsync(transactionId, userId, cancellationToken);
      return Ok(response);
    } 
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpPut("{transactionId}")]
  public async Task<ActionResult<TransactionResponse>> UpdateTransaction(Guid transactionId, [FromBody] TransactionRequest request, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _transactionService.UpdateTransactionAsync(transactionId, userId, request, cancellationToken);
      return Ok(response);
    } 
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpDelete("{transactionId}")]
  public async Task<ActionResult> DeleteTransaction(Guid transactionId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      await _transactionService.DeleteTransactionAsync(transactionId, userId, cancellationToken);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}