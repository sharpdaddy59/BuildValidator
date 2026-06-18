namespace FailingProject;

// Intentionally broken: 'DoesNotExistType' is undefined, producing CS0246.
// This fixture is excluded from the test project's own compilation; it is only
// built indirectly by BuildValidator during the end-to-end tests.
public class Broken
{
    public DoesNotExistType Value { get; set; }
}
