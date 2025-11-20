using Microsoft.EntityFrameworkCore;

namespace ToDoListWebAppPracticing.Models
{
    public class ToDoListDbContext : DbContext
    {
        public ToDoListDbContext (DbContextOptions<ToDoListDbContext> options) : base(options) { }
        
        public DbSet<ToDoListItems> ToDoLists { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = "Work", Name = "งาน" },
                new Category { CategoryId = "Personal", Name = "ส่วนตัว" },
                new Category { CategoryId = "Shopping", Name = "ช็อปปิ้ง" },
                new Category { CategoryId = "Others", Name = "อื่นๆ" }
            );
            modelBuilder.Entity<Status>().HasData(
                new Status { StatusId = "Open", StatusName = "เปิด" },
                new Status { StatusId = "InProgress", StatusName = "กำลังดำเนินการ" },
                new Status { StatusId = "Completed", StatusName = "เสร็จสิ้น" }
            );
        }
    }
}
