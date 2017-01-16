# Azure File System

Replicating file system operations on Microsoft Azure Blob storage.
CRUD on files and folders.


### Getting started

After installing [AzureBlob.FileSystem](https://www.nuget.org/packages/AzureBlob.FileSystem), check the app.config/web.config file.
Replace `AbFsStorageAccountConnectionString` and `AbFsContainerName` with the correct values for the Azure storage used.
Also, for using AzureCdn update values for `AbFsAzureTenantId`, `AbFsAzureUserId`, `AbFsAzureUserKey`, `AbFsAzureSubscriptionId`,
`AbFsAzureCdnResourceGroupName`, `AbFsAzureCdnProfileName` and `AbFsAzureCdnEndpointName`.

Dependencies need to be registered in the dependency injection container.
Following registrations are needed (SimpleInjector syntax used).

```
container.Register<IAzureStorageProvider, AzureStorageProvider>();
container.Register<IAzureBlobItemService, AzureBlobItemService>();
container.Register<IAzureOAuthProvider, AzureOAuthProvider>();
container.Register<IAzureCdnService, AzureCdnService>();
container.Register<IAuthenticationConfiguration, ConfigurationService>();
container.Register<IAzureCdnConfiguration, ConfigurationService>();
container.Register<IAzureStorageConfiguration, ConfigurationService>();
container.Register<IBlobMetadataService, BlobMetadataService>();
container.Register<IBusinessConfiguration, ConfigurationService>();
container.Register<IFileInfoService, FileInfoService>();
container.Register<IFolderInfoService, FolderInfoService>();
container.Register<IPathValidationService, PathValidationService>();
container.Register<IStorageFileService, StorageFileService>();
container.Register<IStorageFolderService, StorageFolderService>();
```

Library main points: 

```
IStorageFileService
IStorageFolderService
```


### Usage

The utility of this library is handling blob files similar to file system.
This comes handy as working with directories is not available in azure blob storage.
Basic operations have been created to work with files and folders.

Using these simple operations further complex operations can be achived. 
For example moving or renaming a file or folder can be mimicked by using copy and delete.