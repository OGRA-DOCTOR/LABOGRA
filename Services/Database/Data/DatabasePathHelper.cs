// بداية الكود لملف Services/Database/Data/DatabasePathHelper.cs
using System;
using System.IO;

namespace LABOGRA.Services.Database.Data
{
    public static class DatabasePathHelper
    {
        private static string? _cachedConnectionString;

        public static string GetConnectionString()
        {
            if (_cachedConnectionString != null)
            {
                return _cachedConnectionString;
            }

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDirectory = Path.Combine(appDataPath, "LABOGRA");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            var dbFileName = "labdatabase.db";
            var dbPath = Path.Combine(dbDirectory, dbFileName);

            _cachedConnectionString = $"Data Source={dbPath}";
            return _cachedConnectionString;
        }
    }
}
// نهاية الكود لملف Services/Database/Data/DatabasePathHelper.cs