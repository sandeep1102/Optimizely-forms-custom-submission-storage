namespace CustomStorage.Web.React.SubmissionStorage
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using EPiServer.Find.Helpers;
    using Newtonsoft.Json;
    using static System.String;

    /// <summary>
    /// Submission Storage Helper Interface.
    /// </summary>
    public class SubmissionStorageHelper : ISubmissionStorageHelper
    {
        private readonly ISqlHelper _sqlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionStorageHelper"/> class.
        /// </summary>
        public SubmissionStorageHelper(ISqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
        }

        private void CreateTable(string tableName)
        {
            if (tableName == null)
                return;

            string query =
                $@"CREATE TABLE {tableName}
                (
                    _id uniqueidentifier NOT NULL PRIMARY KEY,
                    data NVARCHAR(MAX) NOT NULL,
                    submissionDateTime datetime
                );
                CREATE INDEX optimizely_forms_{tableName}
                    ON {tableName}(_id, submissionDateTime);";
            _sqlHelper.ExecuteNonQuery(query);
        }

        /// <summary>
        /// Fetch data from table.
        /// </summary>
        public IEnumerable<SubmissionData> GetTableData(string tableName)
        {
            if (tableName == null)
                return null;
            string query = $@"SELECT * FROM {tableName};";
            return this.GetData(_sqlHelper.GetDataRows(query));
        }

        /// <summary>
        /// Check if table exist.
        /// </summary>
        public bool IsTableExist(string tableName)
        {
            if (tableName == null)
                return false;
            string query = $@"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL SELECT 1;";
            var dt = _sqlHelper.GetDataRows(query);
            return dt?.Rows.Count > 0 ? true : false;
        }

        /// <summary>
        /// Fetch table data by Id.
        /// </summary>
        public IEnumerable<SubmissionData> GetTableDataById(string tableName, string[] submissionIds)
        {
            if (tableName == null)
                return null;
            if (!IsTableExist(tableName))
            {
                this.CreateTable(tableName);
            }

            var ids = submissionIds?.Select(id => id?.Split(':')?.GetValue(id.Contains(':') ? 1 : 0));
            if (ids == null)
                return null;
            var query = $@"SELECT * FROM {tableName} WHERE _id in ('{Join("','", ids)}');";
            return this.GetData(_sqlHelper.GetDataRows(query));
        }

        /// <summary>
        /// Convert data table to custom type.
        /// </summary>
        private IEnumerable<SubmissionData> GetData(DataTable dataTable)
        {
            if (dataTable == null)
                return null;
            var result = new List<SubmissionData>();

            foreach (DataRow row in dataTable.Rows)
            {
                if (!Guid.TryParse(Convert.ToString(row["_id"]), out var id)) continue;
                var data = Convert.ToString(row["data"]);
                if (IsNullOrEmpty(data)) continue;

                var deserializedDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

                if (deserializedDictionary != null)
                {
                    result.Add(new SubmissionData
                    {
                        SubmissionId = id,
                        Data = deserializedDictionary,
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Delete record by Id.
        /// </summary>
        public int DeleteById(string tableName, string submissionId)
        {
            if (tableName == null)
                return 0;

            if (submissionId == null)
                return 0;

            string query =
                $@"DELETE from {tableName} Where _id = '{submissionId?.Split(':')?.GetValue(1)}';";
            return _sqlHelper.ExecuteNonQuery(query);
        }

        /// <summary>
        /// Save data to storage.
        /// </summary>
        public Guid InsertData(string tableName, SubmissionData submission)
        {
            if (tableName == null)
                return Guid.Empty;
            if (submission == null)
                return Guid.Empty;
            if (!IsTableExist(tableName))
            {
                this.CreateTable(tableName);
            }

            submission.SubmissionId = Guid.NewGuid();
            var submissionData = JsonConvert.SerializeObject(submission.Data);

            var query = $@"INSERT into {tableName} values ('{submission.SubmissionId}', '{submissionData}', GETDATE());";
            var result = _sqlHelper.ExecuteNonQuery(query);
            return result > 0 ? submission.SubmissionId : Guid.Empty;
        }

        /// <summary>
        /// Update data in storage.
        /// </summary>
        public Guid UpdateData(Guid formSubmissionId, string tableName, SubmissionData submission)
        {
            if (tableName == null)
                return Guid.Empty;
            if (submission == null)
                return Guid.Empty;
            if (formSubmissionId.IsNull())
                return Guid.Empty;

            var submissionData = JsonConvert.SerializeObject(submission.Data);

            var query = $@"UPDATE {tableName} Set data = '{submissionData}' Where _id = '{Convert.ToString(formSubmissionId)}';";
            var result = _sqlHelper.ExecuteNonQuery(query);
            return result > 0 ? submission.SubmissionId : Guid.Empty;
        }

        /// <summary>
        /// Fetch filtered data by begin and end date.
        /// </summary>
        public IEnumerable<SubmissionData> GetFilteredTableData(string tableName, DateTime beginDate, DateTime endDate)
        {
            if (tableName == null)
                return null;
            if (beginDate.IsNull())
                return null;
            if (endDate.IsNull())
                return null;
            if (!IsTableExist(tableName))
            {
                this.CreateTable(tableName);
            }

            string query = $@"SELECT * FROM {tableName} WHERE submissionDateTime between '{beginDate:MM/dd/yyyy HH:mm:ss}' and '{endDate:MM/dd/yyyy HH:mm:ss}';";
            return this.GetData(_sqlHelper.GetDataRows(query));
        }
    }
}