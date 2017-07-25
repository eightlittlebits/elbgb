using System;

namespace elbgb_test
{
    [Flags]
    public enum TestStatus
    {
        None,
        Inconclusive = 1,
        Failing = 2,
        Passing = 4,
        All = Inconclusive | Passing | Failing,
    }
}
