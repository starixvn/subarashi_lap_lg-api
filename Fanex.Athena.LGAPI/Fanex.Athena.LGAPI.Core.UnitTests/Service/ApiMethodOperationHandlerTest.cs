namespace Fanex.Athena.LGAPI.Core.UnitTests.Service
{
    using System;
    using System.Data;
    using CasinoLibrary.Model.UserException;
    using Fanex.Athena.LGAPI.Core.Service.RNGCasino;
    using Logging;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class ApiMethodOperationHandlerTest
    {
        private readonly ILogger _logger = Substitute.For<ILogger>();

        [TearDown]
        public void TearDown()
        {
            _logger.ClearReceivedCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        [TestCase(100)]
        public void Handle_WhenCall_ReturnExpectedValue(object expectedValue)
        {
            // Arrange
            var handler = new ApiMethodOperationHandler<object>(_logger);

            // Action
            var result = handler.Handle(new object(), (obj) => SuccessCodeBlock<object>(obj, expectedValue));

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void Handle_WhenCall_ThrowError()
        {
            // Arrange
            var handler = new ApiMethodOperationHandler<object>(_logger);

            // Action
            TestDelegate result = () => handler.Handle(new object(), (obj) => ThrowOtherExceptionCodeBlock<object>(obj));

            // Assert
            Assert.Throws<ArgumentException>(result);
        }

        [Test]
        public void Handle_WhenCall_ReturnDefaultValueOfOutPutType()
        {
            // Arrange
            var handler = new ApiMethodOperationHandler<DataTable>(_logger);

            // Action
            var result = handler.Handle(new object(), (obj) => ThrowErrorExceptionCodeBlock<DataTable>(obj));

            // Assert
            Assert.AreEqual(default(DataTable), result);
        }

        [Test]
        public void Handle_WhenCall_ThrowSpecialExceptionAndWriteLog()
        {
            // Arrange
            var handler = new ApiMethodOperationHandler<DataTable>(_logger);

            // Action
            /*
             * Argument matchers should only be used when setting return values
             * or checking received calls. Using Arg.Is or Arg.Any without a call to
             * .Returns or Received() can cause your tests to behave in unexpected ways
             */
            using (var dataTable = new DataTable())
            {

                // Action
                /*
                 * Argument matchers should only be used when setting return values
                 * or checking received calls. Using Arg.Is or Arg.Any without a call to
                 * .Returns or Received() can cause your tests to behave in unexpected ways
                 */
                var result = handler
                    .Handle(dataTable, (obj) => ThrowErrorExceptionCodeBlock<DataTable>(obj));

                // Assert
                _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
            }
        }

        private T SuccessCodeBlock<T>(object input, T expectedValue)
        {
            return expectedValue;
        }

        private T ThrowErrorExceptionCodeBlock<T>(object input)
        {
            throw new ErrorException("Unit Text Error");
        }

        private T ThrowOtherExceptionCodeBlock<T>(object input)
        {
            throw new ArgumentException("Invalid input");
        }
    }
}