using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using (var client = new HttpClient())
        {
            try
            {
                var resp = await client.GetAsync("http://localhost:5248/api/Location/districts/34");
                Console.WriteLine("Status: " + resp.StatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Console.WriteLine("Content: " + content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
