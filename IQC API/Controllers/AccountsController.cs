using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQC_API.Data;
using IQC_API.Models;
using IQC_API.DTO.User;
using IQC_API.Services;
using Azure.Core;

namespace IQC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;
        private readonly IUserService _userService;

        public AccountsController(IQC_API_PG_Context context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Accounts>>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        // GET: api/Accounts/5
        [HttpGet("{accountId}")]
        public async Task<ActionResult<Accounts>> GetAccounts(int accountId)
        {
            //find account from other column
            var accounts = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (accounts == null)
            {
                return NotFound();
            }

            return accounts;
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccounts(int id, Accounts accounts)
        {
            if (id != accounts.Id)
            {
                return BadRequest();
            }

            _context.Entry(accounts).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountsExists(id))
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

        // POST: api/Accounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Accounts>> PostAccounts(Accounts accounts)
        {
            _context.Accounts.Add(accounts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccounts", new { id = accounts.Id }, accounts);
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccounts(int id)
        {
            var accounts = await _context.Accounts.FindAsync(id);
            if (accounts == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(accounts);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountsExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }


        public class ColorRequest
        {
            public int id { get; set; }
            public string bgColor { get; set; }
            public string textColor { get; set; }
        }
        [HttpPost("UpdateColor")]
        public async Task<ActionResult> UpdateColor([FromBody] ColorRequest colorRequest)
        {

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == colorRequest.id);

            if (account == null)
                return NotFound("Account not found in Postgres.");

            account.Theme = colorRequest.bgColor;
            account.FontColor = colorRequest.textColor;

            await _context.SaveChangesAsync();

            return Ok(new { id = colorRequest.id });
        }

        public class UserRequest
        {
            public string id_number { get; set; }
        }
        [HttpPost("getUser")]
        public async Task<ActionResult<UserDto>> GetUser([FromBody] UserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.id_number))
            {
                return BadRequest("ID Number is required.");
            }

            var user = await _userService.GetUserByEmpNoAsync(request.id_number);

            if (user == null)
            {
                return NotFound(new { message = "No data found for the given condition." });
            }

            return Ok(user);
        }
    }
}
