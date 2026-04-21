namespace Shared.Models;

public abstract record ExecutionPlanResult
{
    public sealed record Success(
        IReadOnlyList<Execution> Executions)
        : ExecutionPlanResult;

    public sealed record Failure(
        string Reason) 
        : ExecutionPlanResult;
}
