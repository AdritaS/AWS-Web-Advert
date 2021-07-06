# AWS-Web-Advert
Getting started with AWS. This project has been created as a practice along with this <a href="https://www.udemy.com/course/build-microservices-with-aspnet-core-amazon-web-services/">Udemy Course </a>

![image](https://user-images.githubusercontent.com/29271635/122780086-202e5200-d2cc-11eb-8ba8-bd8063bdfe53.png)


### Description

The Application is built for posting Advertisements and searching them. 

- User Management is handled through **AWS Cognito** 
- User can post Advertisement details with an image. The entry is added in **DynamoDB** and the image is uploaded to  **S3 Bucket**
- When an entry is added in database, the API sends a message (Id and Title) to **SNS**
- SearchWorker microservice subscribes to **SNS** and creates a new document in **Elastic Search**
- When we search something, Search.API is called which gets a list from Elastic Search Container.

### Project Infastructure

The VPC (Private infastructure) is divided into 2 subnets.

- **Private Subnet** - (Not accessible from internet)
   This includes Microservice (Advert.API), Dynamo DB, Elastic Search (Search.API & Search.Worker-AWS Lambda ), SNS (Messaging)
    

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

### Domain Driven Design

Domain is the area of business where the application is intended to apply. Eg, Order, Payment and Inventory are different domins. In our application #Microservice 2 - Advert.API is Advert Management Domain and #Microservice 4 - Search.API & #Microservice 3 - WebAdvert.SearchWorker are part of Search Domain.

Domain Driven Design is a Software Design Method where we focus on the domain and domain logic. We are focussed on the business over technology and conatantly consult with DOmain expert to improve domain understanding.

### CQRS

Command Query Responsibility Segregation is an architectural pattern that separates reading and writing into two different models. It does responsibility segregation for the Command model & Query model. In our Architecture, **#Microservice 2 - Advert.API** is the Command Model (i.e writing Advertisements to database) and **#Microservice 4 - WebAdvert.SearchAPI** is for Query Model (Searching Advertisements for displaying)


## #Microservice 1 - WebAdvert.Web - This is our Web UI.

**Authentication and Authorization**

**AWS Cognito**  supports authentication through OAuth and OpenId Connect. It can be plugged into ASP.NET Core Identity.It supports token authentication (with JWT) as well as API authentication

**AWS Console Steps**

- Go to Service -> Cognito
- Create User pool and add attributes, change password policy, set verification rules, add App Clients like web client, ios client etc
- Create IAM user and attach policy **AmazonCognitoDeveloperAuthentication** and **AdministratorAccess**. Go to Security Credentials Tab and create Access key

**Windows System Steps**

- Create profile in our Windows System. Go to Users root directory (type %USERPROFILE%). Create folder .aws.. Add file credentials

```
[default]
aws_access_key_id = XXXXXXXXXXXXXXXXXXXXXX
aws_secret_access_key = YYYYYYYYYYYYYYYYYY
```

This is a ASP.NET Core MVC Web Application.

It has the following pages:

### SignUp, Login and Confirm Password pages


It connects with AWS Cognito. AWS Nuget Packages has been used **Amazon.AspNetCore.Identity.Cognito** and **Amazon.Extensions.CognitoAuthentication**

      private readonly CognitoUserPool _pool;
      private readonly SignInManager<CognitoUser> _signInManager;
      private readonly UserManager<CognitoUser> _userManager;
      
      public AccountsController(SignInManager<CognitoUser> signInManager,
            UserManager<CognitoUser> userManager, CognitoUserPool pool)
      {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
      }
      
       [HttpPost]
        public async Task<IActionResult> SignUp(SignUpModel model)
        {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email id already exists");
                    return View(model);
                }
                user.Attributes.Add("name", model.Email);

                var createdUser = await _userManager.CreateAsync(user, model.Password);

                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm");
                }              
            return View();
        }
        
        

        
### Advertisement Management page 

It is used to create a new Advertisement (using #Microservice 2 - Advert.API) and s3 Bucket to upload image. AWS Nuget Packages **AWSSDK.S3** has been used.


**AWS Console Steps for S3 Bucket**

- Go to Service -> Amazon S3
- Create a new bucket

```
     var bucketName = _configuration.GetValue<string>("ImageBucket");

            using (var client = new AmazonS3Client())
            {
                if (storageStream.Length > 0)
                    if (storageStream.CanSeek)
                        storageStream.Seek(0, SeekOrigin.Begin);

                var request = new PutObjectRequest
                {
                    AutoCloseStream = true,
                    BucketName = bucketName,
                    InputStream = storageStream,
                    Key = fileName
                };
                var response = await client.PutObjectAsync(request).ConfigureAwait(false);
                return response.HttpStatusCode == HttpStatusCode.OK;
    }
```

### List Advertisement Page

This page displays all Advertisement by connect with #Microservice 2 Advert.API. To display the images (from S3 bucket) associated with each advertisement, following items are needed to be setup.

S3 bucket does't have read permission to the images who aren't logged in to AWS Console, also S3 doesn't provide caching. Thefore we need to setup Amazon CloudFront

**AWS Console Steps for CloudFront**

- Go to Service -> CloudFront
- Create new Distribution -> Get Started on Web -> Set S3 bucket name in the Original Domain Name -> Choose Restrict Bucket Access
- Open the created Distribution and copy the Domain Name to implement it on our website.

We put the Domain name in config file of  WebAdvert.Web appsettings.json.

    {
        "ImageBaseUrl": "http://dxxxxxxxxxo7.cloudfront.net"
    }

### Search Management (using #Microservice 4 - Search.API)

The Home page has a search box. When we type something, Microservice 4 - Search.API is called which in turn gets a list from Elastic Search Container.


## #Microservice 2 - Advert.API - This is the API to add Advertisements

This is a ASP.NET Core WebAPI Application. It connects with DynamoDB database.

**AWS Console Steps for DynamoDB**

- Go to Service -> DynamoDB
- Create a table( here : Adverts)


The Microservice has the following enpoints:

- An endpoint to add Advertisements. To connect with DynamoDB, AWS Nuget Package **AWSSDK.DynamoDBv2** has been used. In order to use DataModel with DynamoDB, some attributes from Amazon.DynamoDBv2.DataModel like [DynamoDBTable], [DynamoDBProperty] has been added to it.

```
using (var client = new AmazonDynamoDBClient())
{
   using (var context = new DynamoDBContext(client))
   {
      await context.SaveAsync(dbModel);
   }
}
```

When Advert API creates an advertisement in database, it sends a message (using **SNS**) to SearchWorker ( #Microservice 3), the SearchWorker creates a new document in **Elastic Search**. When user types for an Advertisement, it sends a request to  #Microservice 4 - WebAdvert.SearchAPI


**Messaging Concept**

- A message is a type of notification that a  microservice can send out.
- A message that is triggered when a state of the system changes is called an **Event**, eg AdvertisementCreated is raised when an advertisement is added to the database
- Messages can be directly sent to subscribers  (subscriber has to be available at that time) or can be placed in Queue, so that any subscriber to the message channel can poll (with a time interval)  and receive the message when the subscriber becomes available
- To implement messing in **AWS**, we can using **Simple Notification Service (SNS)** to send or receive message or **Simple Queue Service (SQS)** to persist the message  and subscribe it with polling.

**AWS Console Steps for SNS**

- Go to Service -> SNS
- Create a topic  (AdvertAPI Topic) , choose standard and keep the topic ARN


In Advert.Api, TopicArn is added in appsettings.json.  AWS Nuget Package **AWSSDK.SimpleNotificationService** is added

       using (var client = new AmazonSimpleNotificationServiceClient())
       {
             var message = new AdvertConfirmedMessage
             {
                  Id = model.Id,
                  Title = dbModel.Title
             };

             var messageJson = JsonConvert.SerializeObject(message);
             await client.PublishAsync(topicArn, messageJson);
       }


**Adding Health Check and Resilient Pattern to the Microservice**

This is added to check if the application is alive. We do it by using .Microsoft.AspNetCor.HealthChecks `AddHealthChecks` in startup.cs We have also added health check for individual service.

Exponential Backoff  and  Circuit Breaker has been added using Polly Library.



## #Microservice 3 - WebAdvert.SearchWorker - This is the AWS Lambda Function to pickup SNS messages and create document in Elastic Search

This is a AWS Lambda (Serverless Functions). This becomes available only when needed and thus saving the infastructure cost. AWS Lambda can be plugged into SNS directly to pickup messages and then act on it.

The SearchWorker creates a new document in **Elastic Search** whenever it gets a message from **SNS**

To create Lamda project, I installed AWS Toolkit for Visual Studio and created a Lambda Project (.NET Core).It is like Class Library .NET Core Project. AWS Nuget Packages **Amazon.Lambda.Core**, **Amazon.Lambda.SNSEvents** and **Amazon.Lambda.Serialization.Json** have been used for Lambda functionality. 

Nuget Package **NEST** is installed to work with Elastic Search


**AWS Console Steps for Elastic Search** 

- Go to Service -> ElasticSearch
- Create a new domain (Elastic Search Domain is like container for our Elastic Search Instance)
-  Choose Deployment type as Development and testing and add a Elasticsearch domain name (eg advertapi)
- We chose Number of instance as 1 and Instance Type t3.small.elasticsearch
- We chose Number Storage Type EBS, EBS VolumeType Magnetic and size 10
- We chose Public access under Network configuration
- Select Fine-grained access control, choose Create master user. Provide a user name and password.
- For Domain access policy template choose Allow Open Access to the domain
- It provides an Elastic Search endpoint and a Kibana endpoint. Copy the Elastic Search endpoint from Overview tab of the Elastic Search Domain created and add it to Search worker's appsettings.json 

appsettings.json 

      {
        "ES": {
           "url": "https://search-advertapi-xxxxxxx.us-xxxx-1.es.amazonaws.com"
         }
       }

Lambda function

        public async Task FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
        {
            var node = new Uri("https://Username:Password@search-advertapi-xxxxxxx.us-xxxx-1.es.amazonaws.com/");

            var settings = new ConnectionSettings(node)
                .DefaultIndex("adverts");

            var client = new ElasticClient(settings);

            foreach (var record in snsEvent.Records)
            {
                context.Logger.LogLine(record.Sns.Message);

                var message = JsonConvert.DeserializeObject<AdvertConfirmedMessage>(record.Sns.Message);
                var advertDocument = new AdvertType
                                       {
                                           Id = message.Id,
                                           Title = message.Title,
                                           CreationDateTime = DateTime.UtcNow
                                       };
                var result = await client.IndexDocumentAsync(advertDocument);
                context.Logger.LogLine($"Result: {result.DebugInformation}");
            }
        }



**AWS Console Steps**

  - We need to create a role for uploading Lambda Function. The role tells Amazon, what services this Lambda can access. Go to **IAM**, create new Role -> Choose Lambda -> Choose policy CloudWatchLogsFullAccess -> we can Add tag - Name: SearchWorkerRole -> Give Role name SearchWorkerRole and create role.
  - Go to Service -> Lambda -> Create Function -> Choose AuthorFromScratch -> Give a name :searchworker, select Runtime (eg: .NET Core 3.1), for Role - selct use existing role (SearchWorkerRole).
  - Go to the created SearchWorker Lambda -> Add Trigger -> Select SNS -> Choose AdvertAPI Topic ARN
  - Upload lambda code :

### Uploading Lambda Function

- Right-Click on Project node and then choosing Publish to AWS Lambda.
- In the Upload Lambda Function window, enter a name for the function, or select a previously published function to republish.
- Set Handler as "Assembly::Namespace.Class::Function" 
- Set other details like IAM Role, Memory etc. and upload
- Test the Lambda function in AWS Console

## #Microservice 4 - Search.API - This is the API to search Advertisements

It is a ASP.NET Core Web API project. It searches Elasticsearch service for any item matching the searched keyword. Nuget Package **NEST** is installed to work with Elastic Search.
Elastic Search endpoint from Overview tab of the Elastic Search Domain created is added to appsettings.json 

appsettings.json: 

      {
        "ES": {
           "url": "https://Username:Password@search-advertapi-xxxxxxx.us-xxxx-1.es.amazonaws.com"
         }
       }

SearchService:

     public SearchService(IElasticClient client)
     {
         _client = client;
     }

     public async Task<List<AdvertType>> Search(string keyword)
     {
         var searchResponse = await _client.SearchAsync<AdvertType>(search => search.
             Query(query => query.
                 Term(field => field.Title, keyword.ToLower())
             ));

         return searchResponse.Hits.Select(hit => hit.Source).ToList();
     }


## Logging for Microservices in AWS

**Types of Logs**

- Infastructure Logs (eg: CPU/ Bandwidth uses)  - AWS Cloud Watch
- Security Logs - AWS Cloud Trail
- Change and Audit Logs (eg: Somebody deletes Elastic Search Domain)- AWS Cloud Trail
- Application Logs (using NLog or Log4net - via code

Our application send logs to **AWS Cloud Watch**. We can set up AWS Cloud Trail and it will send the logs to AWS Cloud Watch as well. AWS Cloud Watch can be configured to ship all the logs to Amazon Elastic Search Service (This launches a Lamda function automatically - which we don't see . It pics logs from Cloud watch and writes them to ELastic Search. Therefore the role used for AWS Cloud Watch must have access to execute AWS Lambda function)

To see what's going on in Elastic Serch when the logs are dumped, we use **Kibana**. We can use Amazon Cognito to provide autehtication to the users that can access Kibana Client

**AWS Console Steps**

- Go to Service -> **Cognito** -> Open WebAdvert User Pool and copy Pool Id and App client Id
- Go to Manage Identity Pool -> Create Identity Pool (KibanaUsers) and under Authentication providers add Pool Id and App client Id and create pool
- Note the Role Name and Allow
- Under IAM - Roles, we can see 2 roles created - CognitoKibanaUsersAuth and CognitoKibanaUsersUnauth. Copy the Role ARN of CognitoKibanaUsersAuth Role
- Go to Service -> **ElasticSearchService** -> Create a new Domain (webadvertslogs) todo (35 -4:16)
- Choose public access and enable Amazon Cognito for Authentication -> Choose WebAdevert User pool -> Choose KibanaUsers Identity Pool -> Choose domain template as Allow Open Access to the domain
- Pick the Role ARN of CognitoKibanaUsersAuth Role and under Add or edit access policy json AWS uder Principal section is '*' by default, replace it with the Role ARN of CognitoKibanaUsersAuth Role. This mens only these users can aceess Kibana

**Sending Data to Kibana**

All the logs we create in CloudWatch goes to Log Group

- Go to Service -> **CloudWatch** -> Logs -> Create Log Group (advertapi)
- Select the created log group and under Actions choose Stream To AmazonElasticSearch Service (todo 35- 8:00)
- Choose Amazon ES Cluster as the webadvertslogs (elastic search domain that we created for logs), select all default options and Start streaming


**Adding Log to #Microservice 4 - Search.API**

To connect with CloudWatch, AWS Nuget Package **AWS.Logger.AspNetCore** has been used.

Add Configuration in **appsettings.json**

    "AWS.Logging": {
       "Region": "us-xxxx-1",
       "LogGroup": "advertapi",
       "LogLevel": {
         "Default": "Information"
        }
     }
     
 Add Provider in **startup.cs**


      public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
      {
            loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection(),
                formatter: (loglevel, message, exception) => $"[{DateTime.Now} {loglevel} {message} {exception?.Message} {exception?.StackTrace}");
      }
      
  
  Add Logger in **controller**
  
    private readonly ILogger<SearchController> _logger;
    
    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
         _logger = logger;
         _logger.LogInformation("Search controller was called");
    }
    
## API Gateway in AWS

In Microservice world, clients (browser, Mobile App) doesn't call the api services (microservices) directly. They call the Load balancer. Only load balancer is public and visible to the clients, the api services are in private network (private subnet). This is suitable for small microservice based applications

For larger microservice based applications **API Gateway** is suitable, it is something between the clients and Load Balancer. Here API Gateway is public,  Load balancer and services are private. Authentication will be only in API Gateway and not on all the services ( saves time and effort if we have 20 microservices )

**AWS API Gateway Service**

Amazon API Gateway is an AWS service for creating, publishing, maintaining, monitoring, and securing REST, HTTP, and WebSocket APIs at any scale. API developers can create APIs that access AWS or other web services, as well as data stored in the AWS Cloud.

It can expose AWS Lambda functions as APIs, supports authentication, web firewall etc to reduce security risks. It also supports stages of API Development eg. Staging and Production

**Creating Revere proxy API using AWS API Gateway**

**AWS Console Steps**

- Go to EC2 instance of the AdvertAPI microservice we deployed and copy the public DNS (todo 42 , 0:23)
- Go to Service -> **API Gateway** 
- Create API -> Give a name (eg : Public Web API Proxy) and choose endpoint type Regional/Edge optimized -> Create API
- The created API is empty now, Go To Actions -> Create Resource -> Add name (eg: proxy) and check Enable API Gateway CORS
- Click on Create Resource and choose Integration Type as HTTP Proxy and add the Endpoint as http:// Copied Nomain Name from EC2 instance / {proxy} and Save
- API is created, Go to ACtions menu and click on Deploy API (It can be deployed as staging/ production etc by adding the stage name and clicking on Deploy
- Once we deploy, we can see the Invoke URL, we can use this URL as the endpoint to get the advertisements

todo 43 44 45

## Securing public API with JWT

Whan we deploy a microservice in Public subnet (i.e a client can directly access the microservice without API Gateway) and we need to perform authentication on the microservice itself, we can use Json Web Token (JWT) Authentication to validate the token we receive from client.

**Workflow**

Client logins to AWS cognito, Cognito sends Token to the  Client. Client makes a call to the microservice with the token, microservice validates the token using Cognito send sends the result back to the client.

todo - 46(3:00) 47



## API Documentation Microservice Discovery

To create a dynamic documentation, we use **Swagger**. To use it in Advert.API Nuget Packages **Swashbuckle.Aspnetcore** has been used and in Startup.cs we add the following:

      services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Web Advertisement Apis",
                    Version = "version 1",
                    Contact = new OpenApiContact
                    {
                        Name = "Adrita Sharma",
                        Email = "adritasharma@gmail.com"
                    }
                });
      });
      
      
       app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
       });
       

When the microservice is built, it will provide SDK. It contains models that clients can directly access. Tools like Autorest and SwaggerGen can be used generates client libraries for accessing RESTful web services.

To create client models, Open .NET Core Project in Visual Studio -> Right click on Project > Click Add -> Choose Rest API Client -> Choose Swagger URL is the Metadata File -> Add the swagger url (eg: https://localhost:44364/swagger/v1/swagger.json). This will generate the models.


## Microservice Discovery

In a microservices application, the set of running service instances changes dynamically. Instances have dynamically assigned network locations. Consequently, in order for a client to make a request to a service it must use a service‑discovery mechanism. A key part of service discovery is the service registry.

- We can setup our own Service Discovery infastructure using Consul tool. It is cloud independent, can be setup in AWS, Azure etc.
- We can also use managed AWS service - **AWS Cloud Map**, We have used this in our infastructure


**AWS Console Steps**

- Go to Service -> **AWS Cloud Map** 
- Create namespace (this is the container for all our services) (eg: web-advertisement) -> Choose API calls for instance discovery
- Click on the namespace created and click on Create a service. Each service refers to a microservice.
- Add service name (eg: advertapi), choose Route 53 health check, add any number for failure threshold (eg: 3) and add /heath as Health check Path and click on create service

We can register our service instance (eg ec2 instance where advertapi is deployed) when via AWS Command line tool (todo 52 middle) as well as .Net core

**Using .NET Core to register an instance of microservice**

We can use the startup.cs (Configure method) or program.cs (main method) of the microservice (advert.api) to add the code that should be executed once. AWS Nuget Packages **AWSSDK.EC2***  and **AWSSDK.ServiceDiscovery** have been used.

We have to copy the Service ID of the CloudMap namespace that we created and add it to appsettings.json

appsettings.json

    {
       "CloudMapNamespaceSeviceId": "srv-xxxxxxxxxxxxxxxx"
    }

Startup.cs

    public async Task RegisterToCloudMap()
    {
          string serviceId = Configuration.GetValue<string>("CloudMapNamespaceSeviceId");

          var instanceId = EC2InstanceMetadata.InstanceId;
          if(!string.IsNullOrEmpty(instanceId))
          {
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;

                var client = new AmazonServiceDiscoveryClient();

                await client.RegisterInstanceAsync(new RegisterInstanceRequest()
                {
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>()
                    {
                        {"AWS_INSTANCE_IPV4", ipv4 },
                        {"AWS_INSTANCE_PORT", "80" },
                    }
                });
          }
    }


We have to discover the services from our web client (#Microservice 1 - WebAdvert.Web)

AWS Nuget Packages **AWSSDK.ServiceDiscovery** is installed in WebAdvert.Web. We won't use the base address from appsettings.json, we will find it using service discovery.

    var discoveryClient = new AmazonServiceDiscoveryClient();
    var discoveryTask = discoveryClient.DiscoverInstancesAsync(
                         new DiscoverInstancesRequest()
                           {
                               NamespaceName = "web-advertisement",
                               ServiceName = "advertapi"
                           });

    discoveryTask.Wait();
    var instances = discoveryTask.Result.Instances;
    var ipv4 = instances[0].Attributes["AWS_INSTANCE_IPV4"];
    var port = instances[0].Attributes["AWS_INSTANCE_PORT"];
    
    
## CI/CD for Microservices

Continuous Integration and Delivery is necessary to achieve the agility that Microservices promise.

**Continuous Integration:** Code changes get built, tested and then merged into the main branch automatically to ensure code is always production ready.

**Continuous Delivery:** Code changes that pass CI get automatically deployed to all pre production environments (eg: dev, staging etc)

**Continuous Deployment:** Code changes that pass CI get automatically deployed to all  production environment.

Types of deployment

**Rolling Deployment:** New service instance (EC2, Lambda or Docker Containers) are launched. New version runs parallel to the old version. Old instances needs to be deleted.

**Red/Black Deployment:** Once the new version is up. 100% of traffic is redirected from old to new.

**Canary Deployment:** Service is deployed for small % of users. When tests are ok, 100% traffic is redirected to new


### Deployment in AWS

**Deployment of AWS Lambda:** 

- Use SAM (Serverless Application Model) 
- Use AWS Cloud Formation to create SNS topic and attach them to Lambda.
- Use Powershell Core
- Use AWS CLI - We can run commands to build the objects and deploy code

**Deployment of ASP.NET Core Web API:** 

-  Use AWS Cloud Formation. It can launch new EC2 instances for each deployment and implement rolling deployments.
-  Use AWS Code Deploy. It is easy to implement using AWS CLI or powershell. Code deployment agent must be installed on each EC2 instance.
-  Use Docker and AWS ECS, we can build using containers and don't need to install tools on servers. Amazon ECS manages containers and their security, scaling etc.

### Deployment with Docker 
Docker is used to package the code artifact, all related files and operating system in a Docker Image. A container is an instance of Image. AWS ECS runs and manages containers

**Deployment Models**

- Fargate: Containers are fully managed by AWS.
- EC2: EC2 instances will be created to host the containers. We can manage clustering, auto scaling etc

**Elastic Container Service (ECS)**

![image](https://user-images.githubusercontent.com/29271635/122956791-f2fea400-d39e-11eb-916e-e2473261d399.png)



- **Container ->** A container is an instance of Image. Amazon pulls the image from Repository (Elastic Container Repository - it is like docker hub) and then create instances of it which are containers.

- **Task ->** Task is definition of how a container should be created, managed, how much memory to give to the container etc.

- **Service ->** It is a the microservice (like advert.api or search.api). It can have 1 or more task running inside it. Every service can go to 1 EC2 machine (if we use EC2 model).

-  **Cluster ->** It is the cluster of EC2 machines. It has the VPC information, auto scaling attached to it.


### Uploading Search.API to ECS using Docker image

First we need to create a DockerFile. Right click on the project from Visual Studio -> Add -> Add Docket Support. Docker file will be ready.
Then we have to prepare AWS Console. We bbed a user with permission for ECS

**AWS Console Steps**

- Go to Service -> **IAM** 
- Create a user (eg: ECSRunner) -> Attach policies **AWSCodeDeployRoleForECS** and **AmazonECSTaskExecutionRolePolicy**
- Go to Service -> **Amazon Elastic Container Service (ECS)** 
- Go to Repositories under Amazon ECR
- Create a repository and provide a name (ex: searchapirepo)
- Go to Permissions under Amazon ECR - Repositories. > Edit Permissions -> Add Statement -> Choose Allow -> Select IAM Entity created (eg: ECSRunner)
- Choose the actions :ecr:CompleteLayerUpload, ecr:InitiateLayerUpload, ecr:PutImage, ecr:UploadLayerPart and Save
- We can get the publish commands by clicking on View Push commands.


To use command, we have to install AWS Cli first. We will use the Docker file created to build the image and upload it to the AWS ECS repo created.

- Login :  docker login --username AWS --password-stdin xxxxxxxxxx.dkr.ecr.us-xxxx-1.amazonaws.com
- Build: docker build -t searchapirepo .
- Tag: docker tag searchapirepo:latest xxxxxxxxxxx.dkr.ecr.us-xxxx-1.amazonaws.com/searchapirepo:latest
- Push to repo: docker push xxxxxxxxxxxxxx.dkr.ecr.us-xxxx-1.amazonaws.com/searchapirepo:latest


### Creating Task Definition

- Click on Create new Task Definition and choose EC2
- Provide Task Definition (eg : TaskSeachApi), Task memory (eg: 2GB), Task CPU (eg: 2 vCPU)
- Click on Add Container -> Give a name (eg:SearchAPIContainer), Copt the earlier created Image URI to the Image (eg: xxxxxxxxxxxx.dkr.ecr.us-xxxx-1.amazonaws.com/searchapirepo)
- Set memory limit  - Hard as 1024,  For port mapping add 80 -> 5000 and click add container
- Create and the Task Definition is ready.


### Creating Cluster

- Go to Clusters -> Create Cluster -> Choose EC2 Linux + Networking 1
- Add Cluster name - eg: AdvertCluster, Choose On-Demand Instance, Choose EC2 instance type as m5zn.large, choose Number of instances as 1
- For Key pair, go to  EC2 console  and generate a new Key pair (give a nax - ECS for ex) and choose that.
- For Networking, choose existing vPC from the dropdown. ( Note: We can also Create a new VPC it has to habe internet access. For that, we have to add internet gateway to it, and in mapping table, map our internet gateway to all traffic to all IPs)
- Choose a existing subnet and security group. (Note: for existing security group - We have to go to  security group section, check inbound and outboud rules. For inbound rukes - eg: we have port 80 -> 5000, we have to open port 80)
- For Container instance IAM role -> choose Create new role
- Once cluster is ready, we have to create a service


### Creating Service

- Go to the created Cluster, under Service Tab - click on Create
- Choose Launch type as EC2, choose the Task Definition and Cluster that we created earlier, provide a Service name (SearchAPIservice)
- Choose Service type as Replica and Number of tasks as 1/2 and Deployment type as Rolling
- We will select Load balancer as none, but we can also create one and attach our service to it
- For demo project, choose Service Auto Scaling Do not adjust the service’s desired count.
- Go to Service -> EC2. We can see that 1 instance is running, we can use it's Public IPv4 DNS :5000 to make a call (eg: https://ec2-35-175-205-14.compute-1.amazonaws.com:5000/search/v1/abc)


## Event Driven Microservice

- **Events** : Notification (message) sent from 1 Microservice to another Microservice to inform something has happened.
- **Command** : A message (command) that is sent from 1 microservice to another Microservice to instruct to do something.

