using System;
using CasinoLibrary.Model.UserException;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Logging;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class ApiMethodOperationHandler<T>
    {
        private readonly ILogger _logger;

        public ApiMethodOperationHandler()
        {
            _logger = Logger.Log;
        }

        public ApiMethodOperationHandler(ILogger logger)
        {
            _logger = logger;
        }

        public delegate V OutAction<in IInput, out V>(IInput input);


        public T Handle<TInput>(TInput input, OutAction<TInput, T> codeBlock)
        {
            T result = default(T);
            try
            {
                result = codeBlock(input);
            }
            catch (Exception exception)
            {
                if (exception is ErrorException)
                {
                    _logger.Error(LogMessageHelper.GenerateMessage(exception), exception);
                }
                else
                {
                    throw;
                }
            }

            return result;
        }
    }
}
