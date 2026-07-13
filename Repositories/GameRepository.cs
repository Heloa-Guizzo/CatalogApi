using CatalogAPI.Data;
using CatalogAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;

        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Game game)
        {
            _context.Games.Add(game);

            await _context.SaveChangesAsync();
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
            return await _context.Games
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Game>> GetAllAsync()
        {
            return await _context.Games
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            _context.Games.Update(game);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Game game)
        {
            _context.Games.Remove(game);

            await _context.SaveChangesAsync();
        }
    }

}
