using System.Collections.Generic;
using NUnit.Framework;
using DataPreparation.Models.Data;

namespace DataPreparation.UnitTests.Stores
{
    [TestFixture]
    public class HistoryStoreTests
    {
        private HistoryStore<string> _historyStore;

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

        [Test]
        public void GetById_WithNonExistentId_ReturnsNull()
        {
            // Act
            var result = _historyStore.GetById(999);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryGetLatest_WithEmptyStore_ReturnsFalse()
        {
            // Act
            var result = _historyStore.TryGetLatest(out _, out _);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryGetLatest_WithItemsInStore_ReturnsLatestItem()
        {
            // Arrange
            _historyStore.TryPush(1, "First Item");
            _historyStore.TryPush(2, "Second Item");
            _historyStore.TryPush(3, "Latest Item");
            
            // Act
            var result = _historyStore.TryGetLatest(out var item, out var id);
            
            // Assert
            Assert.That(result, Is.True);
            Assert.That(item, Is.EqualTo("Latest Item"));
            Assert.That(id, Is.EqualTo(3));
        }

        [Test]
        public void TryGetLatest_WithRequestedCount_ReturnsMultipleItems()
        {
            // Arrange
            _historyStore.TryPush(1, "First Item");
            _historyStore.TryPush(2, "Second Item");
            _historyStore.TryPush(3, "Third Item");
            
            // Act
            var result = _historyStore.TryGetLatest(2, out var items, out var ids);
            
            // Assert
            Assert.That(result, Is.True);
            Assert.That(items, Has.Count.EqualTo(2));
            Assert.That(items[0], Is.EqualTo("Third Item"));
            Assert.That(items[1], Is.EqualTo("Second Item"));
            Assert.That(ids[0], Is.EqualTo(3));
            Assert.That(ids[1], Is.EqualTo(2));
        }

        [Test]
        public void TryPop_WithItemsInStore_RemovesAndReturnsLatestItem()
        {
            // Arrange
            _historyStore.TryPush(1, "First Item");
            _historyStore.TryPush(2, "Latest Item");
            
            // Act
            var result = _historyStore.TryPop(out var item);
            
            // Assert
            Assert.That(result, Is.True);
            Assert.That(item, Is.EqualTo("Latest Item"));
            Assert.That(_historyStore.Count, Is.EqualTo(1));
            
            // Verify the second item is now the latest
            _historyStore.TryGetLatest(out var newLatestItem, out _);
            Assert.That(newLatestItem, Is.EqualTo("First Item"));
        }

        [Test]
        public void TryPop_WithEmptyStore_ReturnsFalse()
        {
            // Act
            var result = _historyStore.TryPop(out _);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAll_WithItemsInStore_ReturnsAllItemsOrderedById()
        {
            // Arrange
            _historyStore.TryPush(3, "Third Item");
            _historyStore.TryPush(1, "First Item");
            _historyStore.TryPush(2, "Second Item");
            
            // Act
            var items = _historyStore.GetAll(out var ids);
            
            // Assert
            Assert.That(items, Has.Count.EqualTo(3));
            Assert.That(ids, Has.Count.EqualTo(3));
            
            // Verify items are ordered by ID
            Assert.That(ids[0], Is.EqualTo(1));
            Assert.That(ids[1], Is.EqualTo(2));
            Assert.That(ids[2], Is.EqualTo(3));
            Assert.That(items[0], Is.EqualTo("First Item"));
            Assert.That(items[1], Is.EqualTo("Second Item"));
            Assert.That(items[2], Is.EqualTo("Third Item"));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            _historyStore.TryPush(1, "Item 1");
            _historyStore.TryPush(2, "Item 2");
            _historyStore.TryPush(3, "Item 3");
            
            // Act
            _historyStore.Clear();
            
            // Assert
            Assert.That(_historyStore.Count, Is.EqualTo(0));
            Assert.That(_historyStore.TryGetLatest(out _, out _), Is.False);
            Assert.That(_historyStore.GetAll(out var ids), Is.Empty);
            Assert.That(ids, Is.Empty);
        }

        [Test]
        public void Enumeration_ReturnsAllItems()
        {
            // Arrange
            _historyStore.TryPush(1, "Item 1");
            _historyStore.TryPush(2, "Item 2");
            
            // Act
            var items = new List<string>();
            foreach (var item in _historyStore)
            {
                items.Add(item);
            }
            
            // Assert
            Assert.That(items, Has.Count.EqualTo(2));
            Assert.That(items, Contains.Item("Item 1"));
            Assert.That(items, Contains.Item("Item 2"));
        }
    }
}