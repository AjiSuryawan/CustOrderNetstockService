using CsvHelper;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace CustOrderNetstockService
{
    // Define classes to represent the JSON structure

    internal class MyJob : IJob
    {
        public void mainExecute()
        {
            string csvFilePath = ConfigurationManager.AppSettings["csvFilePath"];
            string tableName = ConfigurationManager.AppSettings["TableName"];
            string jsonFilePath = ConfigurationManager.AppSettings["jsonFilePath"];

            // Load and deserialize JSON format specifications
            string jsonString = File.ReadAllText(jsonFilePath);
            FormatConfig formatConfig = JsonConvert.DeserializeObject<FormatConfig>(jsonString);

            string DBServer = ConfigurationManager.AppSettings["DBServer"];
            string DBName = ConfigurationManager.AppSettings["DBName"];
            string DBUser = ConfigurationManager.AppSettings["DBUser"];
            string DBPass = ConfigurationManager.AppSettings["DBPass"];
            string Timeout = ConfigurationManager.AppSettings["Timeout"];

            string connectionString = @"Server=" + DBServer + ";Database=" + DBName + ";Uid=" + DBUser + ";Pwd=" + DBPass + ";Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Integrated Security=True";
            string query = "SELECT * FROM " + tableName;

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
                            var values = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);

                                // Check if there is a format specification for this column
                                var formatSpec = formatConfig.Formats.Find(f => f.FieldName == columnName);
                                
                                if (formatSpec != null)
                                {
                                    if (formatSpec.DataType == "date")
                                    {
                                        string dateString = reader[i].ToString().Trim();
                                        try
                                        {
                                            DateTime dateValue;
                                            if (DateTime.TryParseExact(reader.GetDateTime(i).ToString(formatSpec.Format), formatSpec.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                                            {
                                                values.Add(dateValue.ToString(formatSpec.Format)); // Use Format from JSON
                                            }
                                        }
                                        catch (FormatException)
                                        {
                                            Console.WriteLine("Parsing failed for column '" + columnName + "'. Original value added: " + dateString.Trim());
                                            values.Add(dateString);
                                        }


                                    }
                                    else if (formatSpec.DataType == "Numeric")
                                    {
                                        // Adjust formatting for numeric types
                                        decimal numericValue;
                                        if (decimal.TryParse(reader[i].ToString(), out numericValue))
                                        {
                                            values.Add(numericValue.ToString(formatSpec.Format)); // Use Format from JSON
                                        }
                                        else
                                        {
                                            values.Add(reader[i].ToString().Trim());
                                        }
                                    }
                                    else
                                    {
                                        values.Add(reader[i].ToString().Trim());
                                    }

                                }
                                else
                                {
                                    values.Add(reader[i].ToString().Trim());
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
