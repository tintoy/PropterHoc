# Propter Hoc

A causality toolkit for .NET Core / ASP.NET MVC Core.

Includes support for logical-activity correlation across multiple tiers / network hops.

Activity-correlation services are available via `ActivityCorrelationManager.Current` and / or (if using MVC) a request-scoped instance of 'ActivityCorrelationManager' that can be injected as required.
For MVC, the middleware sends and receives a header with the current activity Id.
If the incoming request does not have an activity Id in the headers, it will fall back to the current ASP.NET request Id (if present). It also adds a log context with the current activity Id for the lifetime of the request.

Still a work in progress.

