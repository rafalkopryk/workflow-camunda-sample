using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using static Camunda.Connector.SDK.Core.Impl.Constants;


//var demo = new Demo();
//demo.Setup();
//var res1 = demo.CheckLargeMasageAndRootCorelationKeyAndResultExpression();
//Console.ReadLine();

var result = BenchmarkRunner.Run<Demo>();

[MemoryDiagnoser(false)]
[SimpleJob(RuntimeMoniker.Net70)]
public class Demo
{
    private InboundCorrelationHandler _inboundCorrelationHandler;
    private InboundConnectorProperties _propertiesRootCorelationKey;
    private InboundConnectorProperties _propertiesChildCorelationKey;
    private InboundConnectorProperties _propertiesRootCorelationKeyResultExpression;
    private InboundConnectorProperties _propertiesChildCorelationKeyResultExpression;


    private object _smallMessage;
    private object _largeMessage;

    [GlobalSetup]
    public void Setup()
    {
        var loggerMock = new Mock<ILogger<InboundCorrelationHandler>>();
        var mockCall = TestCalls.AsyncUnaryCall(Task.FromResult(new PublishMessageResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var gatewayClientMock = new Mock<Gateway.GatewayClient>();
        gatewayClientMock
            .Setup(x => x.PublishMessageAsync(It.IsAny<PublishMessageRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(mockCall);

        _inboundCorrelationHandler = new InboundCorrelationHandler(loggerMock.Object, gatewayClientMock.Object);
        _propertiesRootCorelationKey = new InboundConnectorProperties
        {
            BpmnProcessId = "test",
            CorrelationPoint = new MessageCorrelationPoint(
                        "test"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "_id",
            }
        };

        _propertiesChildCorelationKey = _propertiesRootCorelationKey with
        {
            CorrelationPoint = new MessageCorrelationPoint(
                        "test"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "=child.id",
            }
        };

        _propertiesRootCorelationKeyResultExpression = new InboundConnectorProperties
        {
            BpmnProcessId = "test",
            CorrelationPoint = new MessageCorrelationPoint(
                "test"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "_id",
                [RESULT_EXPRESSION_KEYWORD] = "={  id: _id,  isActive: isActive,  child:{    id:child.id  },  balance: balance}",

            }
        };

        _propertiesChildCorelationKeyResultExpression = _propertiesRootCorelationKey with
        {
            CorrelationPoint = new MessageCorrelationPoint(
                        "test"),
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = "=child.id",
                [RESULT_EXPRESSION_KEYWORD] = "={  id: _id,  isActive: isActive,  child:{    id:child.id  },  balance: balance}",
            }
        };

        var smallMessage = """
        {
          "_id": "643ad91eba0b4f4e37c0a46a",
          "index": 0,
          "guid": "9a276517-9439-4297-9fe7-72e2da86cd30",
          "isActive": false,
          "child":{
            "id":"643ad91eba0b4f4e37c0a46a"
          },
          "balance": "$1,506.23"
        }
        """;

        _smallMessage = JsonSerializer.Deserialize<object>(smallMessage);

        var largeMessage = """
        {
            "_id": "643ad91eba0b4f4e37c0a46a",
            "index": 0,
            "guid": "9a276517-9439-4297-9fe7-72e2da86cd30",
            "isActive": false,
            "balance": "$1,506.23",
            "picture": "http://placehold.it/32x32",
            "age": 33,
            "eyeColor": "blue",
            "name": "Vincent Blackwell",
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
                },
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
                },
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
                },
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
                },
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
                },
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
                },
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
                },
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

        _largeMessage = JsonSerializer.Deserialize<object>(largeMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckSmallMasageAndRootCorelationKey()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesRootCorelationKey, _smallMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckLargeMasageAndRootCorelationKey()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesRootCorelationKey, _largeMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckSmallMasageAndChildCorelationKey()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesChildCorelationKey, _smallMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckLargeMasageAndChildCorelationKey()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesChildCorelationKey, _largeMessage);
    }



    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckSmallMasageAndRootCorelationKeyAndResultExpression()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesRootCorelationKeyResultExpression, _smallMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckLargeMasageAndRootCorelationKeyAndResultExpression()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesRootCorelationKeyResultExpression, _largeMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckSmallMasageAndChildCorelationKeyAndResultExpression()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesChildCorelationKeyResultExpression, _smallMessage);
    }

    [Benchmark]
    public async Task<IInboundConnectorResult<IResponseData>> CheckLargeMasageAndChildCorelationKeyAndResultExpression()
    {
        return await _inboundCorrelationHandler.Correlate(_propertiesChildCorelationKeyResultExpression, _largeMessage);
    }
}