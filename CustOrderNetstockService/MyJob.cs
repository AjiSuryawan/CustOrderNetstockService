using CsvHelper;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace CustOrderNetstockService
{
    internal class MyJob : IJob
    {
        public void mainExecute()
        {
            string csvFilePath = ConfigurationManager.AppSettings["csvFilePath"];
            string tableName = ConfigurationManager.AppSettings["TableName"];

            string DBServer = ConfigurationManager.AppSettings["DBServer"];
            string DBName = ConfigurationManager.AppSettings["DBName"];
            string DBUser = ConfigurationManager.AppSettings["DBUser"];
            string DBPass = ConfigurationManager.AppSettings["DBPass"];
            string Timeout = ConfigurationManager.AppSettings["Timeout"];

            string connectionString = @"Server=" + DBServer + ";Database=" + DBName + ";Uid=" + DBUser + ";Pwd=" + DBPass + ";Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Integrated Security=True";
            string jsonFilePath = ConfigurationManager.AppSettings["jsonFilePath"];
            string jsonString = File.ReadAllText(jsonFilePath);
            DateFormatConfig dateFormatConfig = JsonConvert.DeserializeObject<DateFormatConfig>(jsonString);
            string dateFormat = dateFormatConfig.OrderDate;
            Console.WriteLine("data dari json "+dateFormat);
            string query = "SELECT * FROM "+ tableName;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    using (var writer = new StreamWriter(csvFilePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        while (reader.Read())
                        {
                            var values = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (reader.GetName(i) == "OrderDate" || reader.GetName(i) == "RequestDate") // Assuming the column name is "OrderDate"
                                {
                                    DateTime orderDate;
                                    Console.WriteLine("Data from database: " + reader[i].ToString().Trim() + ", Length: " + reader[i].ToString().Trim().Length);
                                    // Try parsing the date using multiple formats
                                    if (DateTime.TryParseExact(reader[i].ToString().Trim(), dateFormatConfig.SourceFormat, null, System.Globalization.DateTimeStyles.None, out orderDate))
                                    {
                                        // Format the parsed date into the target format
                                        string formattedDate = orderDate.ToString(dateFormat);
                                        values[i] = formattedDate;
                                        Console.WriteLine("Formatted date: " + formattedDate);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unable to parse the date.");
                                        values[i] = reader[i].ToString().Trim();
                                    }
                                }
                                else
                                {
                                    values[i] = reader[i].ToString().Trim(); // Trim each value to remove leading and trailing spaces
                                }
                            }
                            string line = string.Join(";", values);
                            writer.WriteLine(line);
                        }
                    }
                }
            }

        }

        public async System.Threading.Tasks.Task Execute(IJobExecutionContext context)
        {
            mainExecute();
        }


    }
}
