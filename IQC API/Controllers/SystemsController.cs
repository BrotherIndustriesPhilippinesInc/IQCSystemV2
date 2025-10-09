using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQC_API.Data;
using IQC_API.Models;

namespace IQC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public SystemsController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/Systems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemModel>>> GetSystem()
        {
            return await _context.System.ToListAsync();
        }

        // GET: api/Systems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemModel>> GetSystem(int id)
        {
            var system = await _context.System.FindAsync(id);

            if (system == null)
            {
                return NotFound();
            }

            return system;
        }

        // PUT: api/Systems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystem(int id, SystemModel system)
        {
            if (id != system.Id)
            {
                return BadRequest();
            }

            _context.Entry(system).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemExists(id))
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

        // POST: api/Systems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SystemModel>> PostSystem(SystemModel system)
        {
            _context.System.Add(system);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSystem", new { id = system.Id }, system);
        }

        // DELETE: api/Systems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystem(int id)
        {
            var system = await _context.System.FindAsync(id);
            if (system == null)
            {
                return NotFound();
            }

            _context.System.Remove(system);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SystemExists(int id)
        {
            return _context.System.Any(e => e.Id == id);
        }
    }
}
