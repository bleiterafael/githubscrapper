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

Request:
<img src="https://i.imgur.com/jLd0fVs.png"></img><br/>

Response:
```
{
    "success": true,
    "data": {
        "gitHubRepository": "https://github.com/bleiterafael/githubscrapper",
        "totalFiles": 35,
        "totalLines": 1166,
        "totalSize": 1237473,
        "totalSizeDescription": "1237473 bytes",
        "lastUpdate": "2020-06-17T07:11:42+00:00",
        "processTime": "0.3801434 s",
        "scrappingItems": [
            {
                "extension": "cs",
                "totalFiles": 25,
                "totalLines": 979,
                "totalSize": 945163,
                "totalSizeDescription": "945163 bytes"
            },
            {
                "extension": "csproj",
                "totalFiles": 4,
                "totalLines": 55,
                "totalSize": 1473,
                "totalSizeDescription": "1473 bytes"
            },
            {
                "extension": "json",
                "totalFiles": 3,
                "totalLines": 49,
                "totalSize": 1140,
                "totalSizeDescription": "1140 bytes"
            },
            {
                "extension": "md",
                "totalFiles": 1,
                "totalLines": 27,
                "totalSize": 14336,
                "totalSizeDescription": "14336 bytes"
            },
            {
                "extension": "sln",
                "totalFiles": 1,
                "totalLines": 43,
                "totalSize": 274432,
                "totalSizeDescription": "274432 bytes"
            },
            {
                "extension": "user",
                "totalFiles": 1,
                "totalLines": 13,
                "totalSize": 929,
                "totalSizeDescription": "929 bytes"
            }
        ]
    }
}
```

