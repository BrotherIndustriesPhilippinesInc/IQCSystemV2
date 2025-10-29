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
using System.Linq.Dynamic.Core;
using IQC_API.Functions;
using System.Linq.Expressions;
using DataTableRequest = IQC_API.Functions.DataTableRequest;

namespace IQC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectionDetailsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;
        private readonly IQC_APIContext _userContext;

        public InspectionDetailsController(IQC_API_PG_Context context, IQC_APIContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspectionDetailsNoMESDataDTO>>> GetInspectionDetails()
        {
            var inspections = await _context.InspectionDetails.ToListAsync();

            var inspectionDtos = inspections.Select(inspection => new InspectionDetailsNoMESDataDTO
            {
                Id = inspection.Id,
                CheckLot = inspection.CheckLot,
                DimenstionsMaxSamplingCheckQty = inspection.DimenstionsMaxSamplingCheckQty,
                ContinuedEligibility = inspection.ContinuedEligibility,
                RelatedCheckLot = inspection.RelatedCheckLot,
                StockInCollectDate = inspection.StockInCollectDate,
                PartCode = inspection.PartCode,
                SamplingCheckQty = inspection.SamplingCheckQty,
                FactoryCode = inspection.FactoryCode,
                PartName = inspection.PartName,
                AllowQty = inspection.AllowQty,
                Standard = inspection.Standard,
                TotalLotQty = inspection.TotalLotQty,
                SamplingRejectQty = inspection.SamplingRejectQty,
                IQCCheckDate = inspection.IQCCheckDate,
                ClassOne = inspection.ClassOne,
                SamplingCheckDefectiveQty = inspection.SamplingCheckDefectiveQty,
                LotJudge = inspection.LotJudge,
                OccuredEngineer = inspection.OccuredEngineer,
                CheckMonitor = inspection.CheckMonitor,
                LotNo = inspection.LotNo,
                ClassTwo = inspection.ClassTwo,
                RejectQty = inspection.RejectQty,
                ProcessMethod = inspection.ProcessMethod,
                CheckUser = inspection.CheckUser,
                ProficienceLevel = inspection.ProficienceLevel,
                FirstSize = inspection.FirstSize,
                SecondSize = inspection.SecondSize,
                Supervisor = inspection.Supervisor,
                ModelNo = inspection.ModelNo,
                DesignNoticeNo = inspection.DesignNoticeNo,
                FirstAppearance = inspection.FirstAppearance,
                SecondAppearance = inspection.SecondAppearance,
                ActualCheckTime = inspection.ActualCheckTime,
                FourMNumber = inspection.FourMNumber,
                Remarks = inspection.Remarks,
                OutgoingInspectionReport = inspection.OutgoingInspectionReport,
                ThreeCDataConfirm = inspection.ThreeCDataConfirm,
                CreatedBy = inspection.CreatedBy,
                CreatedDate = inspection.CreatedDate,
                IsApproved = inspection.IsApproved,
                Approver = inspection.Approver
            }).ToList();

            return Ok(inspectionDtos);
        }

        [HttpPost("datatable")]
        public IActionResult GetInspectionDetailsForDataTable([FromBody] DataTableRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid request body",
                    errors = ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            field = e.Key,
                            error = e.Value.Errors.First().ErrorMessage
                        })
                });
            }

            try
            {
                var baseQuery = _context.InspectionDetails.AsQueryable();

                // Apply search and ordering
                var query = baseQuery
                    .ApplySearch(request, new Expression<Func<InspectionDetails, object>>[]
                    {
                x => x.CheckLot,
                x => x.PartCode,
                x => x.PartName,
                x => x.FactoryCode,
                x => x.IQCCheckDate,
                x => x.Remarks,
                x => x.CreatedBy,
                x => x.Approver
                    })
                    .ApplyOrdering(request);

                var totalRecords = _context.InspectionDetails.Count();
                var filteredRecords = query.Count();

                // Apply paging before loading to memory
                var pagedQuery = query.ApplyPaging(request);

                // Get all part codes from the current page to avoid unnecessary lookups
                var partCodes = pagedQuery.Select(x => x.PartCode).Distinct().ToList();

                // Fetch all parts info for these codes in one go
                var partsInfo = _context.PartsInformation
                    .Where(p => partCodes.Contains(p.PartCode))
                    .AsEnumerable() // 👈 Switch to client-side processing here
                    .GroupBy(p => p.PartCode)
                    .Select(g => new
                    {
                        PartCode = g.Key,
                        VendorSupplierMerged = string.Join(
                            "\n",
                            g.Select((x, i) => $"{i + 1}. {x.VendorCode} -> ({x.SupplierName})")
                        )
                    })
                    .ToDictionary(g => g.PartCode, g => g.VendorSupplierMerged);


                // Build data for DataTables
                var data = pagedQuery
                    .AsEnumerable()
                    .Select(x => new InspectionDetailsNoMESDataDTO
                    {
                        Id = x.Id,
                        CheckLot = x.CheckLot,
                        DimenstionsMaxSamplingCheckQty = x.DimenstionsMaxSamplingCheckQty,
                        ContinuedEligibility = x.ContinuedEligibility,
                        RelatedCheckLot = x.RelatedCheckLot,
                        StockInCollectDate = x.StockInCollectDate,
                        PartCode = x.PartCode,
                        SamplingCheckQty = x.SamplingCheckQty,
                        FactoryCode = x.FactoryCode,
                        PartName = x.PartName,
                        AllowQty = x.AllowQty,
                        Standard = x.Standard,
                        TotalLotQty = x.TotalLotQty,
                        SamplingRejectQty = x.SamplingRejectQty,
                        IQCCheckDate = x.IQCCheckDate,
                        ClassOne = x.ClassOne,
                        SamplingCheckDefectiveQty = x.SamplingCheckDefectiveQty,
                        LotJudge = x.LotJudge,
                        OccuredEngineer = x.OccuredEngineer,
                        CheckMonitor = x.CheckMonitor,
                        LotNo = x.LotNo,
                        ClassTwo = x.ClassTwo,
                        RejectQty = x.RejectQty,
                        ProcessMethod = x.ProcessMethod,
                        CheckUser = x.CheckUser,
                        ProficienceLevel = x.ProficienceLevel,
                        FirstSize = x.FirstSize,
                        SecondSize = x.SecondSize,
                        Supervisor = x.Supervisor,
                        ModelNo = x.ModelNo,
                        DesignNoticeNo = x.DesignNoticeNo,
                        FirstAppearance = x.FirstAppearance,
                        SecondAppearance = x.SecondAppearance,
                        ActualCheckTime = x.ActualCheckTime,
                        FourMNumber = x.FourMNumber,
                        Remarks = x.Remarks,
                        OutgoingInspectionReport = x.OutgoingInspectionReport,
                        ThreeCDataConfirm = x.ThreeCDataConfirm,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        IsApproved = x.IsApproved,
                        Approver = x.Approver,
                        // 🧩 Merge multiple vendor/supplier rows into one cell
                        VendorSupplierMerged = partsInfo.ContainsKey(x.PartCode)
                            ? partsInfo[x.PartCode]
                            : ""
                    })
                    .ToList();

                // Wrap response
                var response = new Functions.DataTableResponse<InspectionDetailsNoMESDataDTO>
                {
                    Draw = request.Draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = filteredRecords,
                    Data = data
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Something went wrong on the server",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        // GET: api/InspectionDetails
        [HttpGet("supervisor/{supervisorName}")]
        public async Task<ActionResult<IEnumerable<InspectionDetails>>> GetInspectionDetails([FromRoute] string supervisorName)
        {
            List<InspectionDetails> inspections;

            if (supervisorName != "Marc Racille Arenga")
            {
                inspections = await _context.InspectionDetails
                    .Where(x => /*x.Supervisor == supervisorName &&*/ x.IsApproved == false)
                    .ToListAsync();
            }
            else
            {
                inspections = await _context.InspectionDetails
                    .Where(x => x.IsApproved == false)
                    .ToListAsync();
            }

            return Ok(inspections);
        }


        // GET: api/InspectionDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InspectionDetails>> GetInspectionDetails(int id)
        {
            var inspectionDetails = await _context.InspectionDetails.FindAsync(id);

            if (inspectionDetails == null)
            {
                return NotFound();
            }

            var inspector = await _userContext.SystemApproverList
            .Where(x => x.ADID == inspectionDetails.CheckUser)
            .Select(x => x.FullName) // only grab the full name
            .FirstOrDefaultAsync();

            if (inspector == null)
            {
                return NotFound();
            }

            var details = new
            {
                Inspection = inspectionDetails,
                InspectorFullName = inspector
            };

            return Ok(details);
        }

        // PUT: api/InspectionDetails/5
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

        public class ApproveInspectionRequest
        {
            public string Checklot { get; set; }
            public string Approver { get; set; }
        }

        [HttpPost("ApproveInspection")]
        public async Task<IActionResult> ApproveInspectionDetails([FromBody] ApproveInspectionRequest request)
        {
            var inspectionDetails = await _context.InspectionDetails
                .Where(x => x.CheckLot == request.Checklot)
                .FirstOrDefaultAsync();

            if (inspectionDetails == null)
            {
                return NotFound();
            }

            string approverFullName = await _userContext.SystemApproverList
                    .Where(x => x.EmployeeNumber == request.Approver)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync();

            inspectionDetails.Approver = approverFullName;
            inspectionDetails.IsApproved = true;
            _context.Entry(inspectionDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InspectionDetailsExists(inspectionDetails.Id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        // POST: api/InspectionDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InspectionDetails>> PostInspectionDetails(InspectionDetails inspectionDetails)
        {
            // check if record already exists by CheckLot
            var existing = await _context.InspectionDetails
                .FirstOrDefaultAsync(x => x.CheckLot == inspectionDetails.CheckLot);

            if (existing != null)
            {
                // update existing entity
                existing.DimenstionsMaxSamplingCheckQty = inspectionDetails.DimenstionsMaxSamplingCheckQty;
                existing.ContinuedEligibility = inspectionDetails.ContinuedEligibility;
                existing.RelatedCheckLot = inspectionDetails.RelatedCheckLot;
                existing.StockInCollectDate = inspectionDetails.StockInCollectDate;
                existing.PartCode = inspectionDetails.PartCode;
                existing.SamplingCheckQty = inspectionDetails.SamplingCheckQty;
                existing.FactoryCode = inspectionDetails.FactoryCode;
                existing.PartName = inspectionDetails.PartName;
                existing.AllowQty = inspectionDetails.AllowQty;
                existing.Standard = inspectionDetails.Standard;
                existing.TotalLotQty = inspectionDetails.TotalLotQty;
                existing.SamplingRejectQty = inspectionDetails.SamplingRejectQty;
                existing.IQCCheckDate = inspectionDetails.IQCCheckDate;
                existing.ClassOne = inspectionDetails.ClassOne;
                existing.SamplingCheckDefectiveQty = inspectionDetails.SamplingCheckDefectiveQty;
                existing.LotJudge = inspectionDetails.LotJudge;
                existing.OccuredEngineer = inspectionDetails.OccuredEngineer;
                existing.CheckMonitor = inspectionDetails.CheckMonitor;
                existing.LotNo = inspectionDetails.LotNo;
                existing.ClassTwo = inspectionDetails.ClassTwo;
                existing.RejectQty = inspectionDetails.RejectQty;
                existing.ProcessMethod = inspectionDetails.ProcessMethod;
                existing.CheckUser = inspectionDetails.CheckUser;
                existing.ProficienceLevel = inspectionDetails.ProficienceLevel;
                existing.FirstSize = inspectionDetails.FirstSize;
                existing.SecondSize = inspectionDetails.SecondSize;
                existing.Supervisor = inspectionDetails.Supervisor;
                existing.ModelNo = inspectionDetails.ModelNo;
                existing.DesignNoticeNo = inspectionDetails.DesignNoticeNo;
                existing.FirstAppearance = inspectionDetails.FirstAppearance;
                existing.SecondAppearance = inspectionDetails.SecondAppearance;
                existing.ActualCheckTime = inspectionDetails.ActualCheckTime;
                existing.FourMNumber = inspectionDetails.FourMNumber;
                existing.Remarks = inspectionDetails.Remarks;
                existing.OutgoingInspectionReport = inspectionDetails.OutgoingInspectionReport;
                existing.ThreeCDataConfirm = inspectionDetails.ThreeCDataConfirm;
                existing.CreatedBy = inspectionDetails.CreatedBy;
                existing.IsApproved = inspectionDetails.IsApproved;
                existing.Approver = inspectionDetails.Approver;
                existing.VisualCheckItems = inspectionDetails.VisualCheckItems;
                existing.DimensionCheckItems = inspectionDetails.DimensionCheckItems;
                existing.CreatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(existing); // return updated record
            }
            else
            {
                // insert new entity
                inspectionDetails.CreatedDate = DateTime.UtcNow;
                _context.InspectionDetails.Add(inspectionDetails);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetInspectionDetails",
                    new { id = inspectionDetails.Id }, inspectionDetails);
            }
        }


        // DELETE: api/InspectionDetails/5
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

        [HttpGet("UpdateApprovalStatus/{checkLot}")]
        public async Task<InspectionDetails> UpdateApprovalStatus(string checkLot)
        {
            var inspectionDetails = await _context.InspectionDetails.FirstOrDefaultAsync(x => x.CheckLot == checkLot && x.IsApproved == true);
            inspectionDetails.IsApproved = true;
            await _context.SaveChangesAsync();
            return inspectionDetails;
        }
    }
}