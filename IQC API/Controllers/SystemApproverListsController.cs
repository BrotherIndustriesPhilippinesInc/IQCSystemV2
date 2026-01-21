using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQC_API.Data;
using IQC_API.Models;
using IQC_API.DTO;

namespace IQC_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SystemApproverListsController : ControllerBase
    {
        private readonly IQC_APIContext _context;
        private readonly IQC_API_PG_Context _pgContext;

        public SystemApproverListsController(IQC_APIContext context, IQC_API_PG_Context pgContext)
        {
            _context = context;
            _pgContext = pgContext;
        }

        // GET: api/SystemApproverLists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSystemApproverList()
        {
            // 1️⃣ Get SystemApproverList from SQL Server
            var approvers = await _context.SystemApproverList
                .Where(x => x.SystemID == 73)
                .ToListAsync();

            // 2️⃣ Get all Accounts from Postgres where AccountId matches
            var accountIds = approvers.Select(a => a.Id).ToList();
            var accounts = await _pgContext.Accounts
                .Where(a => accountIds.Contains(a.AccountId))
                .ToListAsync();

            // 3️⃣ Join in memory
            var result = from approver in approvers
                         join account in accounts
                         on approver.Id equals account.AccountId into acc
                         from account in acc.DefaultIfEmpty() // left join
                         select new
                         {
                             approver.Id,
                             approver.FullName,
                             approver.EmailAddress,
                             approver.Section,
                             approver.Position,
                             approver.EmployeeNumber,
                             approver.ADID,
                             // Accounts info
                             MesName = account != null ? account.MesName : null,
                             IsAdmin = account != null ? account.IsAdmin : false,
                         };

            return Ok(result);
        }

        [HttpGet("MesName/{employeeNumber}")]
        public async Task<ActionResult<object>> GetSystemApproverList(string employeeNumber)
        {
            // Get approver
            var approver = await _context.SystemApproverList
                .FirstOrDefaultAsync(x => x.SystemID == 73 && x.EmployeeNumber == employeeNumber);

            if (approver == null)
                return NotFound($"No approver found for employee {employeeNumber}");

            // Get corresponding IQC user
            var iqcUser = await _pgContext.Accounts
                .FirstOrDefaultAsync(x => x.AccountId == approver.Id);

            if (iqcUser == null)
                return NotFound($"No IQC user found for approver {approver.Id}");

            return Ok(iqcUser);
        }

        // GET: api/SystemApproverLists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetSystemApproverList(int id)
        {
            // 1️⃣ Get the SystemApproverList from SQL Server
            var approver = await _context.SystemApproverList.FindAsync(id);

            if (approver == null)
                return NotFound();

            // 2️⃣ Get the related Accounts from Postgres
            var account = await _pgContext.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == approver.Id);

            // 3️⃣ Combine data into a DTO-like object
            var result = new
            {
                approver.Id,
                approver.FullName,
                approver.EmailAddress,
                approver.Section,
                approver.Position,
                approver.EmployeeNumber,
                approver.ADID,
                // Postgres Accounts info
                MesName = account != null ? account.MesName : null,
                IsAdmin = account != null ? account.IsAdmin : false
            };

            return Ok(result);
        }

        // PUT: api/SystemApproverLists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystemApproverList(int id, SystemApproverList systemApproverList)
        {
            if (id != systemApproverList.Id)
            {
                return BadRequest();
            }

            _context.Entry(systemApproverList).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemApproverListExists(id))
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

        // POST: api/SystemApproverLists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SystemApproverList>> PostSystemApproverList(SystemApproverListDTOPost dto)
        {
            // 1️⃣ Get employee details from SQL Server
            var employeeDetails = await _context.PortalAccount
                .FirstOrDefaultAsync(x => x.EmployeeNumber == dto.EmployeeNumber);

            if (employeeDetails == null)
                return BadRequest("Employee not found.");

            // 2️⃣ Check if already exists in SQL Server
            bool exists = await _context.SystemApproverList
                .AnyAsync(x => x.SystemID == 73 && x.EmployeeNumber == dto.EmployeeNumber);

            if (exists)
                return Conflict("Approver already exists for this system.");

            // 3️⃣ Create SystemApproverList record (SQL Server)
            var systemApproverList = new SystemApproverList
            {
                SystemID = 73,
                SystemName = "IQC System V2",
                ApproverNumber = 0,
                FullName = employeeDetails.EmployeeName,
                EmailAddress = employeeDetails.EmailAddress,
                Section = employeeDetails.Section,
                Position = employeeDetails.Position,
                EmployeeNumber = dto.EmployeeNumber,
                ADID = employeeDetails.ADID
            };

            _context.SystemApproverList.Add(systemApproverList);
            await _context.SaveChangesAsync();

            int newId = systemApproverList.Id;

            // 4️⃣ Create Accounts record (Postgres) with proper schema mapping
            var iqcAccounts = new Accounts
            {
                AccountId = newId,
                MesName = dto.MesName,
                IsAdmin = dto.IsAdmin
            };

            try
            {
                _pgContext.Accounts.Add(iqcAccounts);
                await _pgContext.SaveChangesAsync();

                // 5️⃣ Verify insertion
                var inserted = await _pgContext.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == newId);

                if (inserted == null)
                {
                    return StatusCode(500, "Failed to insert Accounts into Postgres. Check schema/connection.");
                }

                Console.WriteLine($"✅ Postgres insert successful: AccountId={inserted.AccountId}, MesName={inserted.MesName}");
            }
            catch (Exception ex)
            {
                // 6️⃣ Log any exceptions
                Console.WriteLine($"❌ Postgres insert error: {ex}");
                return StatusCode(500, $"Error saving to Postgres: {ex.Message}");
            }

            return CreatedAtAction("GetSystemApproverList", new { id = systemApproverList.Id }, systemApproverList);
        }

        // DELETE: api/SystemApproverLists/5
        public class DeleteApproverRequest
        {
            public string EmployeeNumber { get; set; }
        }

        [HttpPost("delete-approver")]
        public async Task<IActionResult> DeleteSystemApproverList([FromBody] DeleteApproverRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeNumber))
            {
                return BadRequest(new { message = "EmployeeNumber is required" });
            }

            var systemApproverList = await _context.SystemApproverList
                .FirstOrDefaultAsync(x => x.EmployeeNumber == request.EmployeeNumber && x.SystemID == 73);

            if (systemApproverList == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _context.SystemApproverList.Remove(systemApproverList);
            await _context.SaveChangesAsync();

            _pgContext.Accounts.RemoveRange(
                _pgContext.Accounts.Where(a => a.AccountId == systemApproverList.Id).ToList()
            );
            await _pgContext.SaveChangesAsync();

            return Ok(new { status = 200, message = "User deleted successfully" });
        }

        private bool SystemApproverListExists(int id)
        {
            return _context.SystemApproverList.Any(e => e.Id == id);
        }

        [HttpGet("ViaEmployeeNumber/{employeeNumber}")]
        public async Task<ActionResult<object>> GetSystemApproverListViaEmployeeNumber([FromRoute] string employeeNumber)
        {
            // 1️⃣ Get the SystemApproverList from SQL Server
            var approver = await _context.SystemApproverList
                .FirstOrDefaultAsync(x => x.SystemID == 73 && x.EmployeeNumber == employeeNumber);

            if (approver == null)
                return NotFound();

            // 2️⃣ Get the related Accounts from Postgres
            var account = await _pgContext.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == approver.Id);

            // 3️⃣ Combine data into a DTO-like object
            var result = new
            {
                approver.Id,
                approver.FullName,
                approver.EmailAddress,
                approver.Section,
                approver.Position,
                approver.EmployeeNumber,
                approver.ADID,
                // Postgres Accounts info
                MesName = account != null ? account.MesName : null,
                IsAdmin = account != null ? account.IsAdmin : false,
                IsSuperAdmin = account != null ? account.IsSuperAdmin : false
            };

            return Ok(result);
        }

        [HttpPost("UpdateEmployee/{id}")]
        public async Task<ActionResult<object>> UpdateEmployee(int id, [FromBody] SystemApproverListDTOPost dto)
        {
            // Look for the Postgres account
            var account = await _pgContext.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
                return NotFound("Account not found in Postgres.");

            // Update only Postgres fields
            account.MesName = dto.MesName;
            account.IsAdmin = dto.IsAdmin;

            await _pgContext.SaveChangesAsync();

            return Ok(new { id = id });
        }
    }
}
