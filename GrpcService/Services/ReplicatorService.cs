using Grpc.Core;
using Replication;

namespace GrpcService.Services;

public class ReplicatorService : Replicator.ReplicatorBase
{
    private readonly ILogger<ReplicatorService> _logger;
    public ReplicatorService(ILogger<ReplicatorService> logger)
    {
        _logger = logger;
    }

    public override Task<ReplicateResponse> ReplicateGroup(ReplicateRequest request, ServerCallContext context)
    {
        var result = new ReplicateResponse();
        result.ReplicaItems.Clear();

        foreach (var replicaItem in request.ReplicaItems)
        {
            replicaItem.State = "Done";
            result.ReplicaItems.Add(replicaItem);
        }

        return Task.FromResult(result);
    }

    public override Task<ReplicaItem> ReplicateSingle(ReplicaItem request, ServerCallContext context)
    {
        request.State = "Done";
        return Task.FromResult(request);
    }
}