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
    public class DashboardController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public DashboardController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspectionDetails>>> GetInspectionDetails()
        {
            return await _context.InspectionDetails.ToListAsync();
        }

        // GET: api/Dashboard/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InspectionDetails>> GetInspectionDetails(int id)
        {
            var inspectionDetails = await _context.InspectionDetails.FindAsync(id);

            if (inspectionDetails == null)
            {
                return NotFound();
            }

            return inspectionDetails;
        }

        // PUT: api/Dashboard/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInspectionDetails(int id, InspectionDetails inspectionDetails)
        {
            if (id != inspectionDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(inspectionDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InspectionDetailsExists(id))
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

        // POST: api/Dashboard
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        private async Task<ActionResult<InspectionDetails>> PostInspectionDetails(InspectionDetails inspectionDetails)
        {
            _context.InspectionDetails.Add(inspectionDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInspectionDetails", new { id = inspectionDetails.Id }, inspectionDetails);
        }

        // DELETE: api/Dashboard/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspectionDetails(int id)
        {
            var inspectionDetails = await _context.InspectionDetails.FindAsync(id);
            if (inspectionDetails == null)
            {
                return NotFound();
            }

            _context.InspectionDetails.Remove(inspectionDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InspectionDetailsExists(int id)
        {
            return _context.InspectionDetails.Any(e => e.Id == id);
        }

        private IQueryable<InspectionDetails> InspectionBaseQuery =>
        _context.InspectionDetails
        .Select(x => new InspectionDetails
        {
            Id = x.Id,
            IQCCheckDate = x.IQCCheckDate,
            IsApproved = x.IsApproved,
            CheckUser = x.CheckUser
        });


        [HttpGet("inspection-summary")]
        public IActionResult InspectionSummary(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
                var query = _context.InspectionDetails.AsQueryable();

                // 1. Timezone-aware date filtering
                if (startDate.HasValue)
                {
                    var startLocal = new DateTimeOffset(startDate.Value.Date, phTimeZone.BaseUtcOffset);
                    query = query.Where(x => x.CreatedDate >= startLocal.UtcDateTime);
                }

                if (endDate.HasValue)
                {
                    var endLocal = new DateTimeOffset(endDate.Value.Date.AddDays(1).AddTicks(-1), phTimeZone.BaseUtcOffset);
                    query = query.Where(x => x.CreatedDate <= endLocal.UtcDateTime);
                }

                // 2. Fetch ONLY the needed columns into memory
                // WE ADD IsApproved HERE so the data is available for counting
                var filteredData = query
                    .Select(x => new { x.Id, x.CreatedDate, x.IsApproved })
                    .ToList();

                // 3. Group and aggregate in memory
                var dailyTrends = filteredData
                    .GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.CreatedDate, phTimeZone).Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalInspections = g.Count(),
                        // This counts only the rows in this specific day where IsApproved == true
                        TotalApproved = g.Count(x => x.IsApproved)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // 4. Return the final structured JSON
                return Ok(new
                {
                    TotalInspections = dailyTrends.Sum(x => x.TotalInspections),
                    TotalApproved = dailyTrends.Sum(x => x.TotalApproved), // Aggregate the grand total here
                    StartDate = startDate,
                    EndDate = endDate,
                    DailyTrends = dailyTrends
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching inspection summary", Error = ex.Message });
            }
        }

        [HttpGet("approval-summary")]
        public IActionResult ApprovalSummary(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Only light columns loaded
                var raw = InspectionBaseQuery
                    .Where(x => x.IQCCheckDate != null && x.IQCCheckDate != "")
                    .ToList();

                var parsed = raw
                    .Select(x =>
                    {
                        if (!DateTime.TryParse(x.IQCCheckDate, out var dt))
                            return null;

                        return new
                        {
                            x.IsApproved,
                            Date = dt.Date
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                if (startDate.HasValue)
                    parsed = parsed.Where(x => x.Date >= startDate.Value.Date).ToList();

                if (endDate.HasValue)
                    parsed = parsed.Where(x => x.Date <= endDate.Value.Date).ToList();

                return Ok(new
                {
                    TotalInspections = parsed.Count,
                    ApprovedCount = parsed.Count(x => x.IsApproved),
                    StartDate = startDate,
                    EndDate = endDate,
                    DailyTrends = parsed
                        .GroupBy(x => x.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            TotalInspections = g.Count(),
                            ApprovedCount = g.Count(z => z.IsApproved)
                        })
                        .OrderBy(x => x.Date)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching approval summary", Error = ex.Message });
            }
        }

        [HttpGet("members")]
        public IActionResult GetInspectionMembers(DateTime? startDate, DateTime? endDate)
        {
            var raw = InspectionBaseQuery
                .Where(x => x.IQCCheckDate != null && x.IQCCheckDate != "")
                .Where(x => x.CheckUser != null && x.CheckUser != "")
                .ToList();

            var parsed = raw
                .Select(x =>
                {
                    if (!DateTime.TryParse(x.IQCCheckDate, out var dt))
                        return null;

                    return new
                    {
                        x.CheckUser,
                        Date = dt.Date
                    };
                })
                .Where(x => x != null)
                .ToList();

            if (startDate.HasValue)
                parsed = parsed.Where(x => x.Date >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                parsed = parsed.Where(x => x.Date <= endDate.Value.Date).ToList();

            var grouped = parsed
                .GroupBy(x => x.CheckUser)
                .Select(g => new
                {
                    User = g.Key,
                    NumberOfInspection = g.Count()
                })
                .OrderBy(x => x.User)
                .ToList();

            return Ok(grouped);
        }

        [HttpGet("summary")]
        public IActionResult GetOverallSummary(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
                var query = _context.InspectionDetails.AsQueryable();

                // 1. Timezone-aware filtering on CreatedDate (so we don't miss rows!)
                if (startDate.HasValue)
                {
                    var startUtc = new DateTimeOffset(startDate.Value.Date, phTimeZone.BaseUtcOffset).UtcDateTime;
                    query = query.Where(ind => ind.CreatedDate >= startUtc);
                }

                if (endDate.HasValue)
                {
                    var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1).AddTicks(-1), phTimeZone.BaseUtcOffset).UtcDateTime;
                    query = query.Where(ind => ind.CreatedDate <= endUtc);
                }

                // 2. Project into an anonymous type FIRST to safely grab the QMLotCategory
                var baseData = query.Select(ind => new
                {
                    IQCCheckDate = ind.IQCCheckDate,
                    CheckUser = ind.CheckUser,
                    QMLotCategory = _context.PartsInformation
                        .Where(pi => pi.PartCode == ind.PartCode)
                        .Select(pi => pi.QMLotCategory)
                        .FirstOrDefault() // Prevents your duplicate row problem!
                });

                // 3. Group by the three columns to get the count
                var tableData = baseData
                    .GroupBy(x => new { x.IQCCheckDate, x.CheckUser, x.QMLotCategory })
                    .Select(g => new
                    {
                        IQCCheckDate = g.Key.IQCCheckDate,
                        CheckUser = g.Key.CheckUser,
                        QMLotCategory = g.Key.QMLotCategory,
                        TotalInspections = g.Count() // This is your "(Totalnumber of inspection for the day)"
                    })
                    .OrderByDescending(x => x.IQCCheckDate)
                    .ToList();

                // DataTables expects the array inside a "data" property
                return Ok(new { data = tableData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching table data", Error = ex.Message });
            }
        }



    }
}