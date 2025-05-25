// بداية الكود لملف Services/Database/Data/LabDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LABOGRA.Services.Database.Data
{
    public class LabDbContextFactory : IDesignTimeDbContextFactory<LabDbContext>
    {
        public LabDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();
            optionsBuilder.UseSqlite(DatabasePathHelper.GetConnectionString());
            return new LabDbContext(optionsBuilder.Options);
        }
    }
}
// نهاية الكود لملف Services/Database/Data/LabDbContextFactory.cs