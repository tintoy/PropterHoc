# Propter Hoc

Logical-activity correlation support for .NET Core / ASP.NET MVC Core.

Activity-correlation services are available via `ActivityCorrelationManager.Current` and / or (if using MVC) a request-scoped instance of 'ActivityCorrelationManager' that can be injected as required.
For MVC, the middleware sends and receives a header with the current activity Id.
If the incoming request does not have an activity Id in the headers, it will fall back to the current ASP.NET request Id (if present).

Still a work in progress.

TODO: A mechanism (probably a log decorator) to inject the logical activity Id into log entries.
