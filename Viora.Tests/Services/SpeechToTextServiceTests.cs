using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Viora.AIResponses;
using Viora.Services;

namespace Viora.Tests.Services
{
    [TestFixture]
    public class SpeechToTextServiceTests
    {
        private Mock<IConfiguration> _configMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private SpeechToTextService _service;

        [SetUp]
        public void SetUp() {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _configMock
                .Setup(c => c["ApiSettings:SpeechToTextEndPoint"])
                .Returns("https://fake.com");


            _service = new SpeechToTextService(_httpClientFactoryMock.Object, _configMock.Object);


        }

        private void SetUpHttpResponse(HttpStatusCode statusCode, string responseContent)
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent)
                });

            HttpClient httpClient = new HttpClient(handlerMock.Object);

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }


        [Test]
        public async Task ConvertSpeechToTextAsync_Success_String()
        {
            //Arrange
            var expected = new WhisperSpeechResult { DisplayText = "Hello, this is a test.", Status = "Success" };
            string json = JsonConvert.SerializeObject(expected);
            SetUpHttpResponse(HttpStatusCode.OK, json);


            //Act
            Stream audioStream = new MemoryStream(new byte[] { 0x52, 0x49, 0x46, 0x46 });
            var result = await _service.ConvertSpeechToTextAsync(audioStream, "test");



            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.DisplayText, Is.EqualTo(expected.DisplayText));
                Assert.That(result.Status, Is.EqualTo(expected.Status));
            });
        }


        [Test]
        public async Task ConvertSpeechToTextAsync_InternalServerError_ThrowsHttpRequestException()
        {
            // Arrange
            SetUpHttpResponse(
                HttpStatusCode.InternalServerError,
                "{\"error\":\"Internal Server Error\"}");

            Stream audioStream = new MemoryStream(new byte[] { 0x52, 0x49, 0x46, 0x46 });

            // Act and Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.ConvertSpeechToTextAsync(audioStream, "test"));
        }
    }
}

