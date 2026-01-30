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
        public async Task<ActionResult<IEnumerable<MachineLotRequest>>> GetMachineLotRequest()
        {
            return await _context.MachineLotRequest.ToListAsync();
        }

        // GET: api/MachineLotRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MachineLotRequest>> GetMachineLotRequest(int id)
        {
            var machineLotRequest = await _context.MachineLotRequest.FindAsync(id);

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
        public async Task<ActionResult<MachineLotRequest>> PostMachineLotRequest(MachineLotRequestDTO machineLotRequest)
        {
            var entity = new MachineLotRequest
            {
                PartCode = machineLotRequest.PartCode,
                PartName = machineLotRequest.PartName,
                /*ReleaseNo = machineLotRequest.ReleaseNo,*/
                VendorName = machineLotRequest.VendorName,
                Quantity = machineLotRequest.Quantity,
                DCIOtherNo = machineLotRequest.DCIOtherNo,
                YellowCard = machineLotRequest.YellowCard,
                ReleaseReasonId = machineLotRequest.ReleaseReasonId,
                WhatForId = machineLotRequest.WhatForId,
                Remarks = machineLotRequest.Remarks,

                CreatedBy = machineLotRequest.CreatedBy,
                CreatedDate = DateTime.Now

            };
            
            _context.MachineLotRequest.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMachineLotRequest", new { id = entity.Id }, entity);
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
