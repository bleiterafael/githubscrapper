using System;
using Xunit;
using RBL.GitHub.Scrapper.API;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Formatting;
using System.Collections.Generic;

namespace RBL.GitHub.Scrapper.Tests
{
    public class ScrappingTest
    {
        private readonly HttpClient _client;

        public ScrappingTest()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = server.CreateClient();
        }

        [Theory]
        [InlineData("GET")]
        public async Task TestGet(string method)
        {
            //Arrange
            var request = new HttpRequestMessage(new HttpMethod(method), "api/scrapping/");

            //Act
            var response = await _client.SendAsync(request);

            //Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestInvalidGitHubRepository()
        {
            //Arrange
            string gitHubRepository = "";
            bool navigateSubFolders = false;

            //Act
            var response = await PostScrapping(gitHubRepository, navigateSubFolders);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertExpectedJson(response, $"Undefined repository");

        }

        [Fact]
        public async Task TestInvalidDomainRepository()
        {
            //Arrange
            string gitHubRepository = "https://invaliddomain.com";
            bool navigateSubFolders = false;

            //Act
            var response = await PostScrapping(gitHubRepository, navigateSubFolders);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertExpectedJson(response, $"{gitHubRepository} does not belong a https://github.com/{{repository}} domain");
        }

        [Fact]
        public async Task TestNotFoundRepository()
        {
            //Arrange
            string gitHubRepository = "https://github.com/lsjghsdjfd";
            bool navigateSubFolders = false;

            //Act
            var response = await PostScrapping(gitHubRepository, navigateSubFolders);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertExpectedJson(response, $"Repository '{gitHubRepository}' not found");
        }

        [Fact]
        public async Task TestValidDomainRepository()
        {
            //Arrange
            string gitHubRepository = "https://github.com/bleiterafael/githubscrapper";
            bool navigateSubFolders = false;

            //Act
            var response = await PostScrapping(gitHubRepository, navigateSubFolders);
            
            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
        }

        private async Task<HttpResponseMessage> PostScrapping(string gitHubRepository, bool navigateSubFolders)
        {
            var formVariables = new List<KeyValuePair<string, string>>();
            formVariables.Add(new KeyValuePair<string, string>("gitHubRepository", gitHubRepository));
            formVariables.Add(new KeyValuePair<string, string>("navigateSubFolders", navigateSubFolders.ToString()));
            var formContent = new FormUrlEncodedContent(formVariables);

            var response = await _client.PostAsync("/api/scrapping/", formContent);
            return response;
        }

        private async Task AssertExpectedJson(
            HttpResponseMessage response,
            string errorMessage)
        {
            string receiveStream = await response.Content.ReadAsStringAsync();
            dynamic expectedJson = new
            {
                success = false,
                errors = new dynamic[]
                {
                    errorMessage
                }
            };

            string expectedJsonString = JsonConvert.SerializeObject(expectedJson);

            Assert.Equal(expectedJsonString, receiveStream);
        }
    }
}
