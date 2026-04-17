using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Viora.Data;

namespace Viora.Repositories
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        private readonly VioraDBContext _context;
        public GenericRepository(VioraDBContext context) { 
            _context = context;
        
        }

        //Create
        public async Task Add(TEntity entity)
        {
           await _context.Set<TEntity>().AddAsync(entity);
        }


        //Read
        public async Task<TEntity> GetById(int id) { 
            return await _context.Set<TEntity>().FindAsync(id);
        }

        //Get all
        public async Task<IEnumerable<TEntity>> GetAll() { 
            return await _context.Set<TEntity>().ToListAsync();
        }


        //Update
        public void Update(TEntity entity) {
            _context.Set<TEntity>().Update(entity);
        }

        //Delete
        public async Task Delete(int id) { 
            var entity =  await _context.Set<TEntity>().FindAsync(id);
            if (entity != null) {
                _context.Set<TEntity>().Remove(entity);
            }

        }

        //Save changes
        public async Task SaveChanges() {
            await _context.SaveChangesAsync();
        }

        //Find with condition
        public async Task<TEntity?> FindAsync(Expression<Func<TEntity,bool>> preddicate)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(preddicate);
        }
    }
}
