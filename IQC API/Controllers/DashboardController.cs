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

        [HttpGet("inspection-summary")]
        public IActionResult InspectionSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var allData = _context.InspectionDetails
                                      .Where(x => !string.IsNullOrEmpty(x.IQCCheckDate))
                                      .AsEnumerable()
                                      .ToList();

                // Filter by date safely
                var filtered = allData.Where(x =>
                {
                    if (!DateTime.TryParse(x.IQCCheckDate, out var date))
                        return false; // skip invalid dates

                    return (!startDate.HasValue || date >= startDate.Value) &&
                           (!endDate.HasValue || date <= endDate.Value);
                }).ToList();

                var totalInspections = filtered.Count;

                var dailyTrends = filtered
                    .GroupBy(x => DateTime.Parse(x.IQCCheckDate).Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalInspections = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                return Ok(new
                {
                    TotalInspections = totalInspections,
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
        public IActionResult ApprovalSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Start query in the database
                var query = _context.InspectionDetails.AsQueryable();

                // Only include rows with valid date strings
                query = query.Where(x => !string.IsNullOrEmpty(x.IQCCheckDate));

                // Convert and filter in memory (since IQCCheckDate is a string)
                var data = query.AsEnumerable()
                                .Where(x => DateTime.TryParse(x.IQCCheckDate, out _))
                                .Select(x => new
                                {
                                    IsApproved = x.IsApproved,
                                    IQCDate = DateTime.Parse(x.IQCCheckDate).Date
                                })
                                .Where(x =>
                                    (!startDate.HasValue || x.IQCDate >= startDate.Value.Date) &&
                                    (!endDate.HasValue || x.IQCDate <= endDate.Value.Date))
                                .ToList();

                // Totals
                var totalInspections = data.Count;
                var approvedCount = data.Count(x => x.IsApproved == true);

                // Daily group
                var dailyTrends = data
                    .GroupBy(x => x.IQCDate)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalInspections = g.Count(),
                        ApprovedCount = g.Count(x => x.IsApproved == true)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                return Ok(new
                {
                    TotalInspections = totalInspections,
                    ApprovedCount = approvedCount,
                    StartDate = startDate,
                    EndDate = endDate,
                    DailyTrends = dailyTrends
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching approval summary", Error = ex.Message });
            }
        }

        [HttpGet("members")]
        public async Task<ActionResult<IEnumerable<object>>> GetInspectionMembers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            // Fetch safely
            var inspections = _context.InspectionDetails
                .Where(x => !string.IsNullOrEmpty(x.IQCCheckDate))
                .AsEnumerable() // everything after this runs in-memory
                .Select(x =>
                {
                    if (DateTime.TryParse(x.IQCCheckDate, out var dt))
                        return new { x.CheckUser, IQCDate = dt.Date };
                    return null; // skip invalid dates
                })
                .Where(x => x != null); 

            // Apply date range filter
            if (startDate.HasValue)
                inspections = inspections.Where(x => x.IQCDate >= startDate.Value.Date);
            if (endDate.HasValue)
                inspections = inspections.Where(x => x.IQCDate <= endDate.Value.Date);

            // Group by user and count
            var inspectionCounts = inspections
                .GroupBy(x => x.CheckUser)
                .Select(g => new
                {
                    User = g.Key,
                    NumberOfInspection = g.Count()
                })
                .OrderBy(x => x.User)
                .ToList();

            return Ok(inspectionCounts);
        }
    }
}