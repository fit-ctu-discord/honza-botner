using HonzaBotner.Database;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services
{
    public class RemindersService : IRemindersService
    {
        private readonly HonzaBotnerDbContext _context;

        public RemindersService(HonzaBotnerDbContext context)
        {
            _context = context;
        }
    }
}
