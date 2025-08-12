namespace Fanex.Athena.LGAPI.Core.UnitTests.HelperTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoFixture;

    public static class FakeHelper
    {
        private static Fixture _fixture = new Fixture();

        public static T Create<T>(Expression<Func<T, object>> propertyPicker, object value)
        {
            return _fixture.Build<T>().With(propertyPicker, value).Create();
        }

        public static T Create<T>()
        {
            return _fixture.Create<T>();
        }

        public static IEnumerable<T> CreateMany<T>()
        {
            return _fixture.CreateMany<T>();
        }

        public static IEnumerable<T> CreateMany<T>(int count)
        {
            return _fixture.CreateMany<T>(count);
        }

        public static DateTime TruncateMiliseconds(DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
        }
    }
}
