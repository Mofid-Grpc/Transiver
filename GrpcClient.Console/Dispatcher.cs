using BenchmarkDotNet.Attributes;
using Grpc.Core;
using Grpc.Net.Client;
using Replication;

namespace GrpcClient.Console;

public class Dispatcher
{
    private const int MAX_BUFFER_SIZE = 1024 * 1024 * 1024;
    private const int SIZE = 10_000;

    [Benchmark]
    public async Task DispatchGroup()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5165", new GrpcChannelOptions
        {
            MaxReceiveMessageSize = MAX_BUFFER_SIZE,
            MaxSendMessageSize = MAX_BUFFER_SIZE,
        });

        var client = new Replicator.ReplicatorClient(channel);

        var request = new ReplicateRequest();
        request.ReplicaItems.Clear();

        for (int i = 0; i < SIZE; i++)
        {
            request.ReplicaItems.Add(new ReplicaItem
            {
                Title = $"item {i}",
                State = "Ready",
            });
        }

        var response = await client.ReplicateGroupAsync(request);
    }

    [Benchmark]
    public async Task DispatchSingle()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5165");
        var client = new Replicator.ReplicatorClient(channel);

        var tasks = new List<AsyncUnaryCall<ReplicaItem>>();

        for (int i = 0; i < SIZE; i++)
        {
            var item = new ReplicaItem
            {
                Title = $"item {i}",
                State = "Ready",
            };

            var task = client.ReplicateSingleAsync(item);
            tasks.Add(task);
        }

        _ = await Task.WhenAll(tasks.Select(t => t.ResponseAsync));
    }
}
