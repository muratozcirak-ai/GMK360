using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string conn = @"Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;";
        using (var connection = new SqlConnection(conn)) {
            connection.Open();
            var cmd = new SqlCommand("SELECT TOP 5 Name FROM Cities ORDER BY Id DESC", connection);
            using (var reader = cmd.ExecuteReader()) {
                while(reader.Read()){
                    Console.WriteLine(reader.GetString(0));
                }
            }
        }
    }
}
