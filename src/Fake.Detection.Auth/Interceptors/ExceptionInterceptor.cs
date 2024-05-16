using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fake.Detection.Auth.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await Handle(() => continuation(request, responseStream, context));
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        await Handle(() => continuation(requestStream, responseStream, context));
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        return Handle(() => continuation(request, context));
    }

    private static async Task Handle(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw HandlerInternalError(exception);
        }
    }

    private static async Task<TResult> Handle<TResult>(Func<Task<TResult>> func)
    {
        try
        {
            return await func();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw HandlerInternalError(exception);
        }
    }

    private static RpcException HandlerInternalError(Exception exception)
    {
        return new RpcException(new Status(StatusCode.Internal, exception.Message));
    }

}