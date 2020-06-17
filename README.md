# githubscrapper

This project implements a .Net Core API that provide a service to get the total number of lines and total number of bytes of all files from a given public GitHub repository, grouped by file extension.

A lab application is published in AWS service at follow url: <a target="_blank" href="http://rbl-github-scrapping.us-west-2.elasticbeanstalk.com/">AWS Service</a>


Requirements

- This API is written in .Net Core 3.1 C# 8.0;
- This API retrieve data from Github website by using web scraping techniques and it do not use Github’s API;
- This API do not use web scraping libraries;
- This API is written using async functions and multithreading methods;
- In general, first request takes some time to extract information from all files of GitHub repository. However, subsequent requests are returned immediately;
- This API is based in a DDD Pattern to structure project layers;
- This API is deployed to Amazon AWS Elastic Beamstalk cloud provider;
- Below, you can see the API contract available to scrap a gitHub repository:

Method API Scrapping 
URL: http://rbl-github-scrapping.us-west-2.elasticbeanstalk.com/api/scrapping/
Method: POST
Form-Data: 
 - gitHubRepository: Full GitHub Repository URL (https://github.com/bleiterafael/githubscrapper for example)
 - navigateSubFolders (optional, default=true): tells to API navigate into all folders from GitHub Repository
Response: Json with relevant data


