using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_test
{
    partial class ResultTable
    {
        private List<Test> _testResults;
        private TimeSpan _duration;

        public ResultTable(List<Test> testResults, TimeSpan duration)
        {
            _testResults = testResults;
            _duration = duration;
        }
    }
}
