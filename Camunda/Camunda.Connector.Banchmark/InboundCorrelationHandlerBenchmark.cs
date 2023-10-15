using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using static Camunda.Connector.SDK.Core.Impl.Constants;

[MemoryDiagnoser(false)]
[SimpleJob(RuntimeMoniker.Net70, iterationCount: 20)]
public class InboundCorrelationHandlerBenchmark
{
    private InboundCorrelationHandler _inboundCorrelationHandlerConsJson;
    private InboundCorrelationHandler _inboundCorrelationHandlerJmesPath;

    private InboundConnectorProperties _propertiesCorelationKey;
    private InboundConnectorProperties _propertiesCorelationKeyAndResultExpression;

    private object _message;

    [GlobalSetup]
    public void Setup()
    {
        var loggerMock = Substitute.For<ILogger<InboundCorrelationHandler>>();

        var mockCall = TestCalls.AsyncUnaryCall(Task.FromResult(new PublishMessageResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var gatewayClientMock = Substitute.For<Gateway.GatewayClient>();
        gatewayClientMock.PublishMessageAsync(Arg.Any<PublishMessageRequest>(), null, null, Arg.Any<CancellationToken>())
            .Returns(mockCall);

        _inboundCorrelationHandlerConsJson = new InboundCorrelationHandler(loggerMock, gatewayClientMock, new ConsJsonTransformerEngine());
        _inboundCorrelationHandlerJmesPath = new InboundCorrelationHandler(loggerMock, gatewayClientMock, new JmesPathJsonTransformerEngine());
        _propertiesCorelationKey = new InboundConnectorProperties
        {
            BpmnProcessId = "process1",
            CorrelationPoint = new MessageCorrelationPoint("msg1"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "_id",
            }
        };

        _propertiesCorelationKeyAndResultExpression = new InboundConnectorProperties
        {
            BpmnProcessId = "process1",
            CorrelationPoint = new MessageCorrelationPoint("msg1"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "_id",
                [RESULT_EXPRESSION_KEYWORD] = "={  id: _id,  isActive: isActive,  child:{    id:child.id  },  balance: balance}",
            }
        };

        const string message = """
        {
            "_id": "643ad91eba0b4f4e37c0a46a",
            "index": 0,
            "guid": "9a276517-9439-4297-9fe7-72e2da86cd30",
            "isActive": false,
            "balance": "$1,506.23",
            "picture": "http://placehold.it/32x32",
            "age": 33,
            "eyeColor": "blue",
            "name": "ĄVincent Blackwell",
            "gender": "male",
            "company": "YURTURE",
            "email": "vincentblackwell@yurture.com",
            "phone": "+1 (885) 567-3380",
            "address": "109 Bryant Street, Caron, Alaska, 5187",
            "about": "Amet do eiusmod voluptate enim est adipisicing adipisicing et consectetur consequat veniam et dolor labore. Amet velit cillum commodo amet. Voluptate ipsum et tempor consectetur elit laborum proident anim occaecat veniam pariatur sit. Consectetur amet incididunt adipisicing anim. Veniam excepteur esse aliqua velit minim.\r\n",
            "registered": "2018-06-12T02:22:08 -02:00",
            "latitude": 51.879868,
            "longitude": 54.666041,
            "tags": [
                "Lorem",
                "do",
                "dolor",
                "est",
                "non",
                "exercitation",
                "consectetur"
            ],
            "friends": [
                {
                    "id": 0,
                    "name": "Abigail Curtis"
                },
                {
                    "id": 1,
                    "name": "Isabel Cleveland"
                },
                {
                    "id": 2,
                    "name": "Young Whitley"
                }
            ],
            "child":{
                "id":"643ad91eba0b4f4e37c0a46a"
            },
            "greeting": "Hello, Vincent Blackwell! You have 2 unread messages.",
            "favoriteFruit": "strawberry"        
        }
        """;

        _message = JsonSerializer.Deserialize<object>(message);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> Correlate_CorelationKeyProvided_NoResultExpr_ConsJson()
    {
        return await _inboundCorrelationHandlerConsJson.Correlate(_propertiesCorelationKey, _message);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> Correlate_CorelationKeyProvided_ResultExpryProvided_ConsJson()
    {
        return await _inboundCorrelationHandlerConsJson.Correlate(_propertiesCorelationKeyAndResultExpression, _message);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> Correlate_CorelationKeyProvided_NoResultExpr_JmesPath()
    {
        return await _inboundCorrelationHandlerJmesPath.Correlate(_propertiesCorelationKey, _message);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> Correlate_CorelationKeyProvided_ResultExpryProvided_JmesPath()
    {
        return await _inboundCorrelationHandlerJmesPath.Correlate(_propertiesCorelationKeyAndResultExpression, _message);
    }
}