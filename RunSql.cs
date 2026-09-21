using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = ""Server=(localdb)\\MSSQLLocalDB;Database=GMK360Db;Trusted_Connection=True;"";
        using (SqlConnection conn = new SqlConnection(connStr)) {
            conn.Open();
            try {
                SqlCommand cmd = new SqlCommand(""ALTER TABLE Buildings ADD LayoutPattern nvarchar(100) NULL, AttachedToBlock nvarchar(100) NULL;"", conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine(""Columns added successfully."");
            } catch (Exception ex) {
                Console.WriteLine(""Error (might already exist): "" + ex.Message);
            }
        }
    }
}
