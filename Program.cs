using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter DeviceId");
            string deviceId = Console.ReadLine();

            var querystringData = new Dictionary<string, string>();
            querystringData.Add("connectionType", "device");
            querystringData.Add("deviceId", deviceId);
            var connection = new HubConnection("http://relayserver.azurewebsites.net/", querystringData);
            var myHub = connection.CreateHubProxy("IOTHub");
            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }

            }).Wait();


            myHub.On<string>("onCommand", ( message) =>
            {
                Console.WriteLine("received cmd : {0}", message);
            });

            string deviceEvent = "";
            do
            {
                Console.WriteLine("Enter Event to Hub (type close to disconnect) :");
                deviceEvent = Console.ReadLine();
                myHub.Invoke<string>("onDeviceEvent", deviceId, deviceEvent).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error calling send: {0}",
                                          task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine(task.Result);
                    }
                });

            } while (!deviceEvent.Equals("close"));

            connection.Stop();
        }
    }
}