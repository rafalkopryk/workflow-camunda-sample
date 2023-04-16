using Grpc.Core;
using Grpc.Core.Testing;

namespace Camunda.Connector.SDK.Runtime.Tests;

public static class TestCallsExtenisons
{
    public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TResponse>(
            Task<TResponse> responseAsync)
    {
        return TestCalls.AsyncUnaryCall(responseAsync, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
    }
}
