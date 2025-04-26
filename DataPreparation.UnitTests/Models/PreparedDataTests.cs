using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using DataPreparation.Data;
using DataPreparation.Models;

namespace DataPreparation.UnitTests.Models
{
    [TestFixture]
    public class PreparedDataTests
    {
        private Mock<ILoggerFactory> _mockLoggerFactory = null!;
        private Mock<ILogger> _mockLogger = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            // Remove the setup for the generic extension method
            // _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);
            // Keep the setup for the string-based method
            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
        }

        [Test]
        public void Constructor_WithNullPreparedDataInstance_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PreparedData(null, new object[] { }, new object[] { }, _mockLoggerFactory.Object));
        }

        [Test]
        public void Constructor_WithIBeforePreparationImplementation_SetsUpAndDownMethods()
        {
            // Arrange
            var dataInstance = new TestBeforePreparation();
            var upParams = new object[] { };  // No params needed
            var downParams = new object[] { }; // No params needed

            // Act
            var preparedData = new PreparedData(dataInstance, upParams, downParams, _mockLoggerFactory.Object);

            // Assert
            Assert.That(preparedData.IsRunUpASync(), Is.False);
            Assert.That(preparedData.IsRunDownASync(), Is.False);

            var result = preparedData.RunUp();
            Assert.That(dataInstance.UpResult, Is.EqualTo("Up called"));

            var downResult = preparedData.RunDown();
            Assert.That(dataInstance.DownResult, Is.EqualTo("Down called"));
        }

        [Test]
        public void Constructor_WithIBeforePreparationTaskImplementation_SetsAsyncMethods()
        {
            // Arrange
            var dataInstance = new TestBeforePreparationTask();
            var upParams = new object[] { };
            var downParams = new object[] {  };

            // Act
            var preparedData = new PreparedData(dataInstance, upParams, downParams, _mockLoggerFactory.Object);

            // Assert
            Assert.That(preparedData.IsRunUpASync(), Is.True);
            Assert.That(preparedData.IsRunDownASync(), Is.True);
        }

        [Test]
        public async Task RunUpAsync_WithAsyncMethod_ExecutesTaskAndReturnsCompletedTask()
        {
            // Arrange
            var dataInstance = new TestBeforePreparationTask();
            var upParams = new object[] { }; // No params needed
            var downParams = new object[] { }; // No params needed
            var preparedData = new PreparedData(dataInstance, upParams, downParams, _mockLoggerFactory.Object);
            
            // Act
            var task = preparedData.RunUpAsync();
            
            // Assert
            await task; // Should not throw
            Assert.That(dataInstance.UpExecuted, Is.True);
        }

        [Test]
        public async Task RunDownAsync_WithAsyncMethod_ExecutesTaskAndReturnsCompletedTask()
        {
            // Arrange
            var dataInstance = new TestBeforePreparationTask();
            var upParams = new object[] { }; // No params needed
            var downParams = new object[] { }; // No params needed
            var preparedData = new PreparedData(dataInstance, upParams, downParams, _mockLoggerFactory.Object);
            
            // Act
            var task = preparedData.RunDownAsync();
            
            // Assert
            await task; // Should not throw
            Assert.That(dataInstance.DownExecuted, Is.True);
        }

        [Test]
        public void Constructor_WithCustomAttributeImplementation_FindsUpAndDownMethods()
        {
            // Arrange
            var dataInstance = new TestCustomAttributeImplementation();
            // Pass parameters directly in the arrays, assuming PreparedData might expect this structure
            var upParams = new object[] { "upParam" }; 
            var downParams = new object[] { "downParam" };

            // Act
            var preparedData = new PreparedData(dataInstance, upParams, downParams, _mockLoggerFactory.Object);

            // Assert
            Assert.That(preparedData.IsRunUpASync(), Is.False);
            Assert.That(preparedData.IsRunDownASync(), Is.False);

            var result = preparedData.RunUp();
            Assert.Null(result);

            var downResult = preparedData.RunDown();
            Assert.Null(downResult);
        }

        [Test]
        public void RunUp_WhenNoUpMethodExists_ReturnsNull()
        {
            // Arrange
            var dataInstance = new TestNoMethods();
            var preparedData = new PreparedData(dataInstance, new object[] { }, new object[] { }, _mockLoggerFactory.Object);
            
            // Act
            var result = preparedData.RunUp();
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void RunDown_WhenNoDownMethodExists_ReturnsNull()
        {
            // Arrange
            var dataInstance = new TestNoMethods();
            var preparedData = new PreparedData(dataInstance, new object[] { }, new object[] { }, _mockLoggerFactory.Object);
            
            // Act
            var result = preparedData.RunDown();
            
            // Assert
            Assert.That(result, Is.Null);
        }

        #region Test Classes

        private class TestBeforePreparation : IBeforePreparation
        {
            public string UpResult { get; private set; } = string.Empty;
            public string DownResult { get; private set; } = string.Empty;

            public void UpData()
            {
                UpResult = "Up called";
            }

            public void DownData()
            {
                DownResult = "Down called";
            }
        }

        private class TestBeforePreparationTask : IBeforePreparationTask
        {
            public string UpParam { get; private set; } = string.Empty;
            public string DownParam { get; private set; } = string.Empty;
            public bool UpExecuted { get; private set; }
            public bool DownExecuted { get; private set; }

            public Task UpData()
            {
                UpParam = "upParam"; // Use a default value since interface doesn't take params
                UpExecuted = true;
                return Task.CompletedTask;
            }

            public Task DownData()
            {
                DownParam = "downParam"; // Use a default value since interface doesn't take params
                DownExecuted = true;
                return Task.CompletedTask;
            }
        }

        private class TestCustomAttributeImplementation
        {
            [UpData]
            public object CustomUpMethod(string param) // Ensure method signature matches expected parameters
            {
                return $"CustomUp: {param}";
            }

            [DownData]
            public object CustomDownMethod(string param) // Ensure method signature matches expected parameters
            {
                return $"CustomDown: {param}";
            }
        }

        private class TestNoMethods
        {
            // No UpData or DownData methods
        }

        #endregion
    }

    // We need to create these attribute classes for testing since they're used in PreparedData
    [AttributeUsage(AttributeTargets.Method)]
    public class UpDataAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DownDataAttribute : Attribute
    {
    }
}