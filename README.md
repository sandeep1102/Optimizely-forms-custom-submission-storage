# Optimizely-forms-custom-submission-storage
This repository only ave classes that were used to integrate custom storage system for Optimizely forms
Optimizely Forms uses DDS as its default storage mechanism. Which may cause performance-related issues in special cases. To solve this performance-related issue we can create an alternate data storage mechanism using MSSQL data storage system. We can use any data storage system and also there is an article on how to use MongoDb as alternate storage mechanism, but sometimes we dont need an additional database for storage and want to use the existing one, In that case we can use MSSQL data storage system.

MSSQL data storage system will be a lot faster than default storage mechanism (DDS). The data that we need to store in tables is of such a type that NoSQL data storage system is best suited for it, but in some circumstances many developers are bound to use Microsoft SQL Server (MSSQL) for some reasons such as saving cost of having an additional database.

Here are step by step guide for how to use MSSQL as alternate data storage system for storing Optimizely Forms submission data:

Step 1:


Add NuGet package :-

EPiServer.Forms.Core

Step 2: 

Create a class which implements or say inherits PermanentStorage class, add ServiceConfiguration attribute on class. I have chosen the name MsSqlPermanentStorage in the exaple below for class, you can choose any name.

Step 3:


Override the five functions below.

SaveToStorage(FormIdentity formIden, Submission submission). This function saves data at the first step of a form. This may also be the last step, if a form only has one step. Using this method, based on these two parameters, you can save data into database::
formIden. Contains form information, such as form id or language.
submission. Contains form submission data.
UpdateToStorage(Guid formSubmissionId, FormIdentity formIden, Submission submission). This function is for saving data at the next steps or updating data at previous ones. Within this method, based on these two parameters, you can update data in the database.
submission. Contains form submission data.
formIden. Contains form information, such as form id or language.
LoadSubmissionFromStorage(FormIdentity formIden, DateTime beginDate, DateTime endDate, bool finalizedOnly = false). This function loads form submission data from the database within a specific time period and the status finalized.
LoadSubmissionFromStorage(FormIdentity formIden, string[] submissionIds). Loads form submission data only based on the ids of the submission.
Delete(FormIdentity formIden, string submissionId). Deletes a form submission based on submission Id.

Note: - ADO.Net is used for communication between C# and database. Which is faster than any ORM because at base every object relational mapper uses ADO.Net.

Step 4:

Configuring new database provider:-

Now at last we need to add this custom storage provider in Forms.config and default provider is changed from DdsPermanentStorage to MsSqlPermanentStorage or whatever name you have given to the provider, according to the example code I used name MsSqlPermanentStorage.

<storage defaultProvider="MsSqlPermanentStorage">
            <providers>
	            <add name="MsSqlPermanentStorage" type="CustomStorage.Web.React.MsSqlPermanentStorage, CustomStorage.Web.React" />
				<add name="DdsPermanentStorage" type="EPiServer.Forms.Core.Data.DdsPermanentStorage, EPiServer.Forms.Core" />
              
