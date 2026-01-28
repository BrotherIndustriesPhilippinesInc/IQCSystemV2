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
    public class WhatForsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public WhatForsController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/WhatFors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WhatFor>>> GetWhatFor()
        {
            return await _context.WhatFor.ToListAsync();
        }

        // GET: api/WhatFors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WhatFor>> GetWhatFor(int id)
        {
            var whatFor = await _context.WhatFor.FindAsync(id);

            if (whatFor == null)
            {
                return NotFound();
            }

            return whatFor;
        }

        // PUT: api/WhatFors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWhatFor(int id, WhatFor whatFor)
        {
            if (id != whatFor.Id)
            {
                return BadRequest();
            }

            _context.Entry(whatFor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WhatForExists(id))
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

        // POST: api/WhatFors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<WhatForDTO>> PostWhatFor(WhatForDTO whatFor)
        {
            var entity = new WhatFor
            {
                WhatForCode = whatFor.WhatForCode,
                WhatForName = whatFor.WhatForName,
                WhatForDetails = whatFor.WhatForDetails,
                CreatedBy = whatFor.CreatedBy,
                CreatedDate = DateTime.Now
                
            };

            _context.WhatFor.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWhatFor", new { id = entity.Id }, whatFor);
        }

        // DELETE: api/WhatFors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWhatFor(int id)
        {
            var whatFor = await _context.WhatFor.FindAsync(id);
            if (whatFor == null)
            {
                return NotFound();
            }

            _context.WhatFor.Remove(whatFor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WhatForExists(int id)
        {
            return _context.WhatFor.Any(e => e.Id == id);
        }
    }
}
