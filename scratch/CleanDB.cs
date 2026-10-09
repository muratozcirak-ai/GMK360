using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

var connectionString = "Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true";
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Buildings WHERE BlockName IN ('A Blok', 'B Blok', 'Tek Yapı', 'B Yapısı') AND ParentBuildingId IS NULL;";
    int deleted = command.ExecuteNonQuery();
    Console.WriteLine($"Deleted {deleted} corrupted old blocks.");
}
