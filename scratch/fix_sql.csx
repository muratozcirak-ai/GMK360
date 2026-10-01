using System;
using Microsoft.Data.SqlClient;

string connectionString = "Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true";

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();
    
    // Check and add AverageFlatSalePrice
    string checkCol1 = "SELECT COUNT(*) FROM sys.columns WHERE Name = N'AverageFlatSalePrice' AND Object_ID = Object_ID(N'ConstructionProjects')";
    using (SqlCommand cmd1 = new SqlCommand(checkCol1, connection))
    {
        int count = (int)cmd1.ExecuteScalar();
        if (count == 0)
        {
            string alter1 = "ALTER TABLE ConstructionProjects ADD AverageFlatSalePrice decimal(18,2) NOT NULL DEFAULT 0";
            using (SqlCommand cmdAlter = new SqlCommand(alter1, connection))
            {
                cmdAlter.ExecuteNonQuery();
                Console.WriteLine("Added AverageFlatSalePrice");
            }
        }
    }

    // Check and add ExpectedTotalShopRevenue
    string checkCol2 = "SELECT COUNT(*) FROM sys.columns WHERE Name = N'ExpectedTotalShopRevenue' AND Object_ID = Object_ID(N'ConstructionProjects')";
    using (SqlCommand cmd2 = new SqlCommand(checkCol2, connection))
    {
        int count = (int)cmd2.ExecuteScalar();
        if (count == 0)
        {
            string alter2 = "ALTER TABLE ConstructionProjects ADD ExpectedTotalShopRevenue decimal(18,2) NOT NULL DEFAULT 0";
            using (SqlCommand cmdAlter = new SqlCommand(alter2, connection))
            {
                cmdAlter.ExecuteNonQuery();
                Console.WriteLine("Added ExpectedTotalShopRevenue");
            }
        }
    }
}
Console.WriteLine("Database updated successfully.");
