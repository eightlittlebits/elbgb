using System;

namespace elbgb_test
{
    [Flags]
    public enum TestStatus
    {
        None,
        Inconclusive = 1,
        Passing = 2,
        Failing = 4,
        All = Inconclusive | Passing | Failing,
    }
}
