using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace DataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlitePath = @"C:\Users\murat\source\repos\GMK360\TempAddressDb2\turkiye-il-ilce-sokak-mahalle-veri-tabani-master\dumps\tr_adres.db";
            string sqlServerConnString = "Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

            Console.WriteLine("Importing Address Data from SQLite to SQL Server...");
            
            using var sqliteConn = new SqliteConnection($"Data Source={sqlitePath}");
            sqliteConn.Open();

            var illerTable = new DataTable("Cities");
            illerTable.Columns.Add("Id", typeof(int));
            illerTable.Columns.Add("CountryId", typeof(int));
            illerTable.Columns.Add("Name", typeof(string));
            illerTable.Columns.Add("PlateCode", typeof(string));
            illerTable.Columns.Add("CreatedAt", typeof(DateTime));
            illerTable.Columns.Add("IsDeleted", typeof(bool));

            using (var cmd = new SqliteCommand("SELECT il_id, il_adi FROM iller", sqliteConn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["il_id"]);
                    illerTable.Rows.Add(id, 1, reader["il_adi"].ToString(), id.ToString().PadLeft(2, '0'), DateTime.UtcNow, false);
                }
            }
            Console.WriteLine($"Cities loaded: {illerTable.Rows.Count}");

            var ilcelerTable = new DataTable("Districts");
            ilcelerTable.Columns.Add("Id", typeof(int));
            ilcelerTable.Columns.Add("CityId", typeof(int));
            ilcelerTable.Columns.Add("Name", typeof(string));
            ilcelerTable.Columns.Add("CreatedAt", typeof(DateTime));
            ilcelerTable.Columns.Add("IsDeleted", typeof(bool));

            using (var cmd = new SqliteCommand("SELECT ilce_id, il_id, ilce_adi FROM ilceler", sqliteConn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ilcelerTable.Rows.Add(Convert.ToInt32(reader["ilce_id"]), Convert.ToInt32(reader["il_id"]), reader["ilce_adi"].ToString(), DateTime.UtcNow, false);
                }
            }
            Console.WriteLine($"Districts loaded: {ilcelerTable.Rows.Count}");

            var mahallelerTable = new DataTable("Neighborhoods");
            mahallelerTable.Columns.Add("Id", typeof(int));
            mahallelerTable.Columns.Add("DistrictId", typeof(int));
            mahallelerTable.Columns.Add("Name", typeof(string));
            mahallelerTable.Columns.Add("ZipCode", typeof(string));
            mahallelerTable.Columns.Add("CreatedAt", typeof(DateTime));
            mahallelerTable.Columns.Add("IsDeleted", typeof(bool));

            using (var cmd = new SqliteCommand("SELECT mahalle_id, ilce_id, mahalle_adi FROM mahalleler", sqliteConn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    mahallelerTable.Rows.Add(Convert.ToInt32(reader["mahalle_id"]), Convert.ToInt32(reader["ilce_id"]), reader["mahalle_adi"].ToString(), null, DateTime.UtcNow, false);
                }
            }
            Console.WriteLine($"Neighborhoods loaded: {mahallelerTable.Rows.Count}");

            var sokaklarTable = new DataTable("Streets");
            sokaklarTable.Columns.Add("Id", typeof(int));
            sokaklarTable.Columns.Add("NeighborhoodId", typeof(int));
            sokaklarTable.Columns.Add("Name", typeof(string));
            sokaklarTable.Columns.Add("CreatedAt", typeof(DateTime));
            sokaklarTable.Columns.Add("IsDeleted", typeof(bool));

            using (var cmd = new SqliteCommand("SELECT sokak_id, mahalle_id, sokak_adi FROM sokaklar", sqliteConn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    sokaklarTable.Rows.Add(Convert.ToInt32(reader["sokak_id"]), Convert.ToInt32(reader["mahalle_id"]), reader["sokak_adi"].ToString(), DateTime.UtcNow, false);
                }
            }
            Console.WriteLine($"Streets loaded: {sokaklarTable.Rows.Count}");

            Console.WriteLine("Connecting to SQL Server...");
            using var sqlConn = new SqlConnection(sqlServerConnString);
            sqlConn.Open();

            Console.WriteLine("Clearing old tables...");
            using (var cmd = new SqlCommand("DELETE FROM Streets; DELETE FROM Neighborhoods; DELETE FROM Districts; DELETE FROM Cities;", sqlConn))
            {
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Bulk inserting Cities...");
            using (var bulk = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity, null) { DestinationTableName = "Cities" })
            {
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("CountryId", "CountryId");
                bulk.ColumnMappings.Add("Name", "Name");
                bulk.ColumnMappings.Add("PlateCode", "PlateCode");
                bulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
                bulk.ColumnMappings.Add("IsDeleted", "IsDeleted");
                bulk.WriteToServer(illerTable);
            }

            Console.WriteLine("Bulk inserting Districts...");
            using (var bulk = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity, null) { DestinationTableName = "Districts" })
            {
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("CityId", "CityId");
                bulk.ColumnMappings.Add("Name", "Name");
                bulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
                bulk.ColumnMappings.Add("IsDeleted", "IsDeleted");
                bulk.WriteToServer(ilcelerTable);
            }

            Console.WriteLine("Bulk inserting Neighborhoods...");
            using (var bulk = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity, null) { DestinationTableName = "Neighborhoods", BatchSize = 10000 })
            {
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("DistrictId", "DistrictId");
                bulk.ColumnMappings.Add("Name", "Name");
                bulk.ColumnMappings.Add("ZipCode", "ZipCode");
                bulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
                bulk.ColumnMappings.Add("IsDeleted", "IsDeleted");
                bulk.WriteToServer(mahallelerTable);
            }

            Console.WriteLine("Bulk inserting Streets (This might take 1-2 minutes)...");
            using (var bulk = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity, null) { DestinationTableName = "Streets", BatchSize = 50000, BulkCopyTimeout = 600, NotifyAfter = 200000 })
            {
                bulk.SqlRowsCopied += (s, e) => Console.WriteLine($"Copied {e.RowsCopied} streets...");
                bulk.ColumnMappings.Add("Id", "Id");
                bulk.ColumnMappings.Add("NeighborhoodId", "NeighborhoodId");
                bulk.ColumnMappings.Add("Name", "Name");
                bulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
                bulk.ColumnMappings.Add("IsDeleted", "IsDeleted");
                bulk.WriteToServer(sokaklarTable);
            }

            Console.WriteLine("IMPORT COMPLETE!");
        }
    }
}
