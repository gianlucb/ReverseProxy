using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReverseProxy
{
    public class ReverseMiddleware
    {
        // Middleware documentations for .NET Core
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-2.2

        // for performance reasons the httpclient is 1 and static (avoid resources exhaustion)
        private static readonly HttpClient _httpClient = new HttpClient();

        // needed to continue the pipeline in case we do not handle the request
        private readonly RequestDelegate _next;

        // just one for the entire application
        private static ITracer _tracer;


        public ILoadBalancer LoadBalancer { get; set; }

        public ReverseMiddleware(RequestDelegate nextMiddleware, ILoadBalancer loadBalancer)
        {
            _next = nextMiddleware;
            if (_tracer == null)
                _tracer = new LogFileTracer();

            LoadBalancer = loadBalancer;

            //extend the default number of outstanding http calls
            ServicePointManager.DefaultConnectionLimit = 150;
        }

        //method called by the middleware pipeline
        public async Task Invoke(HttpContext context)
        {
            var targetServiceUri = GetTargetServiceUri(context.Request);

            if (targetServiceUri != null)
            {
                bool callResult = false;

                // build a new request - copy the original one
                var targetRequestMessage = new HttpRequestMessage();
                targetRequestMessage.RequestUri = targetServiceUri;
                targetRequestMessage.Headers.Host = targetServiceUri.Host;
                targetRequestMessage.Method = ParseHttpMethod(context.Request.Method);
                CopyOriginalReqContent(context.Request, targetRequestMessage);

                try
                {

                    using (var response = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        response.EnsureSuccessStatusCode();

                        //we got a correct answer here
                        context.Response.StatusCode = (int)response.StatusCode;

                        //copy any header 
                        foreach (var header in response.Headers)
                        {
                            context.Response.Headers[header.Key] = header.Value.ToArray();
                        }

                        //copy content
                        await response.Content.CopyToAsync(context.Response.Body);
                        callResult = true;
                      
                    };
                }
                catch (Exception ex)
                {
                    //wrong address or something happened on the target service
                    Trace.WriteLine(ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                  
                }
                finally
                {
                    _tracer.TraceCallAsync(targetServiceUri.AbsoluteUri, callResult);
                   
                }
                return;
            }
            else
            {
                // next hop not found
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }


            await _next(context);
        }

        private void CopyOriginalReqContent(HttpRequest req, HttpRequestMessage requestMessage)
        {
            if (!HttpMethods.IsGet(req.Method) &&
              !HttpMethods.IsHead(req.Method) &&
              !HttpMethods.IsDelete(req.Method))
            {
                // here is is a method with a BODY
                var streamContent = new StreamContent(req.Body);
                requestMessage.Content = streamContent;
            }

            // copy headers
            foreach (var header in req.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private static HttpMethod ParseHttpMethod(string method)
        {
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;

            return new HttpMethod(method);
        }

        private Uri GetTargetServiceUri(HttpRequest request)
        {
            Uri targetUri = null;

            if (request.Headers.ContainsKey("Host"))
            {
                try
                {
                    var nextHop = LoadBalancer.Services.Where((s)=>s.Key == request.Headers["Host"]).FirstOrDefault();
                    return nextHop.Value.GetNextServiceHost();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    return null;
                }
            }

            return targetUri;
        }
    }
}