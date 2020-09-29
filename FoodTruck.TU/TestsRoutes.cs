using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FoodTruck.TU
{
    [TestClass]
    public class TestsRoutes
    {

        [TestMethod]
        public void Routes_PageHome()
        {
            RouteData routeData = DefinirUrl("~/");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Home", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual(UrlParameter.Optional, routeData.Values["id"]);
        }
        [TestMethod]
        public void Routes_PageHomeIndex2()
        {
            RouteData routeData = DefinirUrl("~/Home/Index/2");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Home", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual("2", routeData.Values["id"]);
        }
        [TestMethod]
        public void Routes_PageHomeIndex()
        {
            RouteData routeData = DefinirUrl("~/Home/Index");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Home", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual(UrlParameter.Optional, routeData.Values["id"]);
        }

        [TestMethod]
        public void Routes_PageAPropos()
        {
            RouteData routeData = DefinirUrl("~/APropos");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("APropos", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual(UrlParameter.Optional, routeData.Values["id"]);
        }

        [TestMethod]
        public void Routes_PageArticleDetailsPomme()
        {
            RouteData routeData = DefinirUrl("~/Article/Details/Pomme");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Article", routeData.Values["controller"]);
            Assert.AreEqual("Details", routeData.Values["action"]);
            Assert.AreEqual("Pomme", routeData.Values["nom"]);
        }
        [TestMethod]
        public void Routes_PageArticlePomme()
        {
            RouteData routeData = DefinirUrl("~/Article/Pomme");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Article", routeData.Values["controller"]);
            Assert.AreEqual("Details", routeData.Values["action"]);
            Assert.AreEqual("Pomme", routeData.Values["nom"]);
        }
        [TestMethod]
        public void Routes_PageArticle()
        {
            RouteData routeData = DefinirUrl("~/Article");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Article", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual(null, routeData.Values["id"]);
        }
        [TestMethod]
        public void Routes_PageArticles()
        {
            RouteData routeData = DefinirUrl("~/Articles");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Article", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
        }

        [TestMethod]
        public void Routes_PageCommande()
        {
            RouteData routeData = DefinirUrl("~/Commande");
            Assert.IsNotNull(routeData);
            Assert.AreEqual("Commande", routeData.Values["controller"]);
            Assert.AreEqual("Index", routeData.Values["action"]);
            Assert.AreEqual(UrlParameter.Optional, routeData.Values["id"]);
        }











        [TestMethod]
        public void Routes_UrlBidon_RetourneNull()
        {
            RouteData routeData = DefinirUrl("/Url/bidon/Trop/Param");
            Assert.IsNull(routeData);
        }

















        private static RouteData DefinirUrl(string url)
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns(url);
            RouteCollection routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);
            RouteData routeData = routes.GetRouteData(mockContext.Object);
            return routeData;
        }
    }
}
