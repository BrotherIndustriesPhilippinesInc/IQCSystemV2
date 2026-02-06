using IQC_API.Data;
using IQC_API.DTO.User;
using Microsoft.EntityFrameworkCore;

namespace IQC_API.Services
{
    public class UserService : IUserService
    {
        private readonly IQC_APIContext _context;

        public UserService(IQC_APIContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetUserByEmpNoAsync(string empNo)
        {
            // 1. Fetch the raw data
            // Assuming your DB entity class is called 'EmsView' based on your PHP table name
            var record = await _context.EmsViews
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmpNo == empNo);

            if (record == null) return null;

            // 2. Perform the Business Logic (Translation)
            // This logic is now isolated here, reusable by other controllers if needed
            string translatedSection = record.Section switch
            {
                "TC" => "Tape Cassette",
                "BPS" => "BPS",
                "IC" => "Ink Cartridge",
                "IH" => "Ink Head",
                "PT" => "P-Touch",
                "MOLD" => "Molding",
                "PCBA" => "PCBA",
                "PR1" => "Printer 1",
                "PR2" => "Printer 2",
                "TN" => "Toner",
                _ => record.Section
            };

            // 3. Map to DTO
            return new UserDto
            {
                EmpNo = record.EmpNo,
                FullName = record.FullName, // Assuming a Name column exists
                Section = translatedSection,
                Department = record.Department,
                Status = record.Status,
                Company = record.Company,
                Email = record.Email,
                Expr1 = record.Expr1,
                ADID = record.ADID,
                RFID_AMS = record.RFID_AMS,
                Position = record.Position,
                LastName = record.LastName,
                Gender = record.Gender,
                DateHired = record.DateHired,
                CostCode = record.CostCode
            };
        }
    }
}
