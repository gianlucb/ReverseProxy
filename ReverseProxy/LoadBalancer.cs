using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReverseProxy
{
    public interface ILoadBalancer
    {
        Dictionary<string, Service> Services { get; set; }
    }

    public enum LoadBalancingRule
    {
        Random = 0,
        RoundRobin = 1
    }

    public class LoadBalancer : ILoadBalancer
    {      
        public Dictionary<string, Service> Services { get; set; }


        public LoadBalancer()
        {
            Services = new Dictionary<string, Service>();
        }
    }

    // this class mocks a target service
    public class Service
    {
        public String Name { get; set; }
        public String Domain { get; set; }
        public LoadBalancingRule Strategy { get; set; }
        public List<ServiceHost> Hosts { get; set; }
        public int LastUsedIndex { get; set; }
        //public LoadBalancingRule Rule { get; set; }

        public Service()
        {
            Strategy = LoadBalancingRule.Random;
        }

        public Uri GetNextServiceHost()
        {
            switch (Strategy)
            {
                case LoadBalancingRule.Random:
                    {
                        int index = new Random().Next(Hosts.Count);
                        return new Uri($"http://{Hosts[index].Address}:{Hosts[index].Port}");
                    }
                case LoadBalancingRule.RoundRobin:
                    {
                        // incase of overflow start from zero
                        if (LastUsedIndex + 1 < Hosts.Count)
                            LastUsedIndex++;
                        else
                            LastUsedIndex = 0;
                      
                        return new Uri($"http://{Hosts[LastUsedIndex].Address}:{Hosts[LastUsedIndex].Port}");
                    }
                default:
                    return null;
            }
        }
    }

    public class ServiceHost
    {
       // public Uri Uri => new Uri($"http://{Address}:{Port}");

        public String Address { get; set; }
        public int Port { get; set; }
    }


}
