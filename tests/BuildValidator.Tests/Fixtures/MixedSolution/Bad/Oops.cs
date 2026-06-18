namespace Bad;

// Intentionally broken: 'MissingType' is undefined, producing CS0246. Excluded
// from the test assembly's own compilation; built only by BuildValidator's
// end-to-end solution test.
public class Oops
{
    public MissingType Value { get; set; }
}
