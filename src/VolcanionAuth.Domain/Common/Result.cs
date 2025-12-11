namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Represents the outcome of an operation, indicating success or failure and providing an error message if applicable.
/// </summary>
/// <remarks>Use the static methods to create instances representing either a successful or failed result. The
/// <see cref="Result"/> type is commonly used to encapsulate the result of operations that may fail, providing a
/// consistent way to handle errors without using exceptions for control flow.</remarks>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents a failure state.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message associated with the current operation or state.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Initializes a new instance of the Result class with the specified success state and error message.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the result represents a successful operation. Set to <see langword="true"/> for
    /// success; otherwise, <see langword="false"/>.</param>
    /// <param name="error">The error message associated with a failed result. Must be <see langword="null"/> or empty if <paramref
    /// name="isSuccess"/> is <see langword="true"/>; otherwise, must be a non-empty string.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="isSuccess"/> is <see langword="true"/> and <paramref name="error"/> is not <see
    /// langword="null"/> or empty, or if <paramref name="isSuccess"/> is <see langword="false"/> and <paramref
    /// name="error"/> is <see langword="null"/> or empty.</exception>
    protected Result(bool isSuccess, string error)
    {
        // Validate the consistency of the success state and error message
        if (isSuccess && !string.IsNullOrEmpty(error))
        {
            // A successful result should not have an error message
            throw new InvalidOperationException("A successful result cannot have an error.");
        }
        // A failed result must have an error message
        if (!isSuccess && string.IsNullOrEmpty(error))
        {
            // An error message is required for a failed result
            throw new InvalidOperationException("A failed result must have an error.");
        }
        // Assign properties
        IsSuccess = isSuccess;
        // Assign the error message
        Error = error;
    }

    /// <summary>
    /// Creates a new successful result with no error message.
    /// </summary>
    /// <returns>A <see cref="Result"/> instance that represents a successful operation.</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result"/> instance representing a failure, containing the provided error message.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the result.</typeparam>
    /// <param name="value">The value to include in the successful result. Can be null for reference types.</param>
    /// <returns>A Result<T> instance representing a successful operation with the specified value.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the value that would be returned on success.</typeparam>
    /// <param name="error">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    /// <returns>A result representing a failure, containing the specified error message.</returns>
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value and indicates whether the operation was successful.
/// </summary>
/// <remarks>Use this type to encapsulate both the outcome of an operation and its return value. The result
/// includes information about success or failure, as well as an error message if applicable.</remarks>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value contained by the current instance.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the Result class with the specified value, success state, and error message.
    /// </summary>
    /// <param name="value">The value associated with the result. This parameter is used when the operation is successful.</param>
    /// <param name="isSuccess">A value indicating whether the operation was successful. Set to <see langword="true"/> if the operation
    /// succeeded; otherwise, <see langword="false"/>.</param>
    /// <param name="error">The error message describing the failure. This parameter should be null or empty if <paramref name="isSuccess"/>
    /// is <see langword="true"/>.</param>
    protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        // Assign the value
        Value = value;
    }
}
