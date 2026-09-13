using Xunit.Sdk;
using Xunit.v3;

// ProxysqlAdminUiWebApplicationFactory has to publish its temp directory through the process-wide
// APP_DB_PATH environment variable, because SqliteDbExtensions.GetAppDbPath() reads that variable
// directly instead of going through IConfiguration. A process-wide variable is shared state: if
// xunit.v3 ran two test classes in parallel, a second factory's constructor (or a first factory's
// Dispose, which restores the original value) could change APP_DB_PATH out from under a host that
// is still booting, and the two suites would silently share - or lose - an identity database.
//
// Disabling parallelisation assembly-wide is the cheapest correct fix here: this assembly holds a
// single in-process web host whose startup dominates the run time, so there is nothing meaningful
// to parallelise. Revisit this (a collection fixture, or moving APP_DB_PATH into IConfiguration)
// only if the suite grows test classes that boot no host.
//
// CollectionBehaviour(DisableTestParallelization = true) is the xunit.v2 spelling and is an
// obsolete-as-error in xunit.v3 4.0.0; ParallelMode.None is its replacement.
[assembly: Parallelization(Mode = ParallelMode.None)]
