using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = InclusivenessAnalyzer.Test.CSharpCodeFixVerifier<
    InclusivenessAnalyzer.InclusivenessAnalyzer,
    InclusivenessAnalyzer.InclusivenessAnalyzerCodeFixProvider>;

namespace InclusivenessAnalyzer.Test
{
    [TestClass]
    public class InclusivenessAnalyzerUnitTest
    {

        //No diagnostics expected to show up
        [TestMethod]
        public async Task EmptyTest()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ShortWordTest()
        {
            var test = @"class check {}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ShortWordHeTest()
        {
            var test = @"class he {}";

            var expected = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(1, 7).WithArguments("he", "they");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task WhitelistTypeNameTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class WhiteList
        {   
        }
    }";  

            var expected = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(11,15).WithArguments("WhiteList", "allow list, access list, permit");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task WhitelistMethodNameTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Program
        {   
            public void WhiteList() { }
        }
    }";

            var expected = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(13, 25).WithArguments("WhiteList", "allow list, access list, permit");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task WhitelistParameterTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Program
        {   
            public void DoWork(string whiteList) { }
        }
    }";

            var expected = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(13, 39).WithArguments("whiteList", "allow list, access list, permit");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task BlacklistPropertyTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Program
        {   
            public int BlacklistNumber { get; set; }
        }
    }";

            var expected1 = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(13, 24).WithArguments("BlacklistNumber", "deny list, blocklist, exclude list");
            var expected2 = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(13, 42).WithArguments("get_BlacklistNumber", "deny list, blocklist, exclude list");
            var expected3 = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(13, 47).WithArguments("set_BlacklistNumber", "deny list, blocklist, exclude list");
            await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2, expected3);
        }

        [TestMethod]
        public async Task BlacklistFieldTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Program
        {   
            public string BlackListValue;
        }
    }";

            var expected = VerifyCS.Diagnostic("Inclusive").WithLocation(13, 27).WithArguments("BlackListValue", "deny list, blocklist, exclude list");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task BlacklistCommentTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        /// <summary>
        /// blacklist here
        /// </summary>
        class Program
        {   
            public string ListValue;
        }
    }";

            var expected = VerifyCS.Diagnostic(InclusivenessAnalyzer.DiagnosticId).WithLocation(11, 12).WithArguments("blacklist", "deny list, blocklist, exclude list");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
