# AWS-Web-Advert
Getting started with AWS. This project has been created as a practice along with a <a href="https://www.udemy.com/course/build-microservices-with-aspnet-core-amazon-web-services/">Udemy Course </a>

### Project Infastructure

The VPC (Private infastructure) is divided into 2 subnets.

- **Private Subnet** - (Not accessible from internet)
   This includes Microservice (Advert.API), Dynamo DB, Elastic Search (Search.API & Search.Worker-AWS Lamda ), SNS (Messaging)
    

- **Public Subnet** - (Accessible from internet)
     This includes AWS Cognito (Signup + Auth), S3 Bucket (Upload Image), CloudFront (Display Image - Caching image to user region ), Web UI (Cluster),


Advert API is placed in private subnet. It is not neded to be accessed from internet directly. Users can login through AWS Cognito (placed in public subnet)

