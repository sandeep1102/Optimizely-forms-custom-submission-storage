namespace CustomStorage.Web.React.SubmissionStorage
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Sql Helper for DB Interaction.
    /// </summary>
    public class SqlHelper : ISqlHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelper"/> class.
        /// </summary>
        public SqlHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("EPiServerDB"));
        }

        private SqlCommand GetSqlCommand(SqlConnection connection, string query)
        {
            return new SqlCommand(query, connection);
        }

        /// <summary>
        /// Use this method for all queries related to fetching of data.
        /// </summary>
        public DataTable GetDataRows(string query)
        {
            using var connection = this.GetConnection();
            if (connection == null) return null;
            using var command = this.GetSqlCommand(connection, query);
            if (command == null) return null;
            var da = new SqlDataAdapter(command);
            var dt = new DataTable();
            try
            {
                connection?.Open();
                da?.Fill(dt);
                connection?.Close();
            }
            catch (Exception ex)
            {
                // TODO need to figure out what comes here.
            }

            return dt;
        }

        /// <summary>
        /// Use this method for all Execute non query operations.
        /// </summary>
        public int ExecuteNonQuery(string query)
        {
            using var connection = this.GetConnection();
            if (connection == null) return -2;
            using var command = this.GetSqlCommand(connection, query);
            if (command == null) return -2;
            int result;
            try
            {
                connection.Open();
                result = command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                // TODO need to figure out what comes here.
                return -1;
            }

            return result;
        }
    }
}