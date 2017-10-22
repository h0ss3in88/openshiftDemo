using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Npgsql;

namespace app.Models
{
  public static class Extensions
  {
    public static IEnumerable<T> ToList<T>(this IDataReader reader) where T : new()
    {
      while (reader.Read())
      {
        yield return reader.ToSingle<T>();
      }
    }
    public static dynamic ToSingleDynamic(this IDataReader reader)
    {
      var expandoObject = new ExpandoObject();
      var d = expandoObject as IDictionary<string, object>;
      for (var i = 0; i < reader.FieldCount; i++)
      {
        TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
        var replacedName = reader.GetName(i).Replace("_", " ").ToLower();
        var name = textInfo.ToTitleCase(replacedName).Replace(" ",string.Empty);
        d.Add(name,DBNull.Value.Equals(reader.GetValue(i)) ? null : reader.GetValue(i));
      }
      return expandoObject;
    }
    public static T ToSingle<T>(this IDataReader reader) where T : new()
    {
      var item = new T();
      var properties = item.GetType().GetProperties();
      foreach (var property in properties)
      {
        for (var i = 0; i < reader.FieldCount; i++)
        {
          if (property.Name.Equals(reader.GetName(i), StringComparison.InvariantCultureIgnoreCase))
          {
            property.SetValue(item,DBNull.Value.Equals(reader.GetValue(i)) ? null : reader.GetValue(i));
          }
        }
      }
      return item;
    }
    public static IEnumerable<dynamic> DynamicList(this IDataReader reader)
    {
      while (reader.Read())
      {
        yield return reader.ToSingleDynamic();
      }
    }
    public static void AddParam(this NpgsqlCommand command, object arg)
    {
      var parameter = new NpgsqlParameter();
      parameter.ParameterName = string.Format("@{0}", command.Parameters.Count);
      if (arg == null)
      {
        parameter.Value = DBNull.Value;
      }
      else
      {
        if (arg is Guid)
        {
          parameter.Value = arg.ToString();
          parameter.DbType = DbType.Guid;
          parameter.Size = 4000;
        }
        if (arg is string)
        {
          parameter.Value = arg.ToString();
          parameter.Size = arg.ToString().Length > 4000 ? -1 : 4000;
        }
        else
        {
          parameter.Value = arg;
        }
      }
      command.Parameters.Add(parameter);
    }
  }
}
