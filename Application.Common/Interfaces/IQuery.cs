using MediatR;

namespace Application.Common.Interfaces
{
    public interface IQuery<out TResponse> : IRequest<TResponse>, IBaseQuery { }
    public interface IBaseQuery : IMessage { }
    public interface IMessage { }
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
    }

}
