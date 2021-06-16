# AWS-Web-Advert
Getting started with AWS. This project has been created as a practice along with this <a href="https://www.udemy.com/course/build-microservices-with-aspnet-core-amazon-web-services/">Udemy Course </a>

### Project Infastructure

The VPC (Private infastructure) is divided into 2 subnets.

- **Private Subnet** - (Not accessible from internet)
   This includes Microservice (Advert.API), Dynamo DB, Elastic Search (Search.API & Search.Worker-AWS Lamda ), SNS (Messaging)
    

- **Public Subnet** - (Accessible from internet)
     This includes AWS Cognito (Signup + Auth), S3 Bucket (Upload Image), CloudFront (Display Image - Caching image, css, fonts etc to user region ), Web UI (Cluster),


Advert API is placed in private subnet. It is not neded to be accessed from internet directly. Users can login through AWS Cognito (placed in public subnet)


### Working of Microservices in AWS

- We can deploy our .NET Core WebAPI Microservice either on  Amazon EC2 ( virtual server) or deploy it Amazon ECS (container service) using Docker and AWS App Mesh (Monitoring service)
- We can also use .NET Core to create lamda functions (serverless).
- We add Application Load Balancer infront of our EC2/ECS.
- Microservices should have data offloading, so insted of holding data in memory, they should put it in centralised cache (ElasticCache)
- Every Microservice should have it's own database. But creating different instance of database like SQL Server is costly, therefore we use Amazon RDS (Relation Database Service). For NoSql Database, we can use DynamoDB.
- Client (browser or mobile app) connect to Amazon CloudFront(Caching service). Cloudfront sends data to API Gateway (Aggragates all API into one address)


### Authentication and Authorization

**AWS Cognito** has all the following features:

- The applicatiion supports authentication through OAuth and OpenId Connect 
- It supports linking with Google and Facebook
- It is plugged into ASP.NET Core Identity
- It suupports token authentication (with JWT) as well as API authentication

**AWS Console Steps**

- Go to Service -> Cognito
- Create User pool and add attributes, change password policy, set verification rules, add App Clints like web client, ios client etc
- Create IAM user and attch policy **AmazonCognitoDeveloperAuthentication** and **AdministratorAccess**. Go to Security Credentials Tab and create Access key

**Windows System Steps**

- Create profile in our Windows System. Fo to Users root directory (type %USERPROFILE%). Create folder .aws.. Add file credentials

```
[default]
aws_access_key_id = XXXXXXXXXXXXXXXXXXXXXX
aws_secret_access_key = YYYYYYYYYYYYYYYYYY
```


### #Microservice 1 - WebAdvert.Web - This is our Web UI.

This is a ASP.NET Core MVC Web Application.

It has the following pages:

- SignUp, Login and Confirm Password pages which connects with AWS Cognito. AWS Nuget Packages has been used **Amazon.AspNetCore.Identity.Cognito** and **Amazon.Extensions.CognitoAuthentication**

### #Microservice 2 - Advert.API - This is the API to add Advertisements

This is a ASP.NET Core WebAPI Application. It connects with DynamoDB database.

**AWS Console Steps for DynamoDB**

- Go to Service -> DynamoDB
- Create a table( here : Adverts)


The Microservice has the following enpoints:

- An endpoint to add Advertisements. To connect with DynamoDB, AWS Nuget Packages **AWSSDK.DynamoDBv2** has been used. In order to use DataModel with DynamoDB, some attributes from Amazon.DynamoDBv2.DataModel like [DynamoDBTable], [DynamoDBProperty] has been added to it.

```
using (var client = new AmazonDynamoDBClient())
{
   using (var context = new DynamoDBContext(client))
   {
      await context.SaveAsync(dbModel);
   }
}
```

**Adding Health Check to the Microservice**

This is added to check if the application is alive. We do it by using .Microsoft.AspNetCor.HealthChecks `AddHealthChecks` in startup.cs We have also added health check for individual service.
