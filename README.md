# Personal Document Management System(PDMS)

Personal Document Management system is a web-based application to store, manage and download various documents.It has been implemented using .Net Core framework using C#.The codebase has been implemented using a clean architecture.

#Authentication

For authentication purposes,PDMS is integrated with Auth0.
To establish a connection with Auth0 , valid credentials will have to be populated in appsettings.json in Auth0 section.
Forgot Password feature has been added using gmailservice to send verification link. Credentials need to be updated in  Auth0ManagementApi section.

#FileManagement

Nimbus Broker has been used to manage the storage of documents.

#Frontend 

Front end has been implemented using javascript,html/css(bootstrap).

#TestCoverage

Unit Testing has been done to test each features using X unit and fluent assertions.

Preview: 
Login Screen :

![LoginScreen](https://user-images.githubusercontent.com/16679672/155877403-98364c58-5b6c-47cf-a70a-42f1544ab8fd.JPG)

Registration Screen:
![Registration](https://user-images.githubusercontent.com/16679672/155877460-4054b081-e762-437f-8bd2-6032ae548f6f.JPG)

Dashboard:
![Dashboard](https://user-images.githubusercontent.com/16679672/155877467-ed762ee9-afb7-4ac5-9576-66a03d6b08fa.JPG)

Upload Document:
![UploadFile](https://user-images.githubusercontent.com/16679672/155877496-d4048308-21ae-4d69-96dc-b60a55c92bc0.JPG)


DocumentList:
![Document_List](https://user-images.githubusercontent.com/16679672/155877481-32b1aaf8-f5fd-4e0c-9b62-eda5cb6891a5.JPG)

