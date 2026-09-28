using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransferController: ControllerBase
{
  private readonly ITransferService _transferService;

  public TransferController(ITransferService transferService)
  {
    _transferService = transferService;
  }

  [HttpPost]
  public async Task<ActionResult<TransferResponse>> CreateTransfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
  {
    try
    {
      var userId = ControllerHelper.GetUserIdFromClaims(User);
      var response = await _transferService.CreateTransferAsync(request, userId, cancellationToken);
      return CreatedAtAction(nameof(GetTransferById), new {transferId = response.Id}, response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new {message = ex.Message});
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new {message = ex.Message});
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet]
  public async Task<ActionResult<TransferListResponse>> GetTransfers(CancellationToken cancellationToken)
  {
    try
    {
      var userId = ControllerHelper.GetUserIdFromClaims(User);
      var response = await _transferService.GetAllTransfersAsync(userId, cancellationToken);
      return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new {message = ex.Message});
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet("{transferId}")]
  public async Task<ActionResult<TransferResponse>> GetTransferById(Guid transferId, CancellationToken cancellationToken)
  {
    try
    {
      var userId = ControllerHelper.GetUserIdFromClaims(User);
      var response = await _transferService.GetTransferAsync(transferId, userId, cancellationToken);
      return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new {message = ex.Message});
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new {message = ex.Message});
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpPut("{transferId}")]
  public async Task<ActionResult<TransferResponse>> UpdateTransfer(Guid transferId, [FromBody] TransferRequest request, CancellationToken cancellationToken)
  {
    try
    {
      var userId = ControllerHelper.GetUserIdFromClaims(User);
      var response = await _transferService.UpdateTransferAsync(transferId, request, userId, cancellationToken);
      return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new {message = ex.Message});
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new {message = ex.Message});
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpDelete("{transferId}")]
  public async Task<ActionResult> DeleteTransfer(Guid transferId, CancellationToken cancellationToken)
  {
    try
    {
      var userId = ControllerHelper.GetUserIdFromClaims(User);
      await _transferService.DeleteTransferAsync(transferId, userId, cancellationToken);
      return NoContent();
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new {message = ex.Message});
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new {message = ex.Message});
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }
}
