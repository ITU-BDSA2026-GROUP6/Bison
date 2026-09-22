using Xunit;

// E2E tests each spin up a real web service process that shares the same
// on-disk CSV storage , so running test classes in parallel causes cross-test data failure.

[assembly: CollectionBehavior(DisableTestParallelization = true)]
