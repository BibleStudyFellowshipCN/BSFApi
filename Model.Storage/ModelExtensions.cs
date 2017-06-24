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
            entry["Name"] = EntityProperty.CreateEntityPropertyFromObject(lesson.Name);
            entry["AudioUrl"] = EntityProperty.CreateEntityPropertyFromObject(lesson.Audio);
            entry["MemoryVerse"] = EntityProperty.CreateEntityPropertyFromObject(lesson.MemoryVerse);
            entry["One"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[0]));
            entry["Two"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[1]));
            entry["Three"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[2]));
            entry["Four"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[3]));
            entry["Five"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[4]));
            entry["Six"] = EntityProperty.CreateEntityPropertyFromObject(JsonConvert.SerializeObject(lesson.DayQuestions[5]));

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
                Lessons = JsonConvert.DeserializeObject<IList<LessonItem>>(entry["Lessons"].StringValue),
            };
        }

        public static Lesson ToLesson(this DynamicTableEntity entry)
        {
            var days = new[] {
                JsonConvert.DeserializeObject<Day>(entry["One"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry["Two"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry["Three"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry["Four"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry["Five"].StringValue),
                JsonConvert.DeserializeObject<Day>(entry["Six"].StringValue),
            };

            return new Lesson
            {
                Culture = entry.PartitionKey,
                Id = entry.RowKey,
                Name = entry["Name"].StringValue,
                ////AudioUrl = entry["AudioUrl"].StringValue,
                MemoryVerse = entry["MemoryVerse"].StringValue,
                DayQuestions = days.ToList(),
            };
        }

        public static Feedback ToFeedback(this DynamicTableEntity entry)
        {
            return new Feedback
            {
                TimeStamp = entry.Timestamp.UtcDateTime,
                Comment = entry["Comment"].StringValue,
            };
        }

        public static BibleBook ToBibleBook(this DynamicTableEntity entry)
        {
            return new BibleBook
            {
                Culture = entry.PartitionKey,
                Order = int.Parse(entry.RowKey),
                Name = entry["Name"].StringValue,
                Shorthand = entry["Shorthand"].StringValue,
            };
        }
    }
}
