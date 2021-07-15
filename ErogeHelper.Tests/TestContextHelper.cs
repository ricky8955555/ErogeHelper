using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests
{
    // https://stackoverflow.com/questions/9021881/how-can-we-run-a-test-method-with-multiple-parameters-in-mstest/37249060#37249060
    class TestContextHelper<T> : IEnumerable<T>
    {
        public TestContext TestContext { get; }
        public TestContextHelper(TestContext testContext) => TestContext = testContext;

        public List<T> Instances { get; } = new();

        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator() => Instances.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Instances.GetEnumerator();

        #endregion
    }
}
