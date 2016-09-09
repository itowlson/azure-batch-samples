## Azure Batch Unit Test Helper Library

The **Azure Batch Unit Test Helper Library** provides methods for faking the Azure Batch service, so that classes and methods that call Azure Batch can be unit tested in isolation from the service.

### Usage

The library provides helpers for the following scenarios:

  * You want to verify that your code makes a particular call to the Batch service, perhaps with specific parameter values. **Example:** You have a method which creates a pool if that pool doesn't already exist, and want to test that the 'add pool' request is sent with the correct pool id.
  * Your code makes a call to the Batch service, and you want to simulate the service returning specific test data. **Example:** You have a method which makes use of the number of idle compute nodes in a pool to decide whether to schedule more work, and you want to simulate different returns from a 'list nodes' request. 
  * Your code makes a call to the Batch service, and you want to simulate the service returning a specific error response. **Example:** You have a method which creates a pool if that pool doesn't already exist, and you want to simulate a 'pool not found' response from the 'get pool' request.
  * You want to verify that your code does _not_ make a particular call to the Batch service. **Example:** You have a method which creates a pool if that pool doesn't already exist, and you want to test that no 'add pool' request is sent if the pool _does_ exist. 

In all of these cases, you use the library by calling the `BatchClient.OnRequest<TRequest>` extension method to set expectations.  (The same extension method is also provided on `IInheritedBehaviors` which allows you to perform the same faking on methods or classes that take specific objects such as `CloudJob` or operations objects such as `JobOperations`.)  `OnRequest` takes an `Action<TRequest>` - this can be a custom action, or you can use one of the convenience actions for the scenarios above.  These convenience actions are defined as extension methods on `BatchRequest`.

  * To verify that your code makes a particular call to the Batch service, and the parameters with which it makes that call, use the `Capture` extension method. This captures the values you ask for into a list. You can then make assertions about that list to check that the number of calls, and their parameters, is what you expect.
  * To simulate the service returning specific test data, use the `Return` extension method.
  * To simulate the service returning an error response, use the `Error` extension method. This method addresses the common case where your testee code cares only about the HTTP status code and Batch error code; for more demanding cases where your testee code cares about error response messages or values, you can fall back to the more basic `Throw` extension method.
  * To verify that your code does _not_ make a particular call to the Batch service, use the `Unexpected` extension method. This method throws the custom `UnexpectedRequestException` if it receives a matching request.
  * If none of the built-in actions meet your needs, use a custom action. (And let us know or send a pull request so we can make the library better.)

The usage pattern is as follows:

  * Create an instance of `BatchClient` (or a Batch object such as `CloudJob`) to pass to the class or method under test.
  * Call `OnRequest<TRequest>` for each request type that you expect the testee to issue.
  * Invoke the testee.
  * Make any required assertions about the results of the testee or its interactions with the Batch service.

**NOTE:** If you create the Batch client _within_ the class or method under test, the library can't help you. Don't do this. Always pass the Batch client (or whatever Batch object you need to work with) into the testee, either via the method arguments or via a constructor. This allows you to set simulated behaviour on the object from a test, while not affecting the behavior in production code.

For example (using the Xunit unit testing framework to implement a test):

```
[Fact]
public async Task EnsureCapacityMethod_IfPoolDoesNotExistThenItIsCreated()
{
    var createdPools = new List<string>();

    // Create an instance of BatchClient to pass to the class or method under test
    using (BatchClient batchClient = BatchResourceFactory.CreateBatchClient())
    {
        // Call OnRequest for each request type that you expect the testee to issue.
        // In this case, we expect the testee to perform a 'get pool' to see if the pool
        // already exists, followed by an 'add pool' to create it.
        batchClient.OnRequest<PoolGetBatchRequest>(r => r.Error(HttpStatusCode.NotFound, BatchErrorCodeStrings.PoolNotFound));
        batchClient.OnRequest<PoolAddBatchRequest>(r => r.Capture(r.Parameters.Id, createdPools));

        // Invoke the testee
        var poolCoordinator = new PoolCoordinator(batchClient);  // Injecting the BatchClient into the testee via the constructor
        await poolCoordinator.EnsureCapacity("new-pool", "A2", 40);

        // Make any required assertions about the results of the testee (none in this case)
        // or its interactions with the Batch service (in this case, we expect a single 'add pool'
        // request with a pool id of "new-pool").
        Assert.Equal(1, createdPools.Count);
        Assert.Equal("new-pool", createdPools.Single());
    }
}

```

The helper library also includes various utility classes and methods:

  * `BatchResourceFactory.CreateBatchClient` creates a Batch client which will error if it ever attempts to connect to the service. You don't have to use this, but it saves you from having to provide credentials, and ensures that you get a meaningful error if your code issues a Batch request for which you did not provide a handler in your test setup.
  * `DataPage.Empty` and `DataPage.Single` return pages of data to a Batch 'list' request. `Empty` returns an empty list and is useful mainly when you want to capture a request parameter and don't care about the result. `Single` returns the contents of an `IEnumerable` as a single list response. (The library doesn't currently provide a helper for a multi-page response, but you won't need that unless you need to test page traversal code.)

### Implementation

The unit testing helper library uses the Batch .NET client interceptor framework. In production scenarios this is used to customize the request processing pipeline, but it can also be used to inject actions and responses directly into the pipeline, overriding the default behavior of calling the Azure Batch REST API. The helper library sets up interceptor behaviors at the BatchClient level so that you do not need to inject the behaviors into your classes or methods. 

The translation between the `OnRequest` extension methods and the Batch interceptor framework is carried out by the `ServiceResponseSimulator` class. This is a low-level implementation class and you won't usually need to work with it directly.
