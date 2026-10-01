using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true";
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            try {
                var cmd = new SqlCommand(@"
                    CREATE TABLE ProjectManagementInvitations (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        ProjectId int NOT NULL,
                        AgencyId int NOT NULL,
                        ManagerName nvarchar(200) NOT NULL,
                        ManagerPhone nvarchar(50) NOT NULL,
                        ManagerEmail nvarchar(200) NULL,
                        InvitationToken nvarchar(100) NOT NULL,
                        Status nvarchar(50) DEFAULT 'Bekliyor',
                        SentAt datetime2 NOT NULL DEFAULT GETDATE(),
                        AcceptedAt datetime2 NULL,
                        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
                        UpdatedAt datetime2 NULL,
                        IsDeleted bit NOT NULL DEFAULT 0
                    );", conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Created ProjectManagementInvitations");
            } catch (Exception ex) { Console.WriteLine("Table Error: " + ex.Message); }
        }
    }
}
