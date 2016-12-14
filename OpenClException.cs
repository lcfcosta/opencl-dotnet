
#region Using Directives

using System;

#endregion

namespace OpenCl.DotNetCore
{
    /// <summary>
    /// Represents an exception, which is thrown when there is an OpenCL error.
    /// </summary>
    public class OpenClException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        public OpenClException() { }

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        /// <param name="result">The error code that was returned by OpenCL.</param>
        public OpenClException(Result result)
        {
            this.Result = result;
        }

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        /// <param name="message">An error message.</param>
        public OpenClException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="result">The error code that was returned by OpenCL.</param>
        public OpenClException(string message, Result result)
            : base(message)
        {
            this.Result = result;
        }

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="inner">The inner exception, which is the root cause for this exception.</param>
        public OpenClException(string message, Exception inner)
            : base(message, inner) { }

        /// <summary>
        /// Initializes a new <see cref="OpenClException"/> instance.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="inner">The inner exception, which is the root cause for this exception.</param>
        /// <param name="result">The error code that was returned by OpenCL.</param>
        public OpenClException(string message, Exception inner, Result result)
            : base(message, inner)
        {
            this.Result = result;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the error code that was returned by OpenCL.
        /// </summary>
        public Result Result { get; private set; }

        #endregion
    }
}