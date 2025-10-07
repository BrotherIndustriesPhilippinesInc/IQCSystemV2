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
    public class PortalAccountsController : ControllerBase
    {
        private readonly IQC_APIContext _context;

        public PortalAccountsController(IQC_APIContext context)
        {
            _context = context;
        }

        // GET: api/PortalAccounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortalAccount>>> GetPortalAccount()
        {
            return await _context.PortalAccount.ToListAsync();
        }

        // GET: api/PortalAccounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PortalAccount>> GetPortalAccount(int id)
        {
            var portalAccount = await _context.PortalAccount.FindAsync(id);

            if (portalAccount == null)
            {
                return NotFound();
            }

            return portalAccount;
        }

        [HttpGet("{user_id}")]
        public async Task<ActionResult<PortalAccount>> GetPortalAccount(string user_id)
        {
            var portalAccount = await _context.PortalAccount.FirstOrDefaultAsync(x => x.EmployeeNumber == user_id);

            if (portalAccount == null)
            {
                return NotFound();
            }

            return portalAccount;
        }

        // PUT: api/PortalAccounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPortalAccount(int id, PortalAccount portalAccount)
        {
            if (id != portalAccount.Id)
            {
                return BadRequest();
            }

            _context.Entry(portalAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortalAccountExists(id))
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

        // POST: api/PortalAccounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PortalAccount>> PostPortalAccount(PortalAccount portalAccount)
        {
            _context.PortalAccount.Add(portalAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPortalAccount", new { id = portalAccount.Id }, portalAccount);
        }

        // DELETE: api/PortalAccounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortalAccount(int id)
        {
            var portalAccount = await _context.PortalAccount.FindAsync(id);
            if (portalAccount == null)
            {
                return NotFound();
            }

            _context.PortalAccount.Remove(portalAccount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PortalAccountExists(int id)
        {
            return _context.PortalAccount.Any(e => e.Id == id);
        }
    }
}
