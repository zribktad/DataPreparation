using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using DataPreparation.Models;
using DataPreparation.Testing;

namespace DataPreparation.UnitTests.Stores
{
    [TestFixture]
    public class DataPreparationTestStoresTests
    {
        private Mock<ILoggerFactory> _mockLoggerFactory = null!;
        private Mock<ILogger> _mockLogger = null!;
        private DataPreparationTestStores _testStores = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            
            _testStores = new DataPreparationTestStores(_mockLoggerFactory.Object);
        }

        [Test]
        public void AddDataPreparation_SingleObject_AddsToPreparationList()
        {
            // Arrange
            var testData = new TestPreparationData();
            var upData = new object[] { "upParam" };
            var downData = new object[] { "downParam" };
            
            // Act
            _testStores.AddDataPreparation(testData, upData, downData);
            var result = _testStores.GetPreparation();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddDataPreparation_ListOfObjects_AddsAllToPreparationList()
        {
            // Arrange
            var dataList = new List<object?>
            {
                new TestPreparationData(),
                new TestPreparationData(),
                new TestPreparationData()
            };
            
            // Act
            _testStores.AddDataPreparation(dataList);
            var result = _testStores.GetPreparation();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddDataPreparation_WithNullItems_SkipsNullItems()
        {
            // Arrange
            var dataList = new List<object?>
            {
                new TestPreparationData(),
                null,
                new TestPreparationData()
            };
            
            // Act
            _testStores.AddDataPreparation(dataList);
            var result = _testStores.GetPreparation();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddDataPreparationList_WithMatchingArrays_AddsAllValidItems()
        {
            // Arrange
            var dataList = new List<object>
            {
                new TestPreparationData(),
                new TestPreparationData(),
                null
            };
            
            var upData = new[]
            {
                new object[] { "up1" },
                new object[] { "up2" },
                new object[] { "up3" }
            };
            
            var downData = new[]
            {
                new object[] { "down1" },
                new object[] { "down2" },
                new object[] { "down3" }
            };
            
            // Act
            _testStores.AddDataPreparationList(dataList, upData, downData);
            var result = _testStores.GetPreparation();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2)); // Null item should be skipped
        }

        [Test]
        public void PushProcessed_SingleItem_AddsToProcessedStack()
        {
            // Arrange
            var testData = new TestPreparationData();
            var upData = new object[] { "upParam" };
            var downData = new object[] { "downParam" };
            _testStores.AddDataPreparation(testData, upData, downData);
            var preparation = _testStores.GetPreparation().First();
            
            // Act
            _testStores.PushProcessed(preparation);
            
            // Assert
            Assert.That(_testStores.TryPopProcessed(out var data), Is.True);
            Assert.That(data, Is.SameAs(preparation));
        }

        [Test]
        public void TryPopProcessed_EmptyStack_ReturnsFalse()
        {
            // Act
            var result = _testStores.TryPopProcessed(out var data);
            
            // Assert
            Assert.That(result, Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void TryPopProcessed_WithItems_ReturnsItemsInLIFOOrder()
        {
            // Arrange
            var testData1 = new TestPreparationData();
            var testData2 = new TestPreparationData();
            var upData1 = new object[] { "up1" };
            var downData1 = new object[] { "down1" };
            var upData2 = new object[] { "up2" };
            var downData2 = new object[] { "down2" };
            
            _testStores.AddDataPreparation(testData1, upData1, downData1);
            _testStores.AddDataPreparation(testData2, upData2, downData2);
            
            var preparation1 = _testStores.GetPreparation()[0];
            var preparation2 = _testStores.GetPreparation()[1];
            
            _testStores.PushProcessed(preparation1);
            _testStores.PushProcessed(preparation2);
            
            // Act & Assert
            // Should pop in reverse order (LIFO)
            Assert.That(_testStores.TryPopProcessed(out var popped1), Is.True);
            Assert.That(popped1, Is.SameAs(preparation2));
            
            Assert.That(_testStores.TryPopProcessed(out var popped2), Is.True);
            Assert.That(popped2, Is.SameAs(preparation1));
            
            // Should now be empty
            Assert.That(_testStores.TryPopProcessed(out _), Is.False);
        }

        [Test]
        public void IsEmpty_WhenEmpty_ReturnsTrue()
        {
            // Act & Assert
            Assert.That(_testStores.IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_WithDataInPreparationList_ReturnsFalse()
        {
            // Arrange
            var testData = new TestPreparationData();
            _testStores.AddDataPreparation(new List<object?> { testData });
            
            // Act & Assert
            Assert.That(_testStores.IsEmpty(), Is.False);
        }

        [Test]
        public void IsEmpty_WithDataInProcessedStack_ReturnsFalse()
        {
            // Arrange
            var testData = new TestPreparationData();
            _testStores.AddDataPreparation(testData, new object[] { }, new object[] { });
            var preparation = _testStores.GetPreparation().First();
            _testStores.PushProcessed(preparation);
            
            // Clear the preparation list to ensure only processed items remain
            _testStores.GetPreparation().Clear();
            
            // Act & Assert
            Assert.That(_testStores.IsEmpty(), Is.False);
        }

        // Test class to use in our tests
        private class TestPreparationData { }
    }
}