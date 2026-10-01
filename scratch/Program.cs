using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=(localdb)\\mssqllocaldb;Database=GMK360;Trusted_Connection=True;MultipleActiveResultSets=true";
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            try {
                var cmd1 = new SqlCommand("ALTER TABLE ProjectLegalDocuments ADD IsActive bit NOT NULL DEFAULT 1;", conn);
                cmd1.ExecuteNonQuery();
                Console.WriteLine("Added IsActive to ProjectLegalDocuments");
            } catch (Exception ex) { Console.WriteLine("Table 1: " + ex.Message); }

            try {
                var cmd2 = new SqlCommand("ALTER TABLE ModuleDocumentRules ADD IsActive bit NOT NULL DEFAULT 1;", conn);
                cmd2.ExecuteNonQuery();
                Console.WriteLine("Added IsActive to ModuleDocumentRules");
            } catch (Exception ex) { Console.WriteLine("Table 2: " + ex.Message); }
        }
    }
}
