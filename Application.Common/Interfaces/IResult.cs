using Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
