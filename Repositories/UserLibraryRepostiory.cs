using CatalogAPI.Data;
using CatalogAPI.Models;

namespace CatalogAPI.Repositories
{
    public class UserLibraryRepository: IUserLibraryRepository
    {
        private readonly AppDbContext _context;

        public UserLibraryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserLibrary library)
        {
            _context.Libraries.Add(library);

            await _context.SaveChangesAsync();
        }
    }
}
