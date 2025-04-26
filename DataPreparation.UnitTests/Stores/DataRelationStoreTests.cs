using System;
using System.Reflection;
using NUnit.Framework;
using DataPreparation.Testing;

namespace DataPreparation.UnitTests.Stores
{
    [TestFixture]
    public class DataRelationStoreTests
    {
        [Test]
        public void GetClassDataPreparationType_WhenMappingExists_ReturnsCorrectType()
        {
            // Arrange
            var testClassType = typeof(TestClass);
            var dataPreparationType = typeof(TestClassDataPreparation);
            DataRelationStore.SetClassDataPreparationType(testClassType, dataPreparationType);
            
            // Act
            var result = DataRelationStore.GetClassDataPreparationType(testClassType);
            
            // Assert
            Assert.That(result, Is.EqualTo(dataPreparationType));
        }

        [Test]
        public void GetClassDataPreparationType_WhenNoMappingExists_ReturnsNull()
        {
            // Arrange
            var testClassType = typeof(UnregisteredClass);
            
            // Act
            var result = DataRelationStore.GetClassDataPreparationType(testClassType);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMethodDataPreparationType_WhenMappingExists_ReturnsCorrectType()
        {
            // Arrange
            var testMethod = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
            var dataPreparationType = typeof(TestMethodDataPreparation);
            DataRelationStore.SetMethodDataPreparationType(testMethod, dataPreparationType);
            
            // Act
            var result = DataRelationStore.GetMethodDataPreparationType(testMethod);
            
            // Assert
            Assert.That(result, Is.EqualTo(dataPreparationType));
        }

        [Test]
        public void GetMethodDataPreparationType_WhenNoMappingExists_ReturnsNull()
        {
            // Arrange
            var testMethod = typeof(UnregisteredClass).GetMethod(nameof(UnregisteredClass.UnregisteredMethod));
            
            // Act
            var result = DataRelationStore.GetMethodDataPreparationType(testMethod);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void HasMethodDataPreparationType_WhenMappingExists_ReturnsTrue()
        {
            // Arrange
            var testMethod = typeof(TestClass).GetMethod(nameof(TestClass.AnotherTestMethod));
            var dataPreparationType = typeof(AnotherTestMethodDataPreparation);
            DataRelationStore.SetMethodDataPreparationType(testMethod, dataPreparationType);
            
            // Act
            var result = DataRelationStore.HasMethodDataPreparationType(testMethod);
            
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasMethodDataPreparationType_WhenNoMappingExists_ReturnsFalse()
        {
            // Arrange
            var testMethod = typeof(UnregisteredClass).GetMethod(nameof(UnregisteredClass.AnotherUnregisteredMethod));
            
            // Act
            var result = DataRelationStore.HasMethodDataPreparationType(testMethod);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SetClassDataPreparationType_WhenCalledMultipleTimesForSameClass_UpdatesMapping()
        {
            // Arrange
            var testClassType = typeof(MultiUpdateClass);
            var initialDataPreparationType = typeof(InitialDataPreparation);
            var updatedDataPreparationType = typeof(UpdatedDataPreparation);
            
            // Act
            DataRelationStore.SetClassDataPreparationType(testClassType, initialDataPreparationType);
            var initialResult = DataRelationStore.GetClassDataPreparationType(testClassType);
            
            DataRelationStore.SetClassDataPreparationType(testClassType, updatedDataPreparationType);
            var updatedResult = DataRelationStore.GetClassDataPreparationType(testClassType);
            
            // Assert
            Assert.That(initialResult, Is.EqualTo(initialDataPreparationType));
            Assert.That(updatedResult, Is.EqualTo(updatedDataPreparationType));
        }

        [Test]
        public void SetMethodDataPreparationType_WhenCalledMultipleTimesForSameMethod_UpdatesMapping()
        {
            // Arrange
            var testMethod = typeof(MultiUpdateClass).GetMethod(nameof(MultiUpdateClass.MultiUpdateMethod));
            var initialDataPreparationType = typeof(InitialDataPreparation);
            var updatedDataPreparationType = typeof(UpdatedDataPreparation);
            
            // Act
            DataRelationStore.SetMethodDataPreparationType(testMethod, initialDataPreparationType);
            var initialResult = DataRelationStore.GetMethodDataPreparationType(testMethod);
            
            DataRelationStore.SetMethodDataPreparationType(testMethod, updatedDataPreparationType);
            var updatedResult = DataRelationStore.GetMethodDataPreparationType(testMethod);
            
            // Assert
            Assert.That(initialResult, Is.EqualTo(initialDataPreparationType));
            Assert.That(updatedResult, Is.EqualTo(updatedDataPreparationType));
        }

        #region Test Classes

        private class TestClass
        {
            public void TestMethod() { }
            public void AnotherTestMethod() { }
        }

        private class UnregisteredClass
        {
            public void UnregisteredMethod() { }
            public void AnotherUnregisteredMethod() { }
        }

        private class MultiUpdateClass
        {
            public void MultiUpdateMethod() { }
        }

        private class TestClassDataPreparation { }
        private class TestMethodDataPreparation { }
        private class AnotherTestMethodDataPreparation { }
        private class InitialDataPreparation { }
        private class UpdatedDataPreparation { }

        #endregion
    }
}