using System;
using System.Linq;
using NUnit.Framework;
using DataPreparation.Exceptions;

namespace DataPreparation.UnitTests.Exceptions
{
    [TestFixture]
    public class ExceptionAggregatorTests
    {
        private ExceptionAggregator _exceptionAggregator = null!;

        [SetUp]
        public void Setup()
        {
            _exceptionAggregator = new ExceptionAggregator();
        }

        [Test]
        public void Add_SingleException_AddsToInternalCollection()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            
            // Act
            _exceptionAggregator.Add(exception);
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<AggregateException>());
            var aggregateEx = (AggregateException)result;
            Assert.That(aggregateEx.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(aggregateEx.InnerExceptions[0], Is.SameAs(exception));
        }

        [Test]
        public void Add_MultipleExceptions_AddsAllToInternalCollection()
        {
            // Arrange
            var exception1 = new InvalidOperationException("Test exception 1");
            var exception2 = new ArgumentException("Test exception 2");
            var exception3 = new NullReferenceException("Test exception 3");
            
            // Act
            _exceptionAggregator.Add(exception1);
            _exceptionAggregator.Add(exception2);
            _exceptionAggregator.Add(exception3);
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<AggregateException>());
            var aggregateEx = (AggregateException)result;
            Assert.That(aggregateEx.InnerExceptions, Has.Count.EqualTo(3));
            Assert.That(aggregateEx.InnerExceptions, Contains.Item(exception1));
            Assert.That(aggregateEx.InnerExceptions, Contains.Item(exception2));
            Assert.That(aggregateEx.InnerExceptions, Contains.Item(exception3));
        }

        [Test]
        public void Add_AggregateException_FlattensInnerExceptions()
        {
            // Arrange
            var innerException1 = new InvalidOperationException("Inner exception 1");
            var innerException2 = new ArgumentException("Inner exception 2");
            var aggregateException = new AggregateException("Aggregate exception", innerException1, innerException2);
            
            // Act
            _exceptionAggregator.Add(aggregateException);
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<AggregateException>());
            var resultAggregateEx = (AggregateException)result;
            Assert.That(resultAggregateEx.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(resultAggregateEx.InnerExceptions, Contains.Item(innerException1));
            Assert.That(resultAggregateEx.InnerExceptions, Contains.Item(innerException2));
        }

        [Test]
        public void Get_WithNoExceptions_ReturnsNull()
        {
            // Act
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Get_WithSingleException_ReturnsAggregateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _exceptionAggregator.Add(exception);
            
            // Act
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<AggregateException>());
            var aggregateEx = (AggregateException)result;
            Assert.That(aggregateEx.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(aggregateEx.InnerExceptions[0], Is.SameAs(exception));
        }

        [Test]
        public void Get_CalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _exceptionAggregator.Add(exception);
            
            // Act
            var result1 = _exceptionAggregator.Get();
            var result2 = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result1.InnerExceptions[0], Is.SameAs(result2.InnerExceptions[0]));
        }

        [Test]
        public void Clear_RemovesAllExceptions()
        {
            // Arrange
            var exception1 = new InvalidOperationException("Test exception 1");
            var exception2 = new ArgumentException("Test exception 2");
            _exceptionAggregator.Add(exception1);
            _exceptionAggregator.Add(exception2);
            
            // Act
            _exceptionAggregator.Clear();
            var result = _exceptionAggregator.Get();
            
            // Assert
            Assert.That(result, Is.Null);
        }
        

        [Test]
        public void AddAndThrowIfNecessary_WithExistingExceptions_AddsAndThrowsAggregate()
        {
            // Arrange
            var existingException = new InvalidOperationException("Existing exception");
            _exceptionAggregator.Add(existingException);
            var newException = new ArgumentException("New exception");
            
            // Act & Assert
            var ex = Assert.Throws<AggregateException>(() => throw _exceptionAggregator.Add(newException));
            Assert.That(ex.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(ex.InnerExceptions.Contains(existingException), Is.True);
            Assert.That(ex.InnerExceptions.Contains(newException), Is.True);
        }

        [Test]
        public void AddAndThrowIfNecessary_WithNoExistingExceptionsButNewException_ThrowsOnlyNewException()
        {
            // Arrange
            var newException = new ArgumentException("New exception");
            
            // Act & Assert
            var ex = Assert.Throws<AggregateException>(() => throw _exceptionAggregator.Add(newException));
            Assert.That(ex.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(ex.InnerExceptions[0], Is.SameAs(newException));
        }
    }
}