using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using DataPreparation.DataHandlers;
using DataPreparation.Testing;

namespace DataPreparation.UnitTests.DataHandlers
{
    [TestFixture]
    public class GetDataPreparationTests
    {
        [Test]
        public void FilterParams_WithClassDataPreparationOnly_ReturnsOnlyClassParams()
        {
            // Arrange
            var useClassDataPreparation = true;
            string methodName = null;
            var classParamsUp = new object[] { "test1", 123 };
            var methodParamsUp = new object[] { "test2", 456 };
            var classParamsDown = new object[] { "test3", 789 };
            var methodParamsDown = new object[] { "test4", 101112 };

            // Act
            var result = GetDataPreparation.FilterParams(
                useClassDataPreparation,
                methodName,
                classParamsUp,
                methodParamsUp,
                classParamsDown,
                methodParamsDown);

            // Assert
            Assert.That(result.Item1.Length, Is.EqualTo(1));
            Assert.That(result.Item2.Length, Is.EqualTo(1));
            Assert.That(result.Item1[0], Is.EqualTo(classParamsUp));
            Assert.That(result.Item2[0], Is.EqualTo(classParamsDown));
        }

        [Test]
        public void FilterParams_WithMethodNameOnly_ReturnsOnlyMethodParams()
        {
            // Arrange
            var useClassDataPreparation = false;
            string methodName = "TestMethod";
            var classParamsUp = new object[] { "test1", 123 };
            var methodParamsUp = new object[] { "test2", 456 };
            var classParamsDown = new object[] { "test3", 789 };
            var methodParamsDown = new object[] { "test4", 101112 };

            // Act
            var result = GetDataPreparation.FilterParams(
                useClassDataPreparation,
                methodName,
                classParamsUp,
                methodParamsUp,
                classParamsDown,
                methodParamsDown);

            // Assert
            Assert.That(result.Item1.Length, Is.EqualTo(1));
            Assert.That(result.Item2.Length, Is.EqualTo(1));
            Assert.That(result.Item1[0], Is.EqualTo(methodParamsUp));
            Assert.That(result.Item2[0], Is.EqualTo(methodParamsDown));
        }

        [Test]
        public void FilterParams_WithClassAndMethodName_ReturnsBothParamSets()
        {
            // Arrange
            var useClassDataPreparation = true;
            string methodName = "TestMethod";
            var classParamsUp = new object[] { "test1", 123 };
            var methodParamsUp = new object[] { "test2", 456 };
            var classParamsDown = new object[] { "test3", 789 };
            var methodParamsDown = new object[] { "test4", 101112 };

            // Act
            var result = GetDataPreparation.FilterParams(
                useClassDataPreparation,
                methodName,
                classParamsUp,
                methodParamsUp,
                classParamsDown,
                methodParamsDown);

            // Assert
            Assert.That(result.Item1.Length, Is.EqualTo(2));
            Assert.That(result.Item2.Length, Is.EqualTo(2));
            Assert.That(result.Item1[0], Is.EqualTo(classParamsUp));
            Assert.That(result.Item1[1], Is.EqualTo(methodParamsUp));
            Assert.That(result.Item2[0], Is.EqualTo(classParamsDown));
            Assert.That(result.Item2[1], Is.EqualTo(methodParamsDown));
        }

        [Test]
        public void FilterParams_WithNeitherClassNorMethod_ReturnsEmptyArrays()
        {
            // Arrange
            var useClassDataPreparation = false;
            string methodName = null;
            var classParamsUp = new object[] { "test1", 123 };
            var methodParamsUp = new object[] { "test2", 456 };
            var classParamsDown = new object[] { "test3", 789 };
            var methodParamsDown = new object[] { "test4", 101112 };

            // Act
            var result = GetDataPreparation.FilterParams(
                useClassDataPreparation,
                methodName,
                classParamsUp,
                methodParamsUp,
                classParamsDown,
                methodParamsDown);

            // Assert
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(result.Item2.Length, Is.EqualTo(0));
        }
    }
}