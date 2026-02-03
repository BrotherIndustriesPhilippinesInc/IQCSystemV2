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
    public class MachineLotRequestsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public MachineLotRequestsController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/MachineLotRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MachineLotRequestGetDTO>>> GetMachineLotRequest()
        {
            var data = await _context.MachineLotRequest
                .Select(x => new MachineLotRequestGetDTO
                {
                    Id = x.Id,
                    PartCode = x.PartCode,
                    PartName = x.PartName,
                    VendorName = x.VendorName,
                    Quantity = x.Quantity,
                    ReleaseNo = x.ReleaseNo,
                    YellowCard = x.YellowCard,
                    DCIOtherNo = x.DCIOtherNo,
                    Remarks = x.Remarks,

                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,

                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate,

                    // Accessing navigation properties safely
                    WhatForName = x.WhatFor.WhatForName,
                    ReleaseReasonName = x.ReleaseReason.ReleaseReasonName
                })
                .AsQueryable()
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/MachineLotRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MachineLotRequestGetDTO>> GetMachineLotRequest(int id)
        {
            var machineLotRequest = await _context.MachineLotRequest
                .Select(x => new MachineLotRequestGetDTO
                {
                    Id = x.Id,
                    PartCode = x.PartCode,
                    PartName = x.PartName,
                    VendorName = x.VendorName,
                    Quantity = x.Quantity,
                    ReleaseNo = x.ReleaseNo,
                    YellowCard = x.YellowCard,
                    DCIOtherNo = x.DCIOtherNo,
                    Remarks = x.Remarks,

                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,

                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate,

                    // Accessing navigation properties safely
                    WhatForName = x.WhatFor.WhatForName,
                    ReleaseReasonName = x.ReleaseReason.ReleaseReasonName
                })
                .Where(x => x.Id == id)
                .AsQueryable()
                .FirstOrDefaultAsync();
                

            if (machineLotRequest == null)
            {
                return NotFound();
            }

            return machineLotRequest;
        }

        // PUT: api/MachineLotRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMachineLotRequest(int id, MachineLotRequest machineLotRequest)
        {
            if (id != machineLotRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(machineLotRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MachineLotRequestExists(id))
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

        // POST: api/MachineLotRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MachineLotRequestGetDTO>> PostMachineLotRequest(MachineLotRequestDTO machineLotRequest)
        {
            // 1. Map Input DTO -> Entity (For Saving)
            var entity = new MachineLotRequest
            {
                PartCode = machineLotRequest.PartCode,
                PartName = machineLotRequest.PartName,
                VendorName = machineLotRequest.VendorName,
                Quantity = machineLotRequest.Quantity,
                //ReleaseNo = machineLotRequest.ReleaseNo, // It's nullable, so this is fine
                YellowCard = machineLotRequest.YellowCard,
                DCIOtherNo = machineLotRequest.DCIOtherNo,
                Remarks = machineLotRequest.Remarks,

                // Foreign Keys
                WhatForId = machineLotRequest.WhatForId, // We only know the ID here
                ReleaseReasonId = machineLotRequest.ReleaseReasonId,

                // Audit Fields
                CreatedBy = machineLotRequest.CreatedBy,
                CreatedDate = DateTime.Now //
            };

            _context.MachineLotRequest.Add(entity);
            await _context.SaveChangesAsync(); // entity.Id is generated here

            // 2. Re-fetch with Includes to get the Names (The "Hydration" step)
            var result = await _context.MachineLotRequest
                .Include(x => x.WhatFor)
                .Include(x => x.ReleaseReason)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            // 3. Map Entity -> Output DTO (For the Client)
            var response = new MachineLotRequestGetDTO
            {
                Id = result.Id,
                PartCode = result.PartCode,
                PartName = result.PartName,
                VendorName = result.VendorName,
                Quantity = result.Quantity,
                ReleaseNo = result.ReleaseNo,
                YellowCard = result.YellowCard,
                DCIOtherNo = result.DCIOtherNo,
                Remarks = result.Remarks,

                // The Magic: Flattening the navigation properties
                WhatForName = result.WhatFor.WhatForName,
                WhatForCode = result.WhatFor.WhatForCode,

                ReleaseReasonName = result.ReleaseReason.ReleaseReasonName,
                ReleaseReasonCode = result.ReleaseReason.ReleaseReasonCode,

                // If your DTO has these fields, map them too. If not, ignore.
                CreatedBy = result.CreatedBy,
                CreatedDate = result.CreatedDate
            };

            // Return 201 Created with the location header and the fully populated DTO
            return CreatedAtAction("GetMachineLotRequest", new { id = entity.Id }, response);
        }

        // DELETE: api/MachineLotRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachineLotRequest(int id)
        {
            var machineLotRequest = await _context.MachineLotRequest.FindAsync(id);
            if (machineLotRequest == null)
            {
                return NotFound();
            }

            _context.MachineLotRequest.Remove(machineLotRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MachineLotRequestExists(int id)
        {
            return _context.MachineLotRequest.Any(e => e.Id == id);
        }
    }
}
