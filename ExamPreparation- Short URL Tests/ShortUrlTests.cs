using NUnit.Framework;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Net;
namespace ExamPreparation__Short_URL_Tests
{
    public class ShortUrlTests
    {
        private const string apiUrl = "https://shorturl-1.adelinapetrova.repl.co/api";
        private RestClient client;

        [SetUp]
        public void Setup()
        {
            this.client = new RestClient(apiUrl);
            this.client.Timeout = 3000;
        }
        [Test]
        public void Test_Short_URL_ListShortUrls()
        {
            //arrange
            var request = new RestRequest("/urls", Method.GET);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var urls = new JsonDeserializer().Deserialize<List<UrlResponse>>(response);
            Assert.IsTrue(urls != null);
        }
        [Test]
        public void Test_Find_ShortURLByShortCode_Valid()
        {
            //arrange
            var request = new RestRequest("/urls/nak", Method.GET);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var expextedUrl = new UrlResponse
            {
                Url = "https://nakov.com",
                ShortCode = "nak",
                ShortUrl = "http://shorturl-1.adelinapetrova.repl.co/go/nak",
                DateCreated = "2021-02-17T14:41:33.000Z",
                Visits = 166
            };
            var responseUrl = new JsonDeserializer().Deserialize<UrlResponse>(response);
            AsserObjectsAreEqual(expextedUrl, responseUrl);
        }
        private void AsserObjectsAreEqual(object obj1, object obj2)
        {
            string obj1JSON = new JsonDeserializer().Serialize(obj1);
            string obj2JSON = new JsonDeserializer().Serialize(obj2);
            Assert.AreEqual(obj1JSON, obj2JSON);
        }
        [Test]
        public void Test_Find_ShortURL_ByShortCode_Invalid()
        {
            //arrange
            var request = new RestRequest("/urls/testNotCorrect23", Method.GET);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Test]
        public void Test_Create_New_ShortUrl()
        {
            //arrange
            var request = new RestRequest("/urls", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var newUrlData = new
            {
                url = "https://dir.bg",
                shortCode = "dir" + DateTime.Now.Ticks
            };
            request.AddJsonBody(newUrlData);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseUrl = new JsonDeserializer().Deserialize<CreateUrlResponse>(response);
            Assert.AreEqual("Short code added.", responseUrl.msg);
            Assert.AreEqual(newUrlData.url, responseUrl.url.Url);
            Assert.AreEqual(newUrlData.shortCode, responseUrl.url.ShortCode);
            Assert.AreEqual(0, responseUrl.url.Visits);
        }
        [Test]
        public void Test_Create_ShortUrl_InvalidCode()
        {
            //arrange
            var request = new RestRequest("/urls", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var newUrlData = new
            {
                url = "https://dir.bg",
                shortCode = "invalid code"
            };
            request.AddJsonBody(newUrlData);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Test]
        public void Test_Create_ShortUrl_DuplicatedCode()
        {
            //arrange
            var request = new RestRequest("/urls", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var newUrlData = new
            {
                url = "https://dir.bg",
                shortCode = "nak"
            };
            request.AddJsonBody(newUrlData);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Test]
        public void Test_Delete_Created_ShortUrl()
        {
            //Create new url
            var request = new RestRequest("/urls", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var newUrlData = new
            {
                url = "https://www.telenor.bg/",
                shortCode = "globul" + DateTime.Now.Ticks
            };
            request.AddJsonBody(newUrlData);
            var response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            //After creation delete the url
            //act
            var delRequest = new RestRequest("/urls/" + newUrlData.shortCode, Method.DELETE);
            var delResponse = client.Execute(delRequest);
            //assert
            Assert.AreEqual(HttpStatusCode.OK, delResponse.StatusCode);
        }
        [Test]
        public void Test_Delete_Created_ShortUrl_InvalidData()
        {
            var delRequest = new RestRequest("/urls/invalidCode2323", Method.DELETE);
            //act
            var delResponse = client.Execute(delRequest);
            //assert
            Assert.AreEqual(HttpStatusCode.NotFound, delResponse.StatusCode);
        }
        [Test]
        public void Test_Visit_ShortURL_Valid()
        {
            //arrange
            var request = new RestRequest("/urls/visit/seldev", Method.POST);
            //act
            var response1 = client.Execute(request);
            var response2 = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            var responseUrl1 = new JsonDeserializer().Deserialize<UrlResponse>(response1);
            var responseUrl2 = new JsonDeserializer().Deserialize<UrlResponse>(response2);
            Assert.AreEqual(1, responseUrl2.Visits - responseUrl1.Visits);
        }
        [Test]
        public void Test_Visit_ShortURL_Invalid()
        {
            //arrange
            var request = new RestRequest("/urls/visit/invalidCode212", Method.POST);
            //act
            var response = client.Execute(request);
            //assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}