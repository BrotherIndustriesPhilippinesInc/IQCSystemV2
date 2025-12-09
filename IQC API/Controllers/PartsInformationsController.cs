using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQC_API.Data;
using IQC_API.Models;
using OfficeOpenXml;
using Npgsql;
using IQC_API.Functions;

namespace IQC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsInformationsController : ControllerBase
    {
        private readonly IQC_API_PG_Context _context;

        public PartsInformationsController(IQC_API_PG_Context context)
        {
            _context = context;
        }

        // GET: api/PartsInformations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartsInformation>>> GetPartsInformation()
        {
            return await _context.PartsInformation.ToListAsync();
        }

        // GET: api/PartsInformations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PartsInformation>> GetPartsInformation(int id)
        {
            var partsInformation = await _context.PartsInformation.FindAsync(id);

            if (partsInformation == null)
            {
                return NotFound();
            }

            return partsInformation;
        }

        // PUT: api/PartsInformations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPartsInformation(int id, PartsInformation partsInformation)
        {
            if (id != partsInformation.Id)
            {
                return BadRequest();
            }

            _context.Entry(partsInformation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartsInformationExists(id))
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

        // POST: api/PartsInformations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("upload")]
        public async Task<ActionResult> PostPartsInformation(IFormFile file, string username)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            ExcelPackage.License.SetNonCommercialOrganization("BPS");

            var partsList = new List<PartsInformation>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets["Parts Masterlist"];
                    if (worksheet == null)
                        return BadRequest("Worksheet 'Parts Masterlist' not found.");

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 8; row <= rowCount; row++)
                    {
                        string vendorCode = worksheet.Cells[row, 5].Text?.Trim();
                        string partCode = worksheet.Cells[row, 6].Text?.Trim();
                        string plant = worksheet.Cells[row, 7].Text?.Trim();

                        if (string.IsNullOrEmpty(vendorCode) || string.IsNullOrEmpty(partCode) || string.IsNullOrEmpty(plant))
                            continue;

                        var part = new PartsInformation
                        {
                            Category = worksheet.Cells[row, 2].Text,
                            StandardTaktTime = worksheet.Cells[row, 3].Text,
                            N1N2 = worksheet.Cells[row, 4].Text,
                            VendorCode = vendorCode,
                            PartCode = partCode,
                            Plant = plant,
                            PartName = worksheet.Cells[row, 8].Text,
                            SupplierName = worksheet.Cells[row, 9].Text,
                            OverseasManufacturer = worksheet.Cells[row, 10].Text,
                            Model = worksheet.Cells[row, 11].Text,
                            ModelCategory = worksheet.Cells[row, 12].Text,
                            QMLotCategory = worksheet.Cells[row, 13].Text,
                            SAPMasterRegistration = worksheet.Cells[row, 14].Text,
                            QualityLevel = worksheet.Cells[row, 15].Text,
                            STS_N1 = worksheet.Cells[row, 16].Text,
                            InspectionStatus = worksheet.Cells[row, 17].Text,
                            FutureInspectionStatus = worksheet.Cells[row, 18].Text,
                            SFCategory = worksheet.Cells[row, 19].Text,
                            PSMark = worksheet.Cells[row, 20].Text,
                            Markings = worksheet.Cells[row, 21].Text,
                            CriticalComponentSafety = worksheet.Cells[row, 22].Text,
                            SupplierData = worksheet.Cells[row, 23].Text,
                            CommonGroup = worksheet.Cells[row, 24].Text,
                            N_Z = worksheet.Cells[row, 25].Text,
                            EOL = worksheet.Cells[row, 26].Text,
                            EOLDate = worksheet.Cells[row, 27].Text,
                            Remarks = worksheet.Cells[row, 28].Text,
                            Reference_STS_Checking = worksheet.Cells[row, 29].Text,
                            STS_Normal_N1 = worksheet.Cells[row, 30].Text,
                            QM_Lot_Category = worksheet.Cells[row, 31].Text,
                            JIT = worksheet.Cells[row, 32].Text,
                            Sloc = worksheet.Cells[row, 33].Text,
                            Size = worksheet.Cells[row, 34].Text,
                            VisualTT = worksheet.Cells[row, 35].Text,
                            DimensionTT = worksheet.Cells[row, 36].Text,
                            VisualDimension = worksheet.Cells[row, 37].Text,
                            CreatedDate = DateTime.Now,
                            CreatedBy = username,
                            LastUpdate = DateTime.Now,
                            UpdatedBy = username
                        };

                        partsList.Add(part);
                    }
                }
            }

            if (!partsList.Any())
                return BadRequest("No valid data found.");

            var connectionString = _context.Database.GetConnectionString();
            var added = 0;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // 🔥 Truncate before inserting (faster than DELETE)
                using (var truncateCmd = new NpgsqlCommand(@"TRUNCATE TABLE ""PartsInformation"" RESTART IDENTITY CASCADE;", conn))
                {
                    await truncateCmd.ExecuteNonQueryAsync();
                }

                using var writer = conn.BeginBinaryImport(@"
            COPY ""PartsInformation"" (
                ""Category"", ""StandardTaktTime"", ""N1N2"", ""VendorCode"", ""PartCode"", ""PartName"",
                ""Plant"", ""SupplierName"", ""OverseasManufacturer"", ""Model"", ""ModelCategory"",
                ""QMLotCategory"", ""SAPMasterRegistration"", ""QualityLevel"", ""STS_N1"", ""InspectionStatus"",
                ""FutureInspectionStatus"", ""SFCategory"", ""PSMark"", ""Markings"", ""CriticalComponentSafety"",
                ""SupplierData"", ""CommonGroup"", ""N_Z"", ""EOL"", ""EOLDate"", ""Remarks"",
                ""Reference_STS_Checking"", ""STS_Normal_N1"", ""QM_Lot_Category"", ""JIT"", ""Sloc"",
                ""Size"", ""VisualTT"", ""DimensionTT"", ""VisualDimension"", ""CreatedDate"",
                ""CreatedBy"", ""LastUpdate"", ""UpdatedBy"")
            FROM STDIN (FORMAT BINARY)");

                foreach (var p in partsList)
                {
                    await writer.StartRowAsync();
                    await writer.WriteAsync(p.Category);
                    await writer.WriteAsync(p.StandardTaktTime);
                    await writer.WriteAsync(p.N1N2);
                    await writer.WriteAsync(p.VendorCode);
                    await writer.WriteAsync(p.PartCode);
                    await writer.WriteAsync(p.PartName);
                    await writer.WriteAsync(p.Plant);
                    await writer.WriteAsync(p.SupplierName);
                    await writer.WriteAsync(p.OverseasManufacturer);
                    await writer.WriteAsync(p.Model);
                    await writer.WriteAsync(p.ModelCategory);
                    await writer.WriteAsync(p.QMLotCategory);
                    await writer.WriteAsync(p.SAPMasterRegistration);
                    await writer.WriteAsync(p.QualityLevel);
                    await writer.WriteAsync(p.STS_N1);
                    await writer.WriteAsync(p.InspectionStatus);
                    await writer.WriteAsync(p.FutureInspectionStatus);
                    await writer.WriteAsync(p.SFCategory);
                    await writer.WriteAsync(p.PSMark);
                    await writer.WriteAsync(p.Markings);
                    await writer.WriteAsync(p.CriticalComponentSafety);
                    await writer.WriteAsync(p.SupplierData);
                    await writer.WriteAsync(p.CommonGroup);
                    await writer.WriteAsync(p.N_Z);
                    await writer.WriteAsync(p.EOL);
                    await writer.WriteAsync(p.EOLDate);
                    await writer.WriteAsync(p.Remarks);
                    await writer.WriteAsync(p.Reference_STS_Checking);
                    await writer.WriteAsync(p.STS_Normal_N1);
                    await writer.WriteAsync(p.QM_Lot_Category);
                    await writer.WriteAsync(p.JIT);
                    await writer.WriteAsync(p.Sloc);
                    await writer.WriteAsync(p.Size);
                    await writer.WriteAsync(p.VisualTT);
                    await writer.WriteAsync(p.DimensionTT);
                    await writer.WriteAsync(p.VisualDimension);
                    await writer.WriteAsync(p.CreatedDate);
                    await writer.WriteAsync(p.CreatedBy);
                    await writer.WriteAsync(p.LastUpdate);
                    await writer.WriteAsync(p.UpdatedBy);
                    added++;
                }

                await writer.CompleteAsync();
            }

            return Ok(new
            {
                Message = $"Table truncated and bulk insert complete: {added} records inserted successfully."
            });
        }

        // DELETE: api/PartsInformations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartsInformation(int id)
        {
            var partsInformation = await _context.PartsInformation.FindAsync(id);
            if (partsInformation == null)
            {
                return NotFound();
            }

            _context.PartsInformation.Remove(partsInformation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PartsInformationExists(int id)
        {
            return _context.PartsInformation.Any(e => e.Id == id);
        }

        [HttpPost("datatables")]
        public async Task<IActionResult> PartsInformationDatatables([FromBody] DataTableRequest request)
        {
            var query = _context.PartsInformation.AsQueryable();

            var result = await DataTableHelper.BuildResponseAutoAsync(query, request);
            return Ok(result);
        }

        [HttpGet("distinct_vendorcodes")]
        public async Task<ActionResult<IEnumerable<object>>> DistinctVendorCode()
        {
            var results = await _context.PartsInformation
                .Where(x => x.VendorCode != null
                         && x.SupplierName != null
                         && x.VendorCode != "N/A")
                .GroupBy(x => x.VendorCode)
                .Select(g => new
                {
                    VendorCode = g.Key,
                    SupplierName = g.First().SupplierName
                })
                .OrderBy(x => x.SupplierName)
                .ToListAsync();

            return results;
        }
    }
}
