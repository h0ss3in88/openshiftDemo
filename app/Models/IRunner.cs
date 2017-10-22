using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace app.Models
{
  public interface IRunner
  {
    string ConnectionString { get; set; }
    IEnumerable<object> ExecuteDynamic(string command, params object[] args);
    dynamic ExecuteToSingleDynamic(string command, params object[] args);
    T ExecuteToSingle<T>(string command, params object[] args) where T : new();
    IEnumerable<T> Execute<T>(string sqlCommand, params object[] args) where T : new();
    Task<NpgsqlDataReader> OpenReaderAsync(string sql, params object[] args);
    NpgsqlDataReader OpenReader(string sql, params object[] args);
    NpgsqlCommand BuildCommand(string sql, object[] args);
    IList<int> Transact(params NpgsqlCommand[] commands);
  }
}