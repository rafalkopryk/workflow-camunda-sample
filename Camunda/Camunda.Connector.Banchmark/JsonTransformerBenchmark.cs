using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Camunda.Connector.SDK.Runtime.Util.Feel;

[MemoryDiagnoser(false)]
[SimpleJob(RuntimeMoniker.Net70, iterationCount:20)]
public class JsonTransformerBenchmark
{
    private const string MESSAGE = """
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

    private const string SMALL_MESSAGE_EXPRESSION = @"
    {  
        id: _id,
        childId:child.id,
        balance: balance
    }";

    private const string LARGE_MESSAGE_EXPRESSION = @"
    {  
        id: _id,
        childId:child.id,
        balance: balance,
        name: name,
        gender: gender,
        company: company,
        email: email,
        phone: phone,
        location:{
          latitude: latitude,
          longitude: longitude
        }
    }";

    private static readonly IJsonTransformerEngine s_ConsJsonTransformerEngine = new ConsJsonTransformerEngine();
    private static readonly IJsonTransformerEngine s_JmesPathJsonTransformerEngine = new JmesPathJsonTransformerEngine();

    [Benchmark]
    public string? Transform_JmesPath_SmallMessageExpression()
    {
        return s_JmesPathJsonTransformerEngine.Transform(SMALL_MESSAGE_EXPRESSION, MESSAGE);
    }

    [Benchmark]
    public string? Transform_ConsJson_SmallMessageExpression()
    {
        return s_ConsJsonTransformerEngine.Transform(SMALL_MESSAGE_EXPRESSION, MESSAGE);

    }

    [Benchmark]
    public string? Transform_JmesPath_LargeMessageExpression()
    {
        return s_JmesPathJsonTransformerEngine.Transform(LARGE_MESSAGE_EXPRESSION, MESSAGE);
    }

    [Benchmark]
    public string? Transform_ConsJson_LargeMessageExpression()
    {
        return s_ConsJsonTransformerEngine.Transform(LARGE_MESSAGE_EXPRESSION, MESSAGE);
    }
}
