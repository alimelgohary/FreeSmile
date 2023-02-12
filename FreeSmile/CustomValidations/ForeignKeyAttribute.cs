using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FreeSmile.CustomValidations
{
    public class ForeignKeyAttribute : RequiredAttribute
    {
        public string className { get; set; }
        public string colName { get; set; }

        public ForeignKeyAttribute(string className, string colName)
        {
            this.className = className;
            this.colName = colName;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (Exsists(className, colName, (int)value))
                return true;

            return false;
        }
        public static bool Exsists(string className, string colName, int value)
        {
            var tableName = Plural(className);
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE {colName}=@{colName}";
            using var context = new FreeSmileContext();
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new SqlParameter($"@{colName}", value));
            context.Database.OpenConnection();

            var result = command.ExecuteScalar();

            if (result.ToString() == 1.ToString())
                return true;
            else
                return false;
        }

        private static string Plural(string noun)
        {
            if(noun.EndsWith('y'))
            {
                return noun.Substring(0, noun.Length - 1) + "ies";
            }
            return noun + 's';
        }
    }
}

