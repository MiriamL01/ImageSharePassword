using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ImageSharePassword.Data
{
    public class ImageShareManager
    {
        private readonly string _connectionString;
        
        public ImageShareManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddImage(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Images (FileName, Password, Views) " +
                "VALUES(@fileName, @password, 0) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@fileName", image.FileName);
            cmd.Parameters.AddWithValue("@password", image.Password);
            connection.Open();
            image.Id = (int)(decimal)cmd.ExecuteScalar();
        }
        public Image GetImageById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT TOP 1 * FROM Images WHERE @id = Id";
            cmd.Parameters.AddWithValue("@id",id);
            connection.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Image
            {
                Id = (int)reader["Id"],
                FileName = (string)reader["FileName"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"]
            };
        }
        public void AddImageView(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Images SET Views = Views +1 "+
                "WHERE @id = Id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
