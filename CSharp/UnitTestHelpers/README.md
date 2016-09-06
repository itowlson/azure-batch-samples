## Azure Batch Unit Test Helper Library

The **Azure Batch Unit Test Helper Library** provides methods for faking the Azure Batch service, so that classes and methods that call Azure Batch can be unit tested in isolation from the service.

### Implementation

The unit testing helper library uses the Batch .NET client interceptor framework. In production scenarios this is used to customize the request processing pipeline, but it can also be used to inject actions and responses directly into the pipeline, overriding the default behavior of calling the Azure Batch REST API. The helper library sets up interceptor behaviors at the BatchClient level so that you do not need to inject the behaviors into your classes or methods. 
