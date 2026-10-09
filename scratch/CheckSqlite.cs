using System;
using Microsoft.Data.Sqlite;

class Program {
    static void Main() {
        using (var connection = new SqliteConnection("Data Source=C:\\Users\\murat\\source\\repos\\GMK360\\TempAddressDb2\\turkiye-il-ilce-sokak-mahalle-veri-tabani-master\\dumps\\tr_adres.db")) {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, adi FROM il LIMIT 5;";
            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    Console.WriteLine(reader.GetString(0) + " - " + reader.GetString(1));
                }
            }
        }
    }
}
