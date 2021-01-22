using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequelizeModels
{
    class Program
    {
        static void Main(string[] args)
        {
            Create();
        }

        static string Camelize(string name)
        {
            var result = "";
            var words = name.Split('_');

            foreach (var item in words)
            {
                result += item[0].ToString().ToUpper() + item.Substring(1);
            }
            return result;
        }

        static string GetDataType(string row)
        {
            if (row.ToUpper().Contains("VARCHAR"))
            {
                if (row.Contains("("))
                {
                    var length = row.Split('(', ')');
                    return $"STRING({length[1]})";
                }
                return "STRING";
            }
            if (row.ToUpper().Contains("DATE"))
            {
                return "DATE";
            }
            if (row.ToUpper().Contains("DECIMAL"))
            {
                if (row.Contains("("))
                {
                    var length = row.Split('(', ')');
                    return $"DECIMAL({length[1]})";
                }
                return "DECIMAL";
            }
            if (row.ToUpper().Contains("BIT"))
            {
                return "BOOLEAN";
            }
            return "INTEGER";
        }

        static string GetAllowNull(string row)
        {
            if (row.ToUpper().Contains("NOT NULL"))
            {
                return "false";
            }
            return "true";
        }

        static void Create()
        {
            using (var sql = new StreamReader(@"C:\users\nivaldo.santana\Desktop\prodetur_.sql"))
            {
                var row = sql.ReadLine();
                while (!row.Contains("fimz"))
                {
                    if (row.ToUpper().Contains("CREATE TABLE"))
                    {
                        char simbol = '`';
                        var table = row.Split(simbol)[1];
                        var modelPath = $@"C:\users\nivaldo.santana\Desktop\models\{table}.js";
                        row = sql.ReadLine();
                        using (var model = new StreamWriter(modelPath))
                        {
                            model.WriteLine("module.exports = (sequelize, DataTypes) => {");
                            model.WriteLine($"  const model = sequelize.define('{Camelize(table)}', {{");
                            while (!row.Contains("PRIMARY KEY"))
                            {
                                if (row.Contains('`'))
                                {
                                    var field = row.Split(simbol)[1];
                                    model.WriteLine($"    {field}: {{");
                                    model.WriteLine($"      type: DataTypes.{GetDataType(row)},");
                                    if (row.ToUpper().Contains("`ID`"))
                                    {
                                        model.WriteLine("      primaryKey: true,");
                                    }
                                    model.WriteLine($"      allowNull: {GetAllowNull(row)}");
                                    model.WriteLine("    },");
                                }
                                row = sql.ReadLine();
                            }
                            model.WriteLine("  }, {");
                            model.WriteLine($"    tableName: '{table}',");
                            model.WriteLine("  });");
                            model.WriteLine("  model.associate = (models) => {");
                            model.WriteLine("  };");
                            model.WriteLine("  return model;");
                            model.WriteLine("};");
                        }
                    }
                    row = sql.ReadLine();
                }
            }
        }
    }
}
