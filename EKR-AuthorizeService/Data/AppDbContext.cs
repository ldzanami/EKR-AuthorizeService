using Microsoft.EntityFrameworkCore;
using EKR_AuthorizeService.Entities;

namespace EKR_AuthorizeService.Data
{
    /// <summary>
    /// Контекст БД приложения.
    /// </summary>
    /// <param name="options">Параметры контекста.</param>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Сущность Users.
        /// </summary>
        public DbSet<User> Users { get; set; }
        
        /// <summary>
        /// Сущность Sessions.
        /// </summary>
        public DbSet<Session> Sessions { get; set; }

        /// <summary>
        /// Сущность Logs.
        /// </summary>
        public DbSet<Log> Logs { get; set; }

        /// <summary>
        /// Особенности содзания схемы БД.
        /// </summary>
        /// <param name="modelBuilder">Объект проектировщика БД.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasMany(user => user.Sessions)
                                       .WithOne(session => session.User)
                                       .HasForeignKey(session => session.UserId)
                                       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
