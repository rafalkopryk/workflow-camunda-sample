using Camunda.Connector.SDK.Core.Api.Error;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using FluentAssertions;
using GatewayProtocol;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using static Camunda.Connector.SDK.Core.Impl.Constants;

namespace Camunda.Connector.SDK.Runtime.Tests.Inbound;


public class InboundCorrelationHandlerTest
{
    private readonly InboundCorrelationHandler _handler;
    private readonly Mock<Gateway.GatewayClient> _zeebeClient;

    const string EMPTY_JSON_KEYWORD = "{}";

    public InboundCorrelationHandlerTest()
    {
        var loggerMock = new Mock<ILogger<InboundCorrelationHandler>>();
        _zeebeClient = new Mock<Gateway.GatewayClient>();

        var publishMessageAsyncCallMock = TestCallsExtenisons.AsyncUnaryCall(Task.FromResult(new PublishMessageResponse()));
        _zeebeClient
            .Setup(x => x.PublishMessageAsync(It.IsAny<PublishMessageRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(publishMessageAsyncCallMock);

        var createProcessInstanceAsyncCallMock = TestCallsExtenisons.AsyncUnaryCall(Task.FromResult(new CreateProcessInstanceResponse()));
        _zeebeClient
            .Setup(x => x.CreateProcessInstanceAsync(It.IsAny<CreateProcessInstanceRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(createProcessInstanceAsyncCallMock);

        _handler = new InboundCorrelationHandler(loggerMock.Object, _zeebeClient.Object, new ConsJsonTransformerEngine());
    }

    [Fact]
    public async Task StartEvent_ShouldCallCorrectZeebeMethod()
    {
        //arrage
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = new StartEventCorrelationPoint("process1", default, default),
            BpmnProcessId = "process1",

        };

        //act
        await _handler.Correlate(properties, new { });

        //assert
        _zeebeClient.Verify(x => x.CreateProcessInstanceAsync(
            It.Is<CreateProcessInstanceRequest>(
                x => x.BpmnProcessId == properties.BpmnProcessId),
            null,
            null,
            CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Message_ShouldCallCorrectZeebeMethod), MemberType = typeof(TestDataGenerator))]
    public async Task Message_ShouldCallCorrectZeebeMethod(object veriables, string correlationKey, string correlationKeyValue)
    {
        //arrage
        var point = new MessageCorrelationPoint("msg1");

        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey
            }
        };

        //act
        await _handler.Correlate(properties, veriables);

        //assert
        _zeebeClient.Verify(x => x.PublishMessageAsync(
            It.Is<PublishMessageRequest>(
                x => x.Name == point.MessageName &&
                x.CorrelationKey == correlationKeyValue),
            null,
            null,
            CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Message_IncorrectCorrelationKey_ShouldThrowConnectorException()
    {
        //arrage
        var correlationKey = "=correlationKey";
        var veriables = new
        {
            TestKey = "Testvalue"
        };

        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey
            }
        };

        //act
        var action = async () => await _handler.Correlate(properties, veriables);

        //assert
        await action.Should().ThrowAsync<ConnectorException>();
    }

    [Fact]
    public async Task Message_NoResultVar_IncorrectResultExprProvided_ShouldThrowConnectorException()
    {
        //arrage
        var correlationKey = "=correlationKey";
        var veriables = new
        {
            CorrelationKey = correlationKey,
            TestKey = "Testvalue"
        };

        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey,
                [RESULT_EXPRESSION_KEYWORD] = "={ otherKeyAlias: otherKey}"
            }
        };

        //act
        var action = async () => await _handler.Correlate(properties, veriables);

        //assert
        await action.Should().ThrowAsync<ConnectorException>();
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Message_NoResultVar_NoResultExpr_ShouldNotCopyVariables), MemberType = typeof(TestDataGenerator))]
    public async Task Message_NoResultVar_NoResultExpr_ShouldNotCopyVariables(object veriables, string correlationKey, string correlationKeyValue)
    {
        //arrage
        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey
            }
        };

        //act
        await _handler.Correlate(properties, veriables);

        //assert
        _zeebeClient.Verify(x => x.PublishMessageAsync(
            It.Is<PublishMessageRequest>(
                x => x.Name == point.MessageName &&
                x.CorrelationKey == correlationKeyValue &&
                x.Variables == EMPTY_JSON_KEYWORD),
            null,
            null,
            CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Message_ResultVarProvided_NoResultExpr_ShouldCopyAllVarsToResultVar), MemberType = typeof(TestDataGenerator))]
    public async Task Message_ResultVarProvided_NoResultExpr_ShouldCopyAllVarsToResultVar(object veriables, string correlationKey, string resultVariableValue, string correlationKeyValue, string expectedVariables)
    {
        //arrage
        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey,
                [RESULT_VARIABLE_KEYWORD] = resultVariableValue
            }
        };

        //act
        await _handler.Correlate(properties, veriables);

        //assert
        _zeebeClient.Verify(x => x.PublishMessageAsync(
            It.Is<PublishMessageRequest>(
                x => x.Name == point.MessageName &&
                x.CorrelationKey == correlationKeyValue &&
                IsJsonEquals(x.Variables, expectedVariables)),
            null,
            null,
            CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Message_NoResultVar_ResultExprProvided_ShouldExtractVariables), MemberType = typeof(TestDataGenerator))]
    public async Task Message_NoResultVar_ResultExprProvided_ShouldExtractVariables(object veriables, string correlationKey, string resultExpresionValue, string correlationKeyValue, string expectedVariables)
    {
        //arrage
        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey,
                [RESULT_EXPRESSION_KEYWORD] = resultExpresionValue
            }
        };

        //act
        await _handler.Correlate(properties, veriables);

        //assert
        _zeebeClient.Verify(x => x.PublishMessageAsync(
            It.Is<PublishMessageRequest>(
                x => x.Name == point.MessageName &&
                x.CorrelationKey == correlationKeyValue &&
                IsJsonEquals(x.Variables, expectedVariables)),
            null,
            null,
            CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Message_ResultVarProvided_ResultExprProvided_ShouldExtractVarsAndCopyAllVarsToResultVar), MemberType = typeof(TestDataGenerator))]
    public async Task Message_ResultVarProvided_ResultExprProvided_ShouldExtractVarsAndCopyAllVarsToResultVar(object veriables, string correlationKey, string resultVariableValue, string resultExpresionValue, string correlationKeyValue, string expectedVariables)
    {
        //arrage
        var point = new MessageCorrelationPoint("msg1");
        var properties = new InboundConnectorProperties
        {
            CorrelationPoint = point,
            BpmnProcessId = "process1",
            Properties = new Dictionary<string, string>
            {
                [CORRELATION_KEY_EXPRESSION_KEYWORD] = correlationKey,
                [RESULT_VARIABLE_KEYWORD] = resultVariableValue,
                [RESULT_EXPRESSION_KEYWORD] = resultExpresionValue
            }
        };

        //act
        await _handler.Correlate(properties, veriables);

        //assert
        _zeebeClient.Verify(x => x.PublishMessageAsync(
            It.Is<PublishMessageRequest>(
                x => x.Name == point.MessageName &&
                x.CorrelationKey == correlationKeyValue &&
                IsJsonEquals(x.Variables, expectedVariables)),
            null, 
            null,
            CancellationToken.None),
            Times.Once);
    }

    private static bool IsJsonEquals(string left, string right)
    {
        var jdp = new JsonDiffPatch();
        var patch = jdp.Diff(left, right);
        return patch is null;
    } 

}

public class TestDataGenerator
{
    private const string CORRELATION_KEY_VALUE = "someTestCorrelationKeyValue";

    public static IEnumerable<object[]> Message_ShouldCallCorrectZeebeMethod()
    {
        yield return new object[]
        {
            JsonSerializer.Deserialize<object>($$"""
            {
                "correlationKey": "{{CORRELATION_KEY_VALUE}}"
            }
            """)!,
            "=correlationKey",
            CORRELATION_KEY_VALUE
        };

        yield return new object[]
        {
            JsonSerializer.Deserialize<object>($$"""
            {
                "result":{
                    "correlationKey": "{{CORRELATION_KEY_VALUE}}"
                }
            }
            """)!,
            "=result.correlationKey",
            CORRELATION_KEY_VALUE
        };
    }

    public static IEnumerable<object[]> Message_NoResultVar_NoResultExpr_ShouldNotCopyVariables()
    {
        var veriables = $$"""
            {
                "correlationKey": "{{CORRELATION_KEY_VALUE}}",
                "testKey": "testValue"
            }
            """;
        
        yield return new object[]
        {
            JsonSerializer.Deserialize<object>(veriables)!,
            "=correlationKey",
            CORRELATION_KEY_VALUE
        };
    }

    public static IEnumerable<object[]> Message_ResultVarProvided_NoResultExpr_ShouldCopyAllVarsToResultVar()
    {
        var veriables = $$"""
            {
                "correlationKey": "{{CORRELATION_KEY_VALUE}}",
                "testKey": "testValue"
            }
            """;

        yield return new object[]
        {
            JsonSerializer.Deserialize<object>(veriables)!,
            "=correlationKey",
            "resultVar",
            CORRELATION_KEY_VALUE,
            $$"""
            {
                "resultVar":{{veriables}}
            }
            """
        };
    }

    public static IEnumerable<object[]> Message_NoResultVar_ResultExprProvided_ShouldExtractVariables()
    {
        var veriables = $$"""
            {
                "correlationKey": "{{CORRELATION_KEY_VALUE}}",
                "testKey": "testValue",
                "otherKey": "otherValue"
            }
            """;

        yield return new object[]
        {
            JsonSerializer.Deserialize<object>(veriables)!,
            "=correlationKey",
            "={ otherKeyAlias: otherKey}",
            CORRELATION_KEY_VALUE,
            """
            {
                "otherKeyAlias": "otherValue"
            }
            """
        };

        yield return new object[]
        {
            JsonSerializer.Deserialize<object>(veriables)!,
            "=correlationKey",
            "={ otherKeyAlias: otherKey, correlationKey: correlationKey}",
            CORRELATION_KEY_VALUE,
            $$"""
            {
                "otherKeyAlias": "otherValue",
                "correlationKey": "{{CORRELATION_KEY_VALUE}}"
            }
            """
        };
    }

    public static IEnumerable<object[]> Message_ResultVarProvided_ResultExprProvided_ShouldExtractVarsAndCopyAllVarsToResultVar()
    {
        var veriables = $$"""
            {
                "correlationKey": "{{CORRELATION_KEY_VALUE}}",
                "testKey": "testValue",
                "otherKey": "otherValue"
            }
            """;

        yield return new object[]
        {
            JsonSerializer.Deserialize<object>(veriables)!,
            "=correlationKey",
            "resultVar",
            "={ otherKeyAlias: otherKey}",
            CORRELATION_KEY_VALUE,
            $$"""
            {
                "resultVar":{{veriables}},
                "otherKeyAlias": "otherValue"
            }
            """
        };
    }
}