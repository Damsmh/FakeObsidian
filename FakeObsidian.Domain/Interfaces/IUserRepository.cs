using FakeObsidian.Domain.Entities;

namespace FakeObsidian.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<ICollection<AppUser>> GetAllAsync();
        public Task<AppUser> GetByNumber(int number);
        public Task<AppUser> GetByIdAsync(int id);
        public Task AddAsync(AppUser user);
        public Task UpdateAsync(AppUser user);
        public Task DeleteAsync(int id);
    }
}
