using BenchmarkDotNet.Running;

//var demo = new JsonTransformerBenchmark();
//var res1 = demo.Transform_ConsJson_LargeMessageExpression();
//var res2 = demo.Transform_JmesPath_LargeMessageExpression();
//Console.ReadLine();

var result = BenchmarkRunner.Run<InboundCorrelationHandlerBenchmark>();
//var result = BenchmarkRunner.Run<JsonTransformerBenchmark>();
