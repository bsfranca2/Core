using MediatR;

namespace Bsfranca2.Core;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}