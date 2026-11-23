using BookFast.PropertyManagement.Tests;

[assembly: AssemblyFixture(typeof(PropertyManagementApiFixture))]

// Keeps the behavior of a shared per-assembly collection where all test classes ran serially
// See https://xunit.net/docs/running-tests-in-parallel#parallel-modes
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
