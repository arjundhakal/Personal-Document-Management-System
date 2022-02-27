# Personal Document Management System(PDMS)

Personal Document Management system is a web-based application to store, manage and download various documents.It has been implemented using .Net Core framework using C#.The codebase has been implemented using a clean architecture.

##Authentication
For authentication purposes,PDMS is integrated with Auth0.
To establish a connection with Auth0 , valid credentials will have to be populated in appsettings.json in Auth0 section.
Forgot Password feature has been added using gmailservice to send verification link. Credentials need to be updated in  Auth0ManagementApi section.

###FileManagement
Nimbus Broker has been used to manage the storage of documents.

####Frontend 
Front end has been implemented using javascript,html/css(bootstrap).

#####TestCoverage
Unit Testing has been done to test each features using X unit and fluent assertions.
