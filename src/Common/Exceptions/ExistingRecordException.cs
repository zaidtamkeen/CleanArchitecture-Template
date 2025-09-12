using System;

namespace CleanTemplate.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create a record that already exists.
    /// Typically raised from repository implementations.
    /// </summary>
    public class ExistingRecordException : Exception
    {
        /// <summary>Creates a new instance of the exception.</summary>
        public ExistingRecordException()
            : base()
        {
        }

        /// <summary>Creates the exception with a custom message.</summary>
        public ExistingRecordException(string message)
            : base(message)
        {
        }

        /// <summary>Creates the exception with a message and inner exception.</summary>
        public ExistingRecordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Creates the exception based on the entity name and key.</summary>
        public ExistingRecordException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
