# Setting up Real-Time Serverless App

## Goals
1. Learn more about serverless architecture
2. Become more fluent in CSharp .net core
3. Build a website that I can continuously build upon and showcase learing and random musings.

## Tech-stack
- [dotnet core](https://dotnet.microsoft.com/)
- [Blazor Web Assembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [AWS Api-Gatway](https://docs.aws.amazon.com/apigateway/latest/developerguide/welcome.html)
- [AWS Lambda](https://docs.aws.amazon.com/lambda/index.html)
- [AWS dynamodb](https://docs.aws.amazon.com/dynamodb/)
- [AWS S3](https://docs.aws.amazon.com/s3/index.html)
- [CSharp](https://docs.microsoft.com/en-us/dotnet/csharp/)

## Build Steps (not-automated) 
*These steps are specific to getting things up and running in a WSL ubuntu env*
1. Install .net core sdk 
Make sure */etc/resolv.conf* is not corrupted. I had issues with dns. Delete and add
```bash
nameserver 8.8.8.8
nameserver 8.8.4.4
```
```bash
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1
```
2. Create WASM Blazor project
```bash
dotnet new blazorwasm -o app
cd app
dotnet run
```
3. Install aws cli tools
```bash
sudo apt install unzip
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "/tmp/awscliv2.zip"
unzip /tmp/awscliv2.zip
sudo ./aws/install
```
4. Create AWS Cli Credential File
```bash
cd ./aws/ 
touch credentials
code credentials
```
Add the following to credential file
```bash
[default]
aws_access_key_id=MYACCESSKEYHERE
aws_secret_access_key=MYSUPERSECRETACCESSKEYHERE
```
5. Create S3 Bucket to host static site
```bash
aws s3api create-bucket --bucket elsermusings --region us-east-1
```
6. Make S3 Bucket Host Static Site
```bash
aws s3 website s3://elsermusings/ --index-document index.html --error-document index.html
```
7. Move published directory to S3 bucket
After publishing using
```bash
dotnet publish
```
The newly compiled WASM app is somewhere under
```bash
~/elsermusings/app/bin/Debug/netstandard2.1/publish/wwwroot
```
We want to copy all contents from wwwroot to our static hosting S3 bucket setting their acl permissions to public-read
```bash
aws s3 cp --recursive --acl public-read wwwroot s3://elsermusings 
```