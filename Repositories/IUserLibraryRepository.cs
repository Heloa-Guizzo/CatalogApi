using CatalogAPI.Models;

namespace CatalogAPI.Repositories
{
    public interface IUserLibraryRepository
    {
        Task AddAsync(UserLibrary library);
    }
}
