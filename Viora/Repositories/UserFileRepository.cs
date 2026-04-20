using Microsoft.EntityFrameworkCore;
using Viora.Data;
using Viora.Models;

namespace Viora.Repositories
{
    public class UserFileRepository:GenericRepository<UserFile>
    {
        private readonly VioraDBContext _context;

        public UserFileRepository(VioraDBContext context) : base(context)
        {
            _context = context;
        }


        public async Task<UserFile?> GetById(int? documentId, int userId)
        {
            return await _context.UserFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId && d.UserOwnerId == userId);
        }
    }
}
