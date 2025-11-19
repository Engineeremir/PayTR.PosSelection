using MediatR;
using Microsoft.AspNetCore.Mvc;
using PayTR.PosSelection.Application.Handlers.PosRatios.Queries.GetBestPos;

namespace PayTR.PosSelection.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosRatiosController(IMediator mediator) : ControllerBase
    {
        [HttpPost("select")]
        public async Task<ActionResult<GetBestPosQueryDto>> SelectPos([FromBody] GetBestPosQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }
    }
}
