namespace CustomStorage.Web.React
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EPiServer.Data;
    using EPiServer.Data.Dynamic;
    using EPiServer.Forms.Core.Data;
    using EPiServer.Forms.Core.Models;
    using EPiServer.ServiceLocation;
    using CustomStorage.Web.React.SubmissionStorage;

    /// <summary>
    /// Custom Submission Storage.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IPermanentStorage))]
    public class MsSqlPermanentStorage : PermanentStorage
    {
        private readonly ISubmissionStorageHelper _submissionStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlPermanentStorage"/> class.
        /// Constructor.
        /// </summary>
        public MsSqlPermanentStorage(ISubmissionStorageHelper submissionStorageHelper)
        {
            _submissionStorageHelper = submissionStorageHelper ?? throw new ArgumentNullException(nameof(submissionStorageHelper));
        }

        /// <summary>
        /// Load list of submissions.
        /// </summary>
        public override IEnumerable<PropertyBag> LoadSubmissionFromStorage(FormIdentity formIden, string[] submissionIds)
        {
            var submissions = _submissionStorageHelper.GetTableDataById(formIden.GetCustomStorageTableName(), submissionIds);

            return submissions?.Select(s =>
            {
                var bag = new PropertyBag
                {
                    Id = Identity.NewIdentity(s?.SubmissionId ?? Guid.Empty)
                };
                bag?.Add(s?.Data);
                return bag;
            });
        }

        /// <summary>
        /// Delete a submission from storage.
        /// </summary>
        public override void Delete(FormIdentity formIden, string submissionId)
        {
            _submissionStorageHelper.DeleteById(formIden?.GetCustomStorageTableName(), submissionId);
        }

        /// <summary>
        /// Save new submission into storage.
        /// </summary>
        public override Guid SaveToStorage(FormIdentity formIden, Submission submission)
        {
            var result = _submissionStorageHelper.InsertData(formIden?.GetCustomStorageTableName(), new SubmissionData
            {
                Data = submission?.Data,
            });
            return result;
        }

        /// <summary>
        /// Update a submission associated with a Guid.
        /// </summary>
        public override Guid UpdateToStorage(Guid formSubmissionId, FormIdentity formIden, Submission submission)
        {
            var result = _submissionStorageHelper.UpdateData(formSubmissionId, formIden?.GetCustomStorageTableName(), new SubmissionData
            {
                Data = submission?.Data,
            });
            return result;
        }

        /// <summary>
        /// Load submissions in a date time range.
        /// </summary>
        public override IEnumerable<PropertyBag> LoadSubmissionFromStorage(FormIdentity formIden, DateTime beginDate, DateTime endDate, bool finalizedOnly = false)
        {
            var submissions = _submissionStorageHelper.GetFilteredTableData(formIden?.GetCustomStorageTableName(), beginDate, endDate);

            return submissions?.Select(s =>
            {
                var bag = new PropertyBag
                {
                    Id = Identity.NewIdentity(s?.SubmissionId ?? Guid.Empty)
                };
                bag?.Add(s?.Data);
                return bag;
            });
        }
    }
}