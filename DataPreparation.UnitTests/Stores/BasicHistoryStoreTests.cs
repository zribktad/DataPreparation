using System.Collections.Generic;
using NUnit.Framework;
using DataPreparation.Models.Data;

namespace DataPreparation.UnitTests.Stores
{
    [TestFixture]
    public class BasicHistoryStoreTests
    {
        private HistoryStore<string> _historyStore = null!;

        [SetUp]
        public void Setup()
        {
            _historyStore = new HistoryStore<string>();
        }

        [Test]
        public void TryPush_WithUniqueId_ReturnsTrue()
        {
            // Act
            var result = _historyStore.TryPush(1, "Test Item");
            
            // Assert
            Assert.That(result, Is.True);
            Assert.That(_historyStore.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryPush_WithDuplicateId_ReturnsFalse()
        {
            // Arrange
            _historyStore.TryPush(1, "First Item");
            
            // Act
            var result = _historyStore.TryPush(1, "Duplicate Item");
            
            // Assert
            Assert.That(result, Is.False);
            Assert.That(_historyStore.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetById_WithExistingId_ReturnsItem()
        {
            // Arrange
            const string testItem = "Test Item";
            _historyStore.TryPush(1, testItem);
            
            // Act
            var result = _historyStore.GetById(1);
            
            // Assert
            Assert.That(result, Is.EqualTo(testItem));
        }
    }
}