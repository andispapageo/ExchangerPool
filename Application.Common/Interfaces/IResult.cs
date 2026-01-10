using Application.Common.DTOs;

namespace Application.Common.Interfaces
{
    public interface IResult
    {
        ResultStatus Status { get; }

        IEnumerable<string> Errors { get; }

        IEnumerable<ValidationError> ValidationErrors { get; }

        Type ValueType { get; }

        string Location { get; }

        object GetValue();
    }
}
