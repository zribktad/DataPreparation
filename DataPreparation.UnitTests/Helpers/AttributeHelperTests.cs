using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using DataPreparation.Helpers;

namespace DataPreparation.UnitTests.Helpers
{
    [TestFixture]
    public class AttributeHelperTests
    {
        [Test]
        public void GetAttributes_WithSingleAttributeType_ReturnsAllAttributesOfThatType()
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithMultipleAttributes));
            
            // Act
            var attributes = AttributeHelper.GetAttributes(methodInfo, typeof(TestAttribute));
            
            // Assert
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(2));
            Assert.That(attributes.All(a => a is TestAttribute), Is.True);
        }

        [Test]
        public void GetAttributes_WithMultipleAttributeTypes_ReturnsAllAttributesOfAllTypes()
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithMultipleAttributes));
            
            // Act
            var attributes = AttributeHelper.GetAttributes(methodInfo, typeof(TestAttribute), typeof(AnotherTestAttribute));
            
            // Assert
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(3));
            Assert.That(attributes.Count(a => a is TestAttribute), Is.EqualTo(2));
            Assert.That(attributes.Count(a => a is AnotherTestAttribute), Is.EqualTo(1));
        }

        [Test]
        public void GetAttributes_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutAttributes));
            
            // Act
            var attributes = AttributeHelper.GetAttributes(methodInfo, typeof(TestAttribute));
            
            // Assert
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAttributes_WhenSearchingForNonexistentAttributeType_ReturnsEmptyList()
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithMultipleAttributes));
            
            // Act
            var attributes = AttributeHelper.GetAttributes(methodInfo, typeof(NonexistentAttribute));
            
            // Assert
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAttributes_WithSingleAttribute_ReturnsOnlyThatAttribute()
        {
            // Arrange
            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithSingleAttribute));
            
            // Act
            var attributes = AttributeHelper.GetAttributes(methodInfo, typeof(TestAttribute));
            
            // Assert
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(1));
            Assert.That(attributes[0], Is.InstanceOf<TestAttribute>());
        }

        #region Test Classes and Attributes

        private class TestClass
        {
            [TestAttribute("First")]
            [TestAttribute("Second")]
            [AnotherTestAttribute]
            public void MethodWithMultipleAttributes() { }

            public void MethodWithoutAttributes() { }

            [TestAttribute]
            public void MethodWithSingleAttribute() { }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private class TestAttribute : Attribute
        {
            public string Value { get; }

            public TestAttribute(string value = "Default")
            {
                Value = value;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class AnotherTestAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method)]
        private class NonexistentAttribute : Attribute { }

        #endregion
    }
}