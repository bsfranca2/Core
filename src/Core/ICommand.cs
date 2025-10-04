using MediatR;

namespace Bsfranca2.Core;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}