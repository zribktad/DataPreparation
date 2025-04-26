using System;
using NUnit.Framework;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace DataPreparation.UnitTests.Helpers
{
    [TestFixture]
    public class LoggerHelperTests
    {
    
      
      
        [Test]
        public void Log_WithNullLogger_SkipsLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            bool actionCalled = false;
            Action<ILogger> logAction = logger => { actionCalled = true; };

            // Act - Call with one null and one valid logger
            LoggerHelper.Log(logAction, null, mockLogger.Object);

            // Assert
            Assert.That(actionCalled, Is.True);
            mockLogger.Verify(l => l.Log(
                It.IsAny<LogLevel>(), 
                It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Never);
        }

        [Test]
        public void Log_WithValidLogger_InvokesAction()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            bool actionCalled = false;
            Action<ILogger> logAction = logger => { actionCalled = true; };

            // Act
            LoggerHelper.Log(logAction, mockLogger.Object);

            // Assert
            Assert.That(actionCalled, Is.True);
        }

        [Test]
        public void Log_WithMultipleLoggers_InvokesActionOnEach()
        {
            // Arrange
            var mockLogger1 = new Mock<ILogger>();
            var mockLogger2 = new Mock<ILogger>();
            var mockLogger3 = new Mock<ILogger>();
            int callCount = 0;
            
            Action<ILogger> logAction = logger => { callCount++; };

            // Act
            LoggerHelper.Log(logAction, mockLogger1.Object, mockLogger2.Object, mockLogger3.Object);

            // Assert
            Assert.That(callCount, Is.EqualTo(3));
        }
    }
}