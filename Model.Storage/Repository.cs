namespace Church.BibleStudyFellowship.Models.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Church.BibleStudyFellowship.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class Repository : IRepository
    {
        private readonly CloudTable studyTable;

        private readonly CloudTable lessonTable;

        private readonly CloudTable feedbackTable;

        internal Repository(CloudTable studyTable, CloudTable lessonTable, CloudTable feedbackTable)
        {
            this.studyTable = studyTable;
            this.lessonTable = lessonTable;
            this.feedbackTable = feedbackTable;
        }

        public static Repository Create(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var studyTable = Repository.CreateTableIfNotExist(tableClient, Constants.StudyTable);
            var lessonTable = Repository.CreateTableIfNotExist(tableClient, Constants.LessonTable);
            var feedbackTable = Repository.CreateTableIfNotExist(tableClient, Constants.FeedbackTable);

            return new Repository(studyTable, lessonTable, feedbackTable);
        }

        public Task AddStudyAsync(Study study)
        {
            var insertOperation = TableOperation.Insert(study.ToStorage());
            return this.studyTable.ExecuteAsync(insertOperation);
        }

        public IEnumerable<Study> GetStudies()
        {
            var query = new TableQuery<DynamicTableEntity>();
            return this.studyTable.ExecuteQuery(query).Select(item => item.ToStudy());
        }

        public Task AddLessonAsync(Lesson lesson)
        {
            var insertOperation = TableOperation.Insert(lesson.ToStorage());
            return this.lessonTable.ExecuteAsync(insertOperation);
        }

        public Lesson GetLesson(string culture, string id)
        {
            var retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(culture, id);
            var tableResult = this.lessonTable.Execute(retrieveOperation);
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(tableResult.Result != null, $"Could not find the lesson on ({culture},{id})");
            return ((DynamicTableEntity)tableResult.Result).ToLesson();
        }

        public Task AddFeedbackAsync(Feedback feedback)
        {
            var insertOperation = TableOperation.Insert(feedback.ToStorage());
            return this.feedbackTable.ExecuteAsync(insertOperation);
        }

        public IEnumerable<Feedback> GetFeedback()
        {
            var query = new TableQuery<DynamicTableEntity>();
            return this.feedbackTable.ExecuteQuery(query).Select(item => item.ToFeedback());
        }

        private static CloudTable CreateTableIfNotExist(CloudTableClient client, string name)
        {
            var table = client.GetTableReference(name);
            if(!table.Exists())
            {
                table.CreateIfNotExists();
            }

            return table;
        }
    }
}
