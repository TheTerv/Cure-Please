using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace CurePlease.Infrastructure.Tests
{
    [TestFixture]
    public class ProcessManagerTests
    {
        private Mock<IProcessUtilities> _MockProcessUtilities;
        private ProcessManager _ProcessManager;

        [SetUp]
        public void SetUp()
        {
            _MockProcessUtilities = new Mock<IProcessUtilities>();
            _ProcessManager = new ProcessManager(_MockProcessUtilities.Object);
        }

        [Test]
        public void CheckForDLLFiles_returnsEmpty_whenNoProcessesFoundNull()
        {
            List<Process> processes = null;
            _MockProcessUtilities.Setup(p => p.GetRunningFFXIProcesses()).Returns(processes);
            
            var results = _ProcessManager.CheckForDLLFiles( out string errorMessage);

            Assert.IsEmpty(results);
            Assert.AreEqual(ProcessManager.ERROR_NO_FFXI_PROCESSES_FOUND, errorMessage);
        }

        [Test]
        public void CheckForDLLFiles_returnsEmpty_whenNoProcessesFound()
        {
            List<Process> processes = new();
            _MockProcessUtilities.Setup(p => p.GetRunningFFXIProcesses()).Returns(processes);

            var results = _ProcessManager.CheckForDLLFiles(out string errorMessage);

            Assert.IsEmpty(results);
            Assert.AreEqual(ProcessManager.ERROR_NO_FFXI_PROCESSES_FOUND, errorMessage);
        }

        [Test]
        public void CheckForDLLFiles_returnsEmpty_whenNoWrapperFound()
        {
            _MockProcessUtilities.Setup(p => p.GetRunningFFXIProcesses()).Returns(new[] { new Process() });
            _MockProcessUtilities.Setup(p => p.ProcessHasModuleWithName(It.IsAny<Process>(), It.IsAny<string>())).Returns(false);

            var results = _ProcessManager.CheckForDLLFiles(out string errorMessage);

            Assert.IsEmpty(results);
            Assert.AreEqual(ProcessManager.ERROR_NO_WRAPPERS_FOUND, errorMessage);
        }

        // System.Diagnostics.Process is a right bugger to try and fake!
        //[Test]
        public void CheckForDLLFiles_returnsTwoResults_whenWrapperHasWindowerAndAshita()
        {
            var mockProcess = new Mock<FakeProcess>();
            mockProcess.SetupGet(p => p.Id).Returns(1);
            mockProcess.SetupGet(p => p.MainWindowTitle).Returns("CharA");

            var mockProcess2 = new Mock<FakeProcess>();
            mockProcess2.SetupGet(p => p.Id).Returns(2);
            mockProcess2.SetupGet(p => p.MainWindowTitle).Returns("CharA");

            _MockProcessUtilities.Setup(p => p.GetRunningFFXIProcesses()).Returns(new[] { mockProcess.Object, mockProcess.Object });
            _MockProcessUtilities.SetupSequence(p => p.ProcessHasModuleWithName(It.IsAny<Process>(), "Ashita.dll")).Returns(true).Returns(false);
            _MockProcessUtilities.SetupSequence(p => p.ProcessHasModuleWithName(It.IsAny<Process>(), "\\Hook.dll")).Returns(false).Returns(true);

            var results = _ProcessManager.CheckForDLLFiles(out string errorMessage);

            Assert.IsEmpty(results);
            Assert.AreEqual(ProcessManager.ERROR_NO_WRAPPERS_FOUND, errorMessage);
        }
    }

    public class FakeProcess : Process
    {
        public new virtual int Id { get; }
        public new virtual string MainWindowTitle { get; }
    }
}
