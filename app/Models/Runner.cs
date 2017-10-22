using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace app.Models
{
  public class Runner : IRunner
  {
    public string ConnectionString { get; set; }

    public Runner(string connectionString)
    {
      ConnectionString = connectionString;
    }

    public IEnumerable<dynamic> ExecuteDynamic(string command, params object[] args)
    {
      using (var connection = new NpgsqlConnection(ConnectionString))
      {
        var cmd = BuildCommand(command,args);
        cmd.Connection = connection;
        connection.Open();
        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
          yield return reader.DynamicList();
        }
      }
    }

    public dynamic ExecuteToSingleDynamic(string command, params object[] args)
    {
      return ExecuteDynamic(command, args).FirstOrDefault();
    }
    public T ExecuteToSingle<T>(string command, params object[] args) where T : new()
    {
      return Execute<T>(command, args).FirstOrDefault();
    }
    public IEnumerable<T> Execute<T>(string sqlCommand, params object[] args) where T : new()
    {
      var reader = OpenReader(sqlCommand, args);
      while (reader.Read())
      {
        yield return reader.ToSingle<T>();
      }
      reader.Dispose();
    }
    public async Task<NpgsqlDataReader> OpenReaderAsync(string sql, params object[] args)
    {
      var connection = new NpgsqlConnection(ConnectionString);
      var cmd = BuildCommand(sql, args);
      cmd.Connection = connection;
      await connection.OpenAsync();
      return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection) as NpgsqlDataReader;
    }
    public NpgsqlDataReader OpenReader(string sql, params object[] args)
    {
      var connection = new NpgsqlConnection(ConnectionString);
      var command = BuildCommand(sql, args);
      command.Connection = connection;
      connection.Open();
      return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public NpgsqlCommand BuildCommand(string sql, object[] args)
    {
      var cmd = new NpgsqlCommand(sql);
      if (args == null) return cmd;
      foreach (var arg in args)
      {
        cmd.AddParam(arg);
      }
      return cmd;
    }
    public IList<int> Transact(params NpgsqlCommand[] commands)
    {
      var result = new List<int>();
      using (var connection = new NpgsqlConnection(ConnectionString))
      {
        connection.Open();
        using (var tx = connection.BeginTransaction())
        {
          try
          {
            foreach (var command in commands)
            {
              command.Transaction = tx;
              command.Connection = connection;
              result.Add(command.ExecuteNonQuery());
            }
            tx.Commit();
          }
          catch (NpgsqlException e)
          {
            tx.Rollback();
            throw e;
          }
          finally
          {
            connection.Close();
          }
        }
        return result;
      }
    }
  }
}
