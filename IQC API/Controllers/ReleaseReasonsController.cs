using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQC_API.Data;
using IQC_API.Models;
using IQC_API.DTO.MachineLotRequest;

namespace IQC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseReasonsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public ReleaseReasonsController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/ReleaseReasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReleaseReason>>> GetReleaseReason()
        {
            return await _context.ReleaseReason.ToListAsync();
        }

        // GET: api/ReleaseReasons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReleaseReason>> GetReleaseReason(int id)
        {
            var releaseReason = await _context.ReleaseReason.FindAsync(id);

            if (releaseReason == null)
            {
                return NotFound();
            }

            return releaseReason;
        }

        // PUT: api/ReleaseReasons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReleaseReason(int id, ReleaseReason releaseReason)
        {
            if (id != releaseReason.Id)
            {
                return BadRequest();
            }

            _context.Entry(releaseReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReleaseReasonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ReleaseReasons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReleaseReason>> PostReleaseReason(ReleaseReasonDTO releaseReason)
        {
            var entity = new ReleaseReason
            {
                CreatedDate = DateTime.Now,
                CreatedBy = releaseReason.CreatedBy,
                ReleaseReasonCode = releaseReason.ReleaseReasonCode,
                ReleaseReasonName = releaseReason.ReleaseReasonName,
            };

            _context.ReleaseReason.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReleaseReason", new { id = entity.Id }, releaseReason);
        }

        // DELETE: api/ReleaseReasons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReleaseReason(int id)
        {
            var releaseReason = await _context.ReleaseReason.FindAsync(id);
            if (releaseReason == null)
            {
                return NotFound();
            }

            _context.ReleaseReason.Remove(releaseReason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReleaseReasonExists(int id)
        {
            return _context.ReleaseReason.Any(e => e.Id == id);
        }
    }
}
