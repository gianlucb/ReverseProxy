# REVERSE PROXY ON KUBERNETS PoC

Sample PoC of a ReverseProxy service for Kubernetes.
The service is written with ASP.NET Core C#, based on a custom Middleware.
I implemented two LoadBalancing rules (RANDOM + ROUNDROBIN)

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
```

The chart creates a LoadBalancer service so after the install we just need for the public IP to get assigned

```cmd
kubectl get services
```

**_Actually on my kubernetes installation seems the LoadBalancer is not working properly and is not forwarding the requests to the pod (that is running fine. confirmed with local port forwarding). I think is a limitation of my current setup. I do not have access to a real kubernetes cluster_**

To delete after the use

```cmd
helm delete --purge rp
```

## TESTS

The code contains **Unit tests** that can be executed after the build and test basic functionality.
I would also do a sort of **integration tests** to run after the deployment.
These can be done with several tools or script and should be automatized, following two very basic tests using powershell

```pshell
$body = "{a:'b'}"
wget -Method GET X.X.X.X -Headers @{Host='my-service.my-company.com'}
wget -Method POST X.X.X.X -Body $body -Headers @{Host='my-service2.my-company.com'}
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
I would add a tracing mechanism (like application insigths or any other tracing software) to log every incoming request and the response of the proxy.
This can be calculated in near real-time if the log are for example sent to an ElasticSearch cluster (+ kibana)
