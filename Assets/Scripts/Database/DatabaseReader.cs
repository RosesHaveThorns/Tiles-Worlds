using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;

public class DatabaseReader {
    private static string db_addr = "URI=file:" + Application.dataPath + "/Data/gamedata";

    private IDbConnection db_conn;

    public DatabaseReader() {
        db_conn = (IDbConnection) new SqliteConnection(db_addr);
        db_conn.Open();
    }

    public DataTable query(string sqlQuery) {
        IDbCommand db_cmd = db_conn.CreateCommand();
        db_cmd.CommandText = sqlQuery;
        IDataReader reader = db_cmd.ExecuteReader();

        DataTable result = new DataTable();
        try {
        result.Load(reader);
        } catch (ConstraintException e) {
            Debug.Log("Issue with constraints during database query, likely no action required\n" + e);
        } // catches failed constraints exception, as some JOINs can confuse DataTable

        reader.Close();
        db_cmd.Dispose();

        return result;
    }

    public void nonQuery(string sql) {
        IDbCommand db_cmd = db_conn.CreateCommand();
        db_cmd.CommandText = sql;

        db_cmd.ExecuteNonQuery();

        db_cmd.Dispose();
    }

    public void close() {
        db_conn.Close();
    }
}