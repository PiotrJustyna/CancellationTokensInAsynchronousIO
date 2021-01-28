- [readme](#readme)
  - [What are we testing](#what-are-we-testing)
  - [Method](#method)
  - [Comparison](#comparison)
    - [.NET Core 3.1](#net-core-31)
    - [.NET 5.0](#net-50)
    - [Conclusions](#conclusions)
      - [Performance](#performance)
      - [Usability](#usability)

---

# readme

Verifying .net 5's "better cancellation support" claims: https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-cancellation-support in a very non-scientific way.

## What are we testing

The aim is to verify whether `HttpClient`'s asynchronous operations are cancelled (using `CancellationToken`s) more efficiently in .NET Core 3.1 or in .NET 5.0. We are after precision here - the less time it takes from token cancellation to the asynchronous operation getting actually stopped, the better.

## Method

The test is divided into two stages:

1. Send `HttpRequestMessage` asynchronously.
2. Read response content asynchronously using `ReadAsStringAsync`.

Each test is repeated 1000 times before final stats are calculated:

* best precision
* worst precision
* average precision

Where `precision` is, again, the time it takes from the cancellation of a cancellation token to the asynchronous operation it was used in to actually stop.

The goal is to stop the chain of asynchronous operations after 200ms.

## Comparison

### .NET Core 3.1

```
...
Second operation started: True, second operation succeeded: True. Elapsed time: 65ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 70ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 63ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 63ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 69ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 63ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 77ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 64ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 63ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 61ms.
Summary:
Best precision: 0.00ms
Worst precision: 55.00ms
Average precision: 0.00ms
```

### .NET 5.0

```
...
Second operation started: True, second operation succeeded: True. Elapsed time: 84ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 71ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 65ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 67ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 75ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 63ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 96ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 68ms.
Second operation started: True, second operation succeeded: True. Elapsed time: 61ms.
Summary:
Best precision: 0.00ms
Worst precision: 47.00ms
Average precision: 0.00ms
```

### Conclusions

Given the 200ms time limit, both frameworks perform rather poorly when it comes to consistent, precise cancellations of asynchronous operations. At worst we are observing 50ms delay from the cancellation to the asynchronous operation actually getting stopped, which is ~25% of the entire time limit. The recommendation is to not rely on cancellation tokens when reliable precision is a requirement.

#### Performance

No noticeable performance improvements observed. What is provided in the Comparison section looks like .NET 5.0 has a slight edge, but those results change slightly from test to test and are actually difficult to distinguish.

#### Usability

Where .NET 5.0 shines, though, is the improved usability of asynchronous code. What it means for developers is that they can now use cancellation tokens with more asynchronous methods provided by the framework (e.g. `ReadAsStringAsync`). The ripple effect of this change is that developers are going to be less tempted to cancel or otherwise break asynchronous execution chains in non-standard ways. Adding cancellation token support to more asynchronous methods provided by the framework will make codebases look more streamlined and easier to digest in general.