using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using DataPreparation.Models.Data;
using DataPreparation.Testing;

namespace DataPreparation.UnitTests.Stores
{
    [TestFixture]
    [NonParallelizable] // Add this attribute to prevent parallel execution
    public class StoreTests
    {
        private FixtureInfo _fixtureInfo = null!;
        private Mock<ILoggerFactory> _mockLoggerFactory = null!;
        private Mock<ILogger> _mockLogger = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;

        [SetUp]
        public void Setup()
        {
            // --- Mocking ITest and ITypeInfo with stable properties ---
            var mockTypeInfo = new Mock<NUnit.Framework.Interfaces.ITypeInfo>();
            mockTypeInfo.Setup(t => t.Type).Returns(GetType());
            mockTypeInfo.Setup(t => t.FullName).Returns(GetType().FullName ?? "DataPreparation.UnitTests.Stores.StoreTests");

            var mockTest = new Mock<NUnit.Framework.Interfaces.ITest>();
            mockTest.Setup(t => t.TypeInfo).Returns(mockTypeInfo.Object);
            // Use a stable mock ID, ClassName, MethodName, FullName
            mockTest.Setup(t => t.Id).Returns("StoreTests_FixtureMockID"); 
            mockTest.Setup(t => t.Arguments).Returns(Array.Empty<object?>()); // Stable arguments
            mockTest.Setup(t => t.ClassName).Returns(GetType().FullName ?? "DataPreparation.UnitTests.Stores.StoreTests");
            // MethodName might vary per test context, use a placeholder or leave null if ContextTestInfo handles it
            // mockTest.Setup(t => t.MethodName).Returns("MockTestMethodName"); 
            mockTest.Setup(t => t.FullName).Returns(GetType().FullName ?? "DataPreparation.UnitTests.Stores.StoreTests" + ".MockTest");

            _fixtureInfo = new FixtureInfo(mockTest.Object, this); // Create FixtureInfo with stable mock properties

            // --- Service Provider Mock ---
            _mockServiceProvider = new Mock<IServiceProvider>();

            // --- Clear Store (before setting up logger mock) ---
            try
            {
                var fixtureStores = Store.GetFixtureStores();
                foreach (var store in fixtureStores.ToList())
                {
                    if (store?.FixtureInfo != null)
                    {
                        Store.RemoveFixtureStore(store.FixtureInfo);
                    }
                }
                // Also try removing the _fixtureInfo we are about to use, just in case it lingered
                Store.RemoveFixtureStore(_fixtureInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error during Store cleanup in Setup: {ex.Message}");
                // Continue execution after warning
            }

            // --- Logger Factory Mock (setup after store cleanup) ---
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            // Mock the CreateLogger(string categoryName) method instead of the extension method
            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
        }

        [Test]
        public void CreateFixtureStore_WithValidArgs_CreatesAndAddsStore()
        {
            // Act
            var result = Store.CreateFixtureStore(_fixtureInfo, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            
            // Assert
            Assert.That(result, Is.True);
            var fixtureStores = Store.GetFixtureStores();
            Assert.That(fixtureStores, Has.Count.EqualTo(1));
            Assert.That(fixtureStores.First().FixtureInfo, Is.EqualTo(_fixtureInfo));
        }

        [Test]
        public void CreateFixtureStore_WhenStoreWithSameFixtureInfoExists_ReturnsFalse()
        {
            // Arrange
            Store.CreateFixtureStore(_fixtureInfo, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            
            // Act
            var result = Store.CreateFixtureStore(_fixtureInfo, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            
            // Assert
            Assert.That(result, Is.False);
            var fixtureStores = Store.GetFixtureStores();
            Assert.That(fixtureStores, Has.Count.EqualTo(1)); // Still only one store
        }

        [Test]
        public void GetFixtureStore_WithExistingFixtureInfo_ReturnsStore()
        {
            // Arrange
            Store.CreateFixtureStore(_fixtureInfo, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            
            // Act
            // Revert to retrieving using FixtureInfo
            var result = Store.GetFixtureStore(_fixtureInfo);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.FixtureInfo, Is.EqualTo(_fixtureInfo));
            Assert.That(result.LoggerFactory, Is.EqualTo(_mockLoggerFactory.Object));
        }

        [Test]
        public void RemoveFixtureStore_WithExistingFixtureInfo_RemovesAndReturnsTrue()
        {
            // Arrange
            Store.CreateFixtureStore(_fixtureInfo, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            
            // Act
            // Revert to removing using FixtureInfo
            var result = Store.RemoveFixtureStore(_fixtureInfo);
            
            // Assert
            Assert.That(result, Is.True);
            var fixtureStores = Store.GetFixtureStores();
            Assert.That(fixtureStores, Is.Empty);
        }

        [Test]
        public void RemoveFixtureStore_WithNonExistentFixtureInfo_ReturnsFalse()
        {
            // Arrange
            var mockTypeInfo = new Mock<ITypeInfo>();
            mockTypeInfo.Setup(t => t.Type).Returns(typeof(string)); // Different type
            mockTypeInfo.Setup(t => t.FullName).Returns("NonExistentTestFixtureForRemovalTest"); // Ensure unique name

            var mockTest = new Mock<ITest>();
            mockTest.Setup(t => t.TypeInfo).Returns(mockTypeInfo.Object);
            mockTest.Setup(t => t.Id).Returns("NonExistentID_ForRemovalTest"); // Ensure unique ID
            mockTest.Setup(t => t.Arguments).Returns(Array.Empty<object?>()); // Ensure stable arguments
            mockTest.Setup(t => t.ClassName).Returns("NonExistentClassName");
            mockTest.Setup(t => t.FullName).Returns("NonExistentFullName.Test");

            // Instantiate FixtureInfo directly, don't mock FixtureInfo itself
            var nonExistentFixtureInfo = new FixtureInfo(mockTest.Object, "test string instance");

            // Ensure the store doesn't contain this specific FixtureInfo before the call
            // The main Setup already clears the store, but explicitly remove just in case.
            Store.RemoveFixtureStore(nonExistentFixtureInfo);

            // Act
            var result = Store.RemoveFixtureStore(nonExistentFixtureInfo);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetFixtureStores_ReturnsAllCreatedStores()
        {
            // Arrange
            // Create distinct ITest mocks for each FixtureInfo
            var mockTypeInfo1 = new Mock<ITypeInfo>();
            mockTypeInfo1.Setup(t => t.Type).Returns(GetType());
            mockTypeInfo1.Setup(t => t.FullName).Returns(GetType().FullName + "_1");
            var mockTest1 = new Mock<ITest>();
            mockTest1.Setup(t => t.TypeInfo).Returns(mockTypeInfo1.Object);
            mockTest1.Setup(t => t.Id).Returns("GetStoresTest_ID1");
            mockTest1.Setup(t => t.Arguments).Returns(Array.Empty<object?>());
            mockTest1.Setup(t => t.ClassName).Returns(GetType().FullName ?? "");
            mockTest1.Setup(t => t.FullName).Returns(GetType().FullName + ".Test1");
            // Instantiate FixtureInfo directly
            var fixtureInfo1 = new FixtureInfo(mockTest1.Object, this);

            var mockTypeInfo2 = new Mock<ITypeInfo>();
            mockTypeInfo2.Setup(t => t.Type).Returns(typeof(string));
            mockTypeInfo2.Setup(t => t.FullName).Returns("System.String_Fixture2");
            var mockTest2 = new Mock<ITest>();
            mockTest2.Setup(t => t.TypeInfo).Returns(mockTypeInfo2.Object);
            mockTest2.Setup(t => t.Id).Returns("GetStoresTest_ID2");
            mockTest2.Setup(t => t.Arguments).Returns(Array.Empty<object?>());
            mockTest2.Setup(t => t.ClassName).Returns("System.String");
            mockTest2.Setup(t => t.FullName).Returns("System.String.Test2");
            // Instantiate FixtureInfo directly
            var fixtureInfo2 = new FixtureInfo(mockTest2.Object, "test instance 2");

            var mockTypeInfo3 = new Mock<ITypeInfo>();
            mockTypeInfo3.Setup(t => t.Type).Returns(typeof(int));
            mockTypeInfo3.Setup(t => t.FullName).Returns("System.Int32_Fixture3");
            var mockTest3 = new Mock<ITest>();
            mockTest3.Setup(t => t.TypeInfo).Returns(mockTypeInfo3.Object);
            mockTest3.Setup(t => t.Id).Returns("GetStoresTest_ID3");
            mockTest3.Setup(t => t.Arguments).Returns(Array.Empty<object?>());
            mockTest3.Setup(t => t.ClassName).Returns("System.Int32");
            mockTest3.Setup(t => t.FullName).Returns("System.Int32.Test3");
            // Instantiate FixtureInfo directly
            var fixtureInfo3 = new FixtureInfo(mockTest3.Object, 42);

            // Ensure store is clean before adding
            Store.RemoveFixtureStore(fixtureInfo1);
            Store.RemoveFixtureStore(fixtureInfo2);
            Store.RemoveFixtureStore(fixtureInfo3);

            // Add the stores
            Store.CreateFixtureStore(fixtureInfo1, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            Store.CreateFixtureStore(fixtureInfo2, _mockLoggerFactory.Object, _mockServiceProvider.Object);
            Store.CreateFixtureStore(fixtureInfo3, _mockLoggerFactory.Object, _mockServiceProvider.Object);

            // Act
            var fixtureStores = Store.GetFixtureStores();

            // Assert
            Assert.That(fixtureStores, Has.Count.EqualTo(3));
            // Check based on the unique FixtureInfo instances
            Assert.That(fixtureStores.Select(f => f.FixtureInfo), Contains.Item(fixtureInfo1));
            Assert.That(fixtureStores.Select(f => f.FixtureInfo), Contains.Item(fixtureInfo2));
            Assert.That(fixtureStores.Select(f => f.FixtureInfo), Contains.Item(fixtureInfo3));
        } // End of GetFixtureStores_ReturnsAllCreatedStores

        // Add a TearDown method to ensure cleanup after each test, complementing the Setup cleanup
        [TearDown]
        public void TearDown()
        {
             try
            {
                // Explicitly remove the FixtureInfo created in Setup for this test
                if (_fixtureInfo != null)
                {
                    Store.RemoveFixtureStore(_fixtureInfo);
                }

                // Attempt to clean up any other stores that might have been created (e.g., in GetFixtureStores test)
                var fixtureStores = Store.GetFixtureStores();
                foreach (var store in fixtureStores.ToList())
                {
                    // This is the correct location for this block
                    if (store?.FixtureInfo != null)
                    {
                        Store.RemoveFixtureStore(store.FixtureInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error during Store cleanup in TearDown: {ex.Message}");
            }
        }
    }
}