using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace FreeSmile.CustomValidations
{
    public class UniqueAttribute : RequiredAttribute
    {
        public string className { get; set; }
        public string colName { get; set; }

        public UniqueAttribute(string className, string colName)
        {
            this.className = className;
            this.colName = colName;
        }
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;
            
            if (IsUnique(className, colName, value.ToString()))
                return true;
            
            return false;
        }
        public static bool IsUnique(string className, string colName, string value)
        {
            var query = $"SELECT COUNT(*) FROM {className}s WHERE {colName}=@{colName}";
            using var context = new FreeSmileContext();
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new SqlParameter($"@{colName}", value));
            context.Database.OpenConnection();

            var result = command.ExecuteScalar();
            
            if (result.ToString() == 0.ToString())
                return true;
            else
                return false;
        }
    }
}

