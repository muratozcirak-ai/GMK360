using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string conn = @"Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;";
        using (var connection = new SqlConnection(conn)) {
            connection.Open();
            var cmd = new SqlCommand("INSERT INTO Streets (Name, NeighborhoodId, CreatedAt, IsActive, IsDeleted) SELECT 'Merkez Sokak (Test)', Id, GETDATE(), 1, 0 FROM Neighborhoods", connection);
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows + " streets added.");
        }
    }
}
