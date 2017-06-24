namespace Church.BibleStudyFellowship.Models
{
    using System;

    public static class ExceptionUtilities
    {
        public static void ThrowArgumentNullExceptionIfNull<T>(T item, string parameterName, string message = null)
        {
            if (item == null)
            {
                throw message == null ? new ArgumentNullException(parameterName) : new ArgumentNullException(parameterName, message);
            }
        }

        public static void ThowInvalidOperationExceptionIfFalse(bool predication, string message)
        {
            if (!predication)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
