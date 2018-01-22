namespace Church.BibleStudyFellowship.Models.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Church.BibleStudyFellowship.Models;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    internal static class ModelExtensions
    {
        public static DynamicTableEntity ToStorage(this Study study)
        {
            var entry = new DynamicTableEntity
            {
                PartitionKey = study.Culture,
                RowKey = study.Title,
            };
            entry.Properties["Lessons"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(study.Lessons));

            return entry;
        }

        public static DynamicTableEntity ToStorage(this Lesson lesson)
        {
            var entry = new DynamicTableEntity
            {
                PartitionKey = lesson.Culture,
                RowKey = lesson.Id,
            };
            entry.Properties["Name"] = EntityProperty.CreateEntityPropertyFromObject(lesson.Name);
            entry.Properties["AudioUrl"] = EntityProperty.CreateEntityPropertyFromObject(lesson.Audio);
            entry.Properties["MemoryVerse"] = EntityProperty.CreateEntityPropertyFromObject(lesson.MemoryVerse);
            entry.Properties["One"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[0]));
            entry.Properties["Two"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[1]));
            entry.Properties["Three"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[2]));
            entry.Properties["Four"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[3]));
            entry.Properties["Five"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[4]));
            entry.Properties["Six"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[5]));

            return entry;
        }

        public static DynamicTableEntity ToStorage(this Feedback feedback)
        {
            var entry = new DynamicTableEntity
            {
                PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                RowKey = Guid.NewGuid().ToString(),
            };
            entry.Properties["Comment"] = EntityProperty.CreateEntityPropertyFromObject(feedback.Comment);

            return entry;
        }

        public static DynamicTableEntity ToStorage(this BibleBook book)
        {
            var entry = new DynamicTableEntity
            {
                PartitionKey = book.Culture,
                RowKey = book.Order.ToString(),
            };
            entry.Properties["Name"] = EntityProperty.CreateEntityPropertyFromObject(book.Name);
            entry.Properties["Shorthand"] = EntityProperty.CreateEntityPropertyFromObject(book.Shorthand);

            return entry;
        }

        public static Study ToStudy(this DynamicTableEntity entry)
        {
            return new Study
            {
                Culture = entry.PartitionKey,
                Title = entry.RowKey,
                Lessons = JsonConvert.DeserializeObject<IList<LessonItem>>(entry.Properties["Lessons"].StringValue),
            };
        }

        public static Lesson ToLesson(this DynamicTableEntity entry)
        {
            var days = new[] {
                JsonConvert.DeserializeObject<Day>(entry.Properties["One"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry.Properties["Two"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry.Properties["Three"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry.Properties["Four"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry.Properties["Five"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry.Properties["Six"].StringValue),
            };

            return new Lesson
            {
                Culture = entry.PartitionKey,
                Id = entry.RowKey,
                Name = entry.Properties["Name"].StringValue,
                ////AudioUrl = entry["AudioUrl"].StringValue,
                MemoryVerse = entry.Properties["MemoryVerse"].StringValue,
                DayQuestions = days.ToList(),
            };
        }

        public static Feedback ToFeedback(this DynamicTableEntity entry)
        {
            return new Feedback
            {
                TimeStamp = entry.Timestamp.UtcDateTime,
                Comment = entry.Properties["Comment"].StringValue,
            };
        }

        public static BibleBook ToBibleBook(this DynamicTableEntity entry)
        {
            return new BibleBook
            {
                Culture = entry.PartitionKey,
                Order = int.Parse(entry.RowKey),
                Name = entry.Properties["Name"].StringValue,
                Shorthand = entry.Properties["Shorthand"].StringValue,
            };
        }
    }
}
