namespace Church.BibleStudyFellowship.Models.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Church.BibleStudyFellowship.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class Repository : IRepository
    {
        private readonly CloudTable studyTable;

        private readonly CloudTable lessonTable;

        private readonly CloudTable feedbackTable;

        private readonly CloudTable bibleBookTable;

        private readonly CloudTable bibleVerseTable;

        internal Repository(
            CloudTable studyTable,
            CloudTable lessonTable,
            CloudTable feedbackTable,
            CloudTable bibleBookTable,
            CloudTable bibleVerseTable)
        {
            this.studyTable = studyTable;
            this.lessonTable = lessonTable;
            this.feedbackTable = feedbackTable;
            this.bibleBookTable = bibleBookTable;
            this.bibleVerseTable = bibleVerseTable;
        }

        public static Repository Create(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var studyTable = Repository.CreateTableIfNotExist(tableClient, Constants.StudyTable);
            var lessonTable = Repository.CreateTableIfNotExist(tableClient, Constants.LessonTable);
            var feedbackTable = Repository.CreateTableIfNotExist(tableClient, Constants.FeedbackTable);
            var bibleBookTable = Repository.CreateTableIfNotExist(tableClient, Constants.BibleBookTable);
            var bibleVerseTable = Repository.CreateTableIfNotExist(tableClient, Constants.BibleVerseTable);

            return new Repository(studyTable, lessonTable, feedbackTable, bibleBookTable, bibleVerseTable);
        }

        public Task UpsertStudyAsync(Study study)
        {
            var insertOperation = TableOperation.InsertOrReplace(study.ToStorage());
            return this.studyTable.ExecuteAsync(insertOperation);
        }

        public IEnumerable<Study> GetStudies(string culture)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, culture);
            var query = new TableQuery().Where(filter);
            return Repository.ExecuteQueryAsync(this.studyTable, query, ModelExtensions.ToStudy).Result;
        }

        public Study GetStudy(string culture, string title)
        {
            var retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(culture, title);
            var tableResult = this.studyTable.ExecuteAsync(retrieveOperation).Result;
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(tableResult.Result != null, $"Could not find the study on ({culture},{title})");
            return ((DynamicTableEntity)tableResult.Result).ToStudy();
        }

        public Task UpsertLessonAsync(Lesson lesson)
        {
            var insertOperation = TableOperation.InsertOrReplace(lesson.ToStorage());
            return this.lessonTable.ExecuteAsync(insertOperation);
        }

        public IEnumerable<Lesson> GetLessons(string culture)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, culture);
            var query = new TableQuery().Where(filter);
            return Repository.ExecuteQueryAsync(this.lessonTable, query, ModelExtensions.ToLesson).Result;
        }

        public Lesson GetLesson(string culture, string id)
        {
            var retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(culture, id);
            var tableResult = this.lessonTable.ExecuteAsync(retrieveOperation).Result;
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
            var query = new TableQuery();
            return Repository.ExecuteQueryAsync(this.feedbackTable, query, ModelExtensions.ToFeedback).Result;
        }

        public Task AddBibleBooksAsync(IEnumerable<BibleBook> books)
        {
            var batchOperations = new TableBatchOperation();
            foreach (var book in books)
            {
                batchOperations.Insert(book.ToStorage());
            }

            return this.bibleBookTable.ExecuteBatchAsync(batchOperations);
        }

        public IEnumerable<BibleBook> GetBibleBooks(string culture)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, culture);
            var query = new TableQuery().Where(filter);
            return Repository.ExecuteQueryAsync(this.bibleBookTable, query, ModelExtensions.ToBibleBook).Result;

        }

        private static CloudTable CreateTableIfNotExist(CloudTableClient client, string name)
        {
            var table = client.GetTableReference(name);
            table.CreateIfNotExistsAsync().Wait();

            return table;
        }

        private static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(CloudTable cloudTable, TableQuery query, Func<DynamicTableEntity, T> convert)
        {
            var token = new TableContinuationToken();
            var opt = new TableRequestOptions();
            var ctx = new OperationContext() { ClientRequestID = "ID" };
            var cancelToken = new CancellationToken();
            var items = new List<T>();
            while (!cancelToken.IsCancellationRequested && token != null)
            {
                var segment = await cloudTable.ExecuteQuerySegmentedAsync(query, token, opt, ctx, cancelToken);

                // Run the method
                items.AddRange(segment.Select(result => convert(result)));
                token = segment.ContinuationToken;
            }

            return items.AsReadOnly();
        }
    }
}
