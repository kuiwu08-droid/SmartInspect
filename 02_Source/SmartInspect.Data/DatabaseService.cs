using Microsoft.Data.Sqlite;
using SmartInspect.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartInspect.Data
{
    public class DatabaseService
    {
        private readonly string databasePath;
        private readonly string connectionString;

        public DatabaseService()
        {
            databasePath =
                Path.Combine(AppContext.BaseDirectory, "smartinspect.db");

            connectionString =
                "Data Source=" + databasePath;
        }

        public void InitializeDatabase()
        {
            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string createTableSql = """
                CREATE TABLE IF NOT EXISTS InspectionRecords
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BatchNumber TEXT NOT NULL,
                    SerialNumber TEXT NOT NULL,
                    Measurement REAL NOT NULL,
                    Result TEXT NOT NULL,
                    InspectTime TEXT NOT NULL
                );
                """;

            using SqliteCommand command =
                new SqliteCommand(createTableSql, connection);

            command.ExecuteNonQuery();
        }

        public void SaveProduct(Product product)
        {
            if (string.IsNullOrEmpty(product.InspectTime)) product.InspectTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string insertSql = """
                INSERT INTO InspectionRecords
                (
                    BatchNumber,
                    SerialNumber,
                    Measurement,
                    Result,
                    InspectTime
                )
                VALUES
                (
                    $batchNumber,
                    $serialNumber,
                    $measurement,
                    $result,
                    $inspectTime
                );
                """;

            using SqliteCommand command =
                new SqliteCommand(insertSql, connection);

            command.Parameters.AddWithValue(
                "$batchNumber",
                product.BatchNumber);

            command.Parameters.AddWithValue(
                "$serialNumber",
                product.SerialNumber);

            command.Parameters.AddWithValue(
                "$measurement",
                product.Measurement);

            command.Parameters.AddWithValue(
                "$result",
                product.Result);

            command.Parameters.AddWithValue(
                "$inspectTime",
                product.InspectTime);

            command.ExecuteNonQuery();
        }

        public List<Product> LoadProducts()
        {
            List<Product> products =
                new List<Product>();

            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            string selectSql = """
                SELECT
                    BatchNumber,
                    SerialNumber,
                    Measurement,
                    Result,
                    InspectTime
                FROM InspectionRecords
                ORDER BY
                    CAST(SUBSTR(BatchNumber, 6) AS INTEGER) DESC,
                    CAST(SUBSTR(SerialNumber, 3) AS INTEGER) ASC,
                    Id ASC;
                """;

            using SqliteCommand command =
                new SqliteCommand(selectSql, connection);

            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Product product = new Product();

                product.BatchNumber =
                    reader.GetString(0);

                product.SerialNumber =
                    reader.GetString(1);

                product.Measurement =
                    reader.GetDouble(2);

                product.Result =
                    reader.GetString(3);

                product.InspectTime = reader.GetString(4);

                products.Add(product);
            }

            return products;
        }

        public void ClearHistory()
        {
            using SqliteConnection connection =
                new SqliteConnection(connectionString);

            connection.Open();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            using SqliteCommand deleteCommand =
                connection.CreateCommand();

            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText =
                "DELETE FROM InspectionRecords;";

            deleteCommand.ExecuteNonQuery();

            using SqliteCommand resetIdCommand =
                connection.CreateCommand();

            resetIdCommand.Transaction = transaction;
            resetIdCommand.CommandText = """
                DELETE FROM sqlite_sequence
                WHERE name = 'InspectionRecords';
                """;

            resetIdCommand.ExecuteNonQuery();

            transaction.Commit();
        }
    }
}
