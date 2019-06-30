# REVERSE PROXY ON KUBERNETES PoC

Sample PoC of a ReverseProxy service for Kubernetes.
The service is written with ASP.NET Core C#, based on a custom Middleware.
I implemented two Load Balancing rules (_RANDOM_ + _ROUNDROBIN_)

## BUILD + UNIT TESTS

```cmd
dotnet restore
dotnet build
dotnet test
```

## KUBERNETES DEPLOY

I already published the image to DockerHub (using Visual studio).
To install the chart simply go to the main folder and execute

```cmd
cd ReverseProxy\charts
helm install ./reverseproxy --name=rp
kubectl port-forward POD_NAME 8080:8080
```

To delete after the use

```cmd
helm delete --purge rp
```

## TESTS

The code contains **Unit tests** that can be executed after the build and test basic functionality.
I would also do a sort of **integration tests** to run after the deployment.
These can be done with several tools or script and should be automatized (DevOps, Load Tests + Integration tests.)
Following two very basic tests using powershell

```pshell
$body = "{a:'b'}"
wget -Method GET X.X.X.X:8080 -Headers @{Host='my-service.my-company.com'}
wget -Method POST X.X.X.X:8080 -Body $body -Headers @{Host='my-service2.my-company.com'}
```

## SLI

Couple of Service Level Indicators I might consider:

| SLI                   | How                                                                                              |
| --------------------- | ------------------------------------------------------------------------------------------------ |
| Response Time         | Time taken to get the response to the client (this include the time taken by the target service) |
| Proxy Time            | Time taken for the proxy to forward the call to the target                                       |
| Succeded Calls %      | Percentuage of succeded calls when the host header is correct                                    |
| Failed Calls %        | Percentuage of failed calls when the host header is correct                                      |
| Availability Uptime % | Percentuage of uptime of the proxy service (since start)                                         |

For example the _Succeded Calls %_ can be easily calculated if for every call we trace the operation.
I would add a tracing mechanism (like application insigths of Azure or any other tracing software) to log every incoming request and the response of the proxy.
This can be calculated in near real-time if the log are for example sent to an ElasticSearch cluster (+ kibana).

In this PoC I put a simple "call tracer" to a local log file in a _csv_ format to be able to parse it later

```csv
6/30/2019 3:51:51 PM,http://scooterlabs.com/echo?foo2=bar2:80,True
6/30/2019 3:51:53 PM,http://scooterlabs.com/echo?foo1=bar1:80,True
6/30/2019 3:52:02 PM,http://scooterlabs.com/echo?foo4=bar4:80,False
6/30/2019 3:52:06 PM,http://scooterlabs.com/echo?foo3=bar3:80,True
6/30/2019 3:52:15 PM,http://scooterlabs.com/echo?foo4=bar4:80,True
6/30/2019 3:52:32 PM,http://scooterlabs.com/echo?foo4=bar4:80,False
```

This allows at the same time to calculate _Succeded Calls %_ and _Failed Calls %_
