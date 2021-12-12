using BenchmarkDotNet.Running;
using GrpcClient.Console;

_ = BenchmarkRunner.Run<Dispatcher>();
