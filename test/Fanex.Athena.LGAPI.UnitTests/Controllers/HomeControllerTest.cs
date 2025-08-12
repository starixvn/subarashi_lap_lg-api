namespace Fanex.Athena.LGAPI.UnitTests.Controllers
{
    using System.Web.Mvc;
    using LGAPI.Controllers;
    using NUnit.Framework;

    [TestFixture]
    public class HomeControllerTest
    {
        [Test]
        public void Index_WhenCalled_ReturnsCorrectMessage()
        {
            using (var controller = new HomeController())
            {
                var result = controller.Index();

                Assert.That(result, Is.EqualTo("LG API is running"));
            }
        }

        [Test]
        public void Index_WhenCalled_ReturnsString()
        {
            using (var controller = new HomeController())
            {
                var result = controller.Index();

                Assert.That(result, Is.TypeOf<string>());
            }
        }
    }
}