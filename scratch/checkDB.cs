using System;
using System.Linq;
using Microsoft.EntityFrameworkData;
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=GMK360.Web/GMK360Db.sqlite";
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, BlockName, IsExistingBuilding, ParentBuildingId FROM Buildings WHERE ConstructionProjectId IS NOT NULL;";
    using (var reader = command.ExecuteReader())
    {
        Console.WriteLine("ID | BlockName | IsExistingBuilding | ParentBuildingId");
        while (reader.Read())
        {
            Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetBoolean(2)} | {(reader.IsDBNull(3) ? "NULL" : reader.GetInt32(3).ToString())}");
        }
    }
}
