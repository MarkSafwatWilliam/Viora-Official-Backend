using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Viora.Services;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Viora.Tests.Services
{

    [TestFixture]
    public class TextToSpeechServiceTests
    {

        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configMock;
        private TextToSpeechService _service;

        [SetUp]
        public void SetUp()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();

            _configMock
                .Setup(c => c["ApiSettings:TextToSpeechEndPoint"])
                .Returns("https://fake-api.com");


            _service = new TextToSpeechService(_httpClientFactoryMock.Object, _configMock.Object);
        }


        private void SetUpHttpResponse(HttpStatusCode statusCode, byte[] audio)
        {

            //H S R C F
            //H = Handler mock
            //S = Setup SendAsync
            //R = Return fake response
            //C = Create HttpClient
            //F = Fake factory

            //Step 1 : Mock the newtork layer to return the expected response
            var handlerMock = new Mock<HttpMessageHandler>();

            //Step 2 : Intercept the SendAsync method to return the mocked response
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode, Content = new ByteArrayContent(audio) });

            //Step 3 :(Inject) Create a HttpClient with the mocked handler
            var httpClient = new HttpClient(handlerMock.Object);


            //Step 4: Give fake client to factory
            _httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
        }

        [Test]
        //testing whether ConvertTextToSpeech() method behaves correctly when the API returns a successful response
        public async Task ConvertTextToSpeechAsync_Success_ReturnsByteArray()
        {
            // Arrange: fake WAV audio bytes
            var fakeAudio = new byte[] { 0x52, 0x49, 0x46, 0x46 };
            SetUpHttpResponse(HttpStatusCode.OK, fakeAudio);


            // Act
            var result = await _service.ConvertTextToSpeech("Hello, world!");

            // Assert
            Assert.That(result, Is.EqualTo(fakeAudio));

        }



        [Test]
        //testing whether ConvertTextToSpeech() method behaves correctly when the API returns a unsuccessful response
        public async Task ConvertTextToSpeechAsync_Fail_Test()
        {
            // Arrange
            SetUpHttpResponse(HttpStatusCode.InternalServerError, Array.Empty<byte>());


            // Assert and Act
            Assert.ThrowsAsync<HttpRequestException>(async () => await _service.ConvertTextToSpeech("Hello"));

        }
    }
}
