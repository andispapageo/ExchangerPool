using Application.Common.Interfaces;
using System.Text.Json.Serialization;

namespace Application.Common.DTOs
{
    public class Result<T> : IResult
    {
        [JsonInclude]
        public T Value { get; init; }

        [JsonIgnore]
        public Type ValueType => typeof(T);

        [JsonInclude]
        public ResultStatus Status { get; protected set; }

        public bool IsSuccess
        {
            get
            {
                ResultStatus status = Status;
                if ((uint)status <= 1u || status == ResultStatus.NoContent)
                {
                    return true;
                }

                return false;
            }
        }

        [JsonInclude]
        public string SuccessMessage { get; protected set; } = string.Empty;


        [JsonInclude]
        public string CorrelationId { get; protected set; } = string.Empty;


        [JsonInclude]
        public string Location { get; protected set; } = string.Empty;


        [JsonInclude]
        public IEnumerable<string> Errors { get; protected set; } = Array.Empty<string>();


        [JsonInclude]
        public IEnumerable<ValidationError> ValidationErrors { get; protected set; } = Array.Empty<ValidationError>();


        protected Result()
        {
        }

        public Result(T value)
        {
            Value = value;
        }

        protected internal Result(T value, string successMessage)
            : this(value)
        {
            SuccessMessage = successMessage;
        }

        protected Result(ResultStatus status)
        {
            Status = status;
        }

        public static implicit operator T(Result<T> result)
        {
            return result.Value;
        }

        public static implicit operator Result<T>(T value)
        {
            return new Result<T>(value);
        }

        public static implicit operator Result<T>(Result result)
        {
            return new Result<T>(default(T))
            {
                Status = result.Status,
                Errors = result.Errors,
                SuccessMessage = result.SuccessMessage,
                CorrelationId = result.CorrelationId,
                ValidationErrors = result.ValidationErrors
            };
        }

        public object GetValue()
        {
            return Value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Success(T value, string successMessage)
        {
            return new Result<T>(value, successMessage);
        }

        public static Result<T> Created(T value)
        {
            return new Result<T>(ResultStatus.Created)
            {
                Value = value
            };
        }

        public static Result<T> Created(T value, string location)
        {
            return new Result<T>(ResultStatus.Created)
            {
                Value = value,
                Location = location
            };
        }

        public static Result<T> Error(string errorMessage)
        {
            Result<T> result = new Result<T>(ResultStatus.Error);
            result.Errors = new string[1] { errorMessage };
            return result;
        }

        public static Result<T> Invalid(params ValidationError[] validationErrors)
        {
            return new Result<T>(ResultStatus.Invalid)
            {
                ValidationErrors = new List<ValidationError>(validationErrors)
            };
        }

        public static Result<T> Invalid(IEnumerable<ValidationError> validationErrors)
        {
            return new Result<T>(ResultStatus.Invalid)
            {
                ValidationErrors = validationErrors
            };
        }

        public static Result<T> NotFound()
        {
            return new Result<T>(ResultStatus.NotFound);
        }

        public static Result<T> NotFound(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.NotFound)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> Forbidden()
        {
            return new Result<T>(ResultStatus.Forbidden);
        }

        public static Result<T> Forbidden(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.Forbidden)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> Unauthorized()
        {
            return new Result<T>(ResultStatus.Unauthorized);
        }

        public static Result<T> Unauthorized(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.Unauthorized)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> Conflict()
        {
            return new Result<T>(ResultStatus.Conflict);
        }

        public static Result<T> Conflict(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.Conflict)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> CriticalError(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.CriticalError)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> Unavailable(params string[] errorMessages)
        {
            return new Result<T>(ResultStatus.Unavailable)
            {
                Errors = errorMessages
            };
        }

        public static Result<T> NoContent()
        {
            return new Result<T>(ResultStatus.NoContent);
        }
    }

    public class Result : Result<Result>
    {
        public Result()
        {
        }

        protected internal Result(ResultStatus status)
            : base(status)
        {
        }

        public static Result Success()
        {
            return new Result();
        }

        public static Result SuccessWithMessage(string successMessage)
        {
            return new Result
            {
                SuccessMessage = successMessage
            };
        }

        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Success<T>(T value, string successMessage)
        {
            return new Result<T>(value, successMessage);
        }

        public static Result<T> Created<T>(T value)
        {
            return Result<T>.Created(value);
        }

        public static Result<T> Created<T>(T value, string location)
        {
            return Result<T>.Created(value, location);
        }


        public new static Result Error(string errorMessage)
        {
            Result result = new Result(ResultStatus.Error);
            result.Errors = new string[1] { errorMessage };
            return result;
        }


        public new static Result Invalid(params ValidationError[] validationErrors)
        {
            return new Result(ResultStatus.Invalid)
            {
                ValidationErrors = new List<ValidationError>(validationErrors)
            };
        }

        public new static Result Invalid(IEnumerable<ValidationError> validationErrors)
        {
            return new Result(ResultStatus.Invalid)
            {
                ValidationErrors = validationErrors
            };
        }

        public new static Result NotFound()
        {
            return new Result(ResultStatus.NotFound);
        }

        public new static Result NotFound(params string[] errorMessages)
        {
            return new Result(ResultStatus.NotFound)
            {
                Errors = errorMessages
            };
        }

        public new static Result Forbidden()
        {
            return new Result(ResultStatus.Forbidden);
        }

        public new static Result Forbidden(params string[] errorMessages)
        {
            return new Result(ResultStatus.Forbidden)
            {
                Errors = errorMessages
            };
        }

        public new static Result Unauthorized()
        {
            return new Result(ResultStatus.Unauthorized);
        }

        public new static Result Unauthorized(params string[] errorMessages)
        {
            return new Result(ResultStatus.Unauthorized)
            {
                Errors = errorMessages
            };
        }

        public new static Result Conflict()
        {
            return new Result(ResultStatus.Conflict);
        }

        public new static Result Conflict(params string[] errorMessages)
        {
            return new Result(ResultStatus.Conflict)
            {
                Errors = errorMessages
            };
        }

        public new static Result Unavailable(params string[] errorMessages)
        {
            return new Result(ResultStatus.Unavailable)
            {
                Errors = errorMessages
            };
        }

        public new static Result CriticalError(params string[] errorMessages)
        {
            return new Result(ResultStatus.CriticalError)
            {
                Errors = errorMessages
            };
        }

        public new static Result NoContent()
        {
            return new Result(ResultStatus.NoContent);
        }
    }
    public enum ResultStatus
    {
        Ok,
        Created,
        Error,
        Forbidden,
        Unauthorized,
        Invalid,
        NotFound,
        NoContent,
        Conflict,
        CriticalError,
        Unavailable
    }
}
