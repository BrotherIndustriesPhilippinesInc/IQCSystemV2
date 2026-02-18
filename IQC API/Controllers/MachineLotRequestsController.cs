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
        private readonly IQC_APIContext _sqlContext;

        public MachineLotRequestsController(IQC_API_PG_Context context, IQC_APIContext sqlContext)
        {
            _context = context;
            _sqlContext = sqlContext;
        }

        // GET: api/MachineLotRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MachineLotRequestGetDTO>>> GetMachineLotRequest()
        {
            // 1. Get the Main Data (MachineLotRequest)
            // We do NOT fetch the names yet.
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
                    CreatedBy = x.CreatedBy, // Ensure this holds the Employee Number
                    CreatedDate = x.CreatedDate,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate,
                    WhatForName = x.WhatFor.WhatForName,
                    ReleaseReasonName = x.ReleaseReason.ReleaseReasonName,
                    CheckLot = x.CheckLot,
                    LotNumber = x.LotNumber,
                    ModelCode = _context.PartsInformation.Where(y => y.PartCode == x.PartCode)
                        .Select(p => p.ModelCategory)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // 2. Extract the Employee Numbers (Distinct) to minimize DB lookup
            // We filter out nulls so we don't query for nothing.
            var employeeNumbers = data
                .Where(x => !string.IsNullOrEmpty(x.CreatedBy))
                .Select(x => x.CreatedBy)
                .Distinct()
                .ToList();

            // 3. Fetch Full Names from the OTHER Context (_sqlContext)
            // Warning: SystemApproverList usually contains duplicates if an approver 
            // is listed for multiple systems. We GroupBy or Select Distinct to avoid crashing the Dictionary.
            var userNames = await _sqlContext.SystemApproverList
                .Where(x => employeeNumbers.Contains(x.EmployeeNumber)) // Use the column name you gave me
                .Select(x => new { x.EmployeeNumber, x.FullName }) // Select only what we need
                .Distinct() // specific to your DB, ensure you don't get duplicates
                .ToDictionaryAsync(
                    k => k.EmployeeNumber,
                    v => v.FullName
                );

            // 4. Fetch the Approvers List (The one you had before)
            var approvers = await _sqlContext.SystemApproverList
                .Where(x => x.SystemID == 73)
                .ToListAsync();

            // 5. Stitch it all together in memory
            foreach (var item in data)
            {
                // Assign the Creator's Full Name using the Dictionary
                if (item.CreatedBy != null && userNames.TryGetValue(item.CreatedBy, out var fullName))
                {
                    item.CreatedByFullName = fullName;
                }
                else
                {
                    // Optional: Handle cases where the ID isn't found in the Approver List
                    item.CreatedByFullName = "Unknown Employee";
                }

                // Assign the generic approver list
                //item.ApproverList = approvers;
            }

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
                    ReleaseReasonName = x.ReleaseReason.ReleaseReasonName,

                    LotNumber = x.LotNumber,

                    ModelCode = _context.PartsInformation.Where(y => y.PartCode == x.PartCode)
                        .Select(p => p.ModelCategory)
                        .FirstOrDefault()
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
                CheckLot = machineLotRequest.CheckLot,
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
                CreatedDate = DateTime.Now, 

                LotNumber = machineLotRequest.LotNumber
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

        [HttpPost("assignReleaseNo")]
        public async Task<ActionResult<MachineLotRequest>> AssignReleaseNo(AssignReleaseNo machineLotRequest)
        {
            //Get latest 
            MachineLotRequest entity = await _context.MachineLotRequest
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.CheckLot == machineLotRequest.CheckLot);
            entity.ReleaseNo = machineLotRequest.ReleaseNo;
            await _context.SaveChangesAsync();

            return entity;
        }

        [HttpPost("updateMachineLotRequest")]
        public async Task<ActionResult<MachineLotRequestGetDTO>> UpdateMachineLotRequest(MachineLotRequestUpdateDTO dto)
        {
            // 1. GET THE TARGET (The "Where" part)
            // We fetch it first because standard EF Core needs to track it to update it.
            // We Include NOW so we don't have to query the database a second time later.
            var targetEntity = await _context.MachineLotRequest
                .Include(x => x.WhatFor)
                .Include(x => x.ReleaseReason)
                .FirstOrDefaultAsync(x => x.ReleaseNo == dto.ReleaseNo);

            if (targetEntity == null)
            {
                return NotFound($"Request with ReleaseNo '{dto.ReleaseNo}' not found.");
            }

            // 2. APPLY CHANGES (The "Update" part)
            // We map the values from your DTO onto the loaded entity.
            targetEntity.PartCode = dto.PartCode;
            targetEntity.PartName = dto.PartName;
            targetEntity.VendorName = dto.VendorName;
            targetEntity.Quantity = dto.Quantity;
            targetEntity.YellowCard = dto.YellowCard;
            targetEntity.DCIOtherNo = dto.DCIOtherNo;
            targetEntity.Remarks = dto.Remarks;

            // Foreign Keys
            // FIND THE RELEASE REASON ID VIA RELEASE REASON CODE
            
            int releaseReasonId = _context.ReleaseReason.Where(x => x.ReleaseReasonCode == dto.ReleaseReasonId).FirstOrDefault().Id;
            if (releaseReasonId == null)
            {
                return NotFound($"Release Reason with Release Reason Code '{releaseReasonId}' not found.");
            }

            targetEntity.ReleaseReasonId = releaseReasonId;

            // Audit Fields
            targetEntity.ModifiedBy = dto.CreatedBy;
            targetEntity.ModifiedDate = DateTime.Now;

            // 3. SAVE
            // EF Core detects the changes in 'targetEntity' and generates the UPDATE SQL.
            await _context.SaveChangesAsync();

            // 4. MAP TO RESPONSE (The "Hydration" part)
            // Since we used .Include() in Step 1, targetEntity.WhatFor is ALREADY loaded.
            // We don't need to fetch again!
            var response = new MachineLotRequestGetDTO
            {
                // Now targetEntity.Id is valid because it came from the DB
                Id = targetEntity.Id,
                ReleaseNo = targetEntity.ReleaseNo,
                PartCode = targetEntity.PartCode,
                PartName = targetEntity.PartName,
                VendorName = targetEntity.VendorName,
                Quantity = targetEntity.Quantity,
                CheckLot = targetEntity.CheckLot,
                YellowCard = targetEntity.YellowCard,
                DCIOtherNo = targetEntity.DCIOtherNo,
                Remarks = targetEntity.Remarks,

                // Flatten Navigation Properties
                WhatForName = targetEntity.WhatFor?.WhatForName,
                WhatForCode = targetEntity.WhatFor?.WhatForCode,

                ReleaseReasonName = targetEntity.ReleaseReason?.ReleaseReasonName,
                ReleaseReasonCode = targetEntity.ReleaseReason?.ReleaseReasonCode,

                CreatedBy = targetEntity.CreatedBy,
                CreatedDate = targetEntity.CreatedDate,
                ModifiedBy = targetEntity.ModifiedBy,
                ModifiedDate = targetEntity.ModifiedDate
            };

            return Ok(response);
        }

        [HttpPost("deleteMachineLotRequest")]
        public async Task<IActionResult> DeleteMachineLotRequest(string releaseNo)
        {
            var machineLotRequest = await _context.MachineLotRequest.FirstOrDefaultAsync(x => x.ReleaseNo == releaseNo);
            if (machineLotRequest == null)
            {
                return NotFound();
            }

            _context.MachineLotRequest.Remove(machineLotRequest);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("MachineLotRequestExportation")]
        public async Task<ActionResult<MachineLotRequestGetDTO>> MachineLotRequestExportation(string releaseNo, string exportedBy)
        {
            // 1. Find the entity
            var machineLotRequestEntity = await _context.MachineLotRequest.FirstOrDefaultAsync(x => x.ReleaseNo == releaseNo);

            if (machineLotRequestEntity == null)
            {
                return NotFound();
            }

            // 2. Update the entity
            machineLotRequestEntity.ExportedBy = exportedBy;
            machineLotRequestEntity.ExportDate = DateTime.UtcNow;

            // 3. Save changes to DB
            await _context.SaveChangesAsync();

            // ---------------------------------------------------------
            // THE CHEAT FIX: Call your existing GET method manually!
            // ---------------------------------------------------------

            // We call the method directly. Since it returns ActionResult<T>, we await it.
            var resultAction = await GetMachineLotRequest(machineLotRequestEntity.Id);

            // 4. Extract the DTO from the result
            // Since your GetMachineLotRequest returns the object directly (not wrapped in Ok()), 
            // the data is in .Value.
            if (resultAction.Value != null)
            {
                // Return the FULL DTO (which includes ModelCode)
                return CreatedAtAction("GetMachineLotRequest", new { id = machineLotRequestEntity.Id }, resultAction.Value);
            }

            // Fallback: If for some reason the GET failed (it shouldn't), return the entity.
            // But this will still be missing ModelCode, so don't rely on this hitting often.
            return CreatedAtAction("GetMachineLotRequest", new { id = machineLotRequestEntity.Id }, machineLotRequestEntity);
        }
    }
}
