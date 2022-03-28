using System.Runtime.CompilerServices;

namespace CX.LoggingLib
{
    /// <summary>
    /// Logging level for messages
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// Interface for logging. Implement <see cref="ILogbook.Write(string, LogLevel, Exception?, object?, string?, string)"/> with
    /// your chosen logging framework to log messages. For default abstract base class, see <see cref="Logbook"/>.
    /// </summary>
    public interface ILogbook
    {
        /// <summary>
        /// Create a typed version of this <see cref="ILogbook"/>. A typed version provides assigned type as extra information when
        /// writing messages.
        /// </summary>
        /// <typeparam name="T">Type of the containing class</typeparam>
        /// <returns></returns>
        public TypedLogbook<T> CreateTyped<T>();

        /// <summary>
        /// Log message and info.
        /// </summary>
        /// <param name="message">Message to write into log.</param>
        /// <param name="level">Logging level for the message.</param>
        /// <param name="exception">Optional exception in case of errors.</param>
        /// <param name="customContent">Custom extra info.</param>
        /// <param name="callerName">Name of the calling class.</param>
        /// <param name="callerMemberName">Name of the calling method.</param>
        public void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent);
    }

    /// <summary>
    /// Default abstract base class for <see cref="ILogbook"/>.
    /// </summary>
    public abstract class Logbook : ILogbook
    {
        /// <summary>
        /// Create a typed <see cref="ILogbook"/> with given type as container. <see cref="TypedLogbook{T}"/> provides
        /// the name of <typeparamref name="T"/> as extra information when logging.
        /// </summary>
        /// <typeparam name="T">Type of the containing class.</typeparam>
        /// <returns></returns>
        public virtual TypedLogbook<T> CreateTyped<T>()
        {
            return new TypedLogbook<T>(this);
        }
        public abstract void Write(string message, LogLevel level, Exception? exception = null,
            string? callerName = "", [CallerMemberName] string callerMemberName = "", params object[]? customContent);
    }

    /// <summary>
    /// An <see cref="ILogbook"/> with info on the containing class.
    /// </summary>
    /// <typeparam name="T">Containing class type.</typeparam>
    public class TypedLogbook<T>
    {
        private readonly ILogbook logbook;

        /// <summary>
        /// Create a logger with information on the containing class.
        /// </summary>
        /// <param name="logbook"><see cref="ILogbook"/> to create the typed version from.</param>
        public TypedLogbook(ILogbook logbook)
        {
            this.logbook = logbook;
        }

        /// <summary>
        /// Call <see cref="ILogbook.Write(string, LogLevel, Exception?, string?, string, object[]?)"/> with given
        /// arguments and the name of <typeparamref name="T"/> as callerName./>
        /// </summary>
        /// <param name="message">Message to write into log.</param>
        /// <param name="level">Logging level for the message.</param>
        /// <param name="exception">Optional exception in case of errors.</param>
        /// <param name="callerMemberName">Name of the calling method.</param>
        /// <param name="customContent">Custom info for logging</param>
        public virtual void Write(string message, LogLevel level, Exception? exception = null,
            [CallerMemberName] string callerMemberName = "", params object[]? customContent)
        {
            logbook.Write(message, level, exception, typeof(T).FullName, callerMemberName, customContent);
        }

        /// <summary>
        /// Get the underlyin <see cref="ILogbook"/> instance
        /// </summary>
        public ILogbook BaseLogbook
        {
            get => logbook;
        }
    }
}