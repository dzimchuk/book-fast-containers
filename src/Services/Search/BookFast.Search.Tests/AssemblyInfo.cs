using BookFast.Search.Tests;

[assembly: AssemblyFixture(typeof(SearchApiFixture))]

// Keeps the behavior of a shared per-assembly collection where all test classes ran serially
// See https://xunit.net/docs/running-tests-in-parallel#parallel-modes
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
