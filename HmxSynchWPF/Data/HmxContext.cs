using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.IO;
using HmxSynchWPF.Models;

namespace HmxSynchWPF.Data
{
    public class HmxContext : DbContext
    {
        private const string NameOfDb = "HmxProgrammeDb.sqlite";

        public HmxContext()
        {
            CreateDbIfMissing();
        }

        private void CreateDbIfMissing()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HmxContext"].ConnectionString;
            if (!File.Exists(NameOfDb))
            {
                SQLiteConnection.CreateFile(NameOfDb);
                using (var con = new SQLiteConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(Properties.Settings.Default.SqlLiteCreateScript, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
        }

        public DbSet<Episode> Episodes { get; set; }

    }

    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
}