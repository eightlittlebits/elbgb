using System;
using System.Collections.Generic;

namespace elbgb_test
{
    partial class ResultTable
    {
        private string _manifestName;
        private List<Test> _testResults;
        private TimeSpan _duration;

        public ResultTable(string manifestname, List<Test> testResults, TimeSpan duration)
        {
            _manifestName = manifestname;
            _testResults = testResults;
            _duration = duration;
        }
    }
}
