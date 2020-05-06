using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using DFSUtility;

namespace DFSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task requestAcceptor = Task.Run(() => AcceptRequest());
            Task requestProcessor = Task.Run(() => ProcessRequestFromQueue());
            Task.WaitAll(new Task[] { requestAcceptor, requestProcessor });
        }

        static void AcceptRequest()
        {
            RequestListener listener = new RequestListener();
            listener.Listen(100);

            while (true)
            {
                Console.WriteLine("Waiting");
                //this is blocking
                listener.Accept();
            }
        }

        static void ProcessRequestFromQueue()
        {
            while (true)
            {
                var request = MessageQueue.Dequeue();
                if (request != null)
                {
                    var executingAssembly = Assembly.GetExecutingAssembly();
                    Type type = executingAssembly.GetTypes()
                        .FirstOrDefault(x => x.FullName.Contains(request.Request.Type));
                    var met = type.GetMethod(request.Request.Method, BindingFlags.Static | BindingFlags.Public);
                    var value = met.Invoke(null, request.Request.Parameters);

                    Response response = new Response()
                    {
                        RequestId = request.Request.Id
                    };

                    if (met.ReturnType.IsArray)
                    {
                        response.Data = (byte[])value;
                        response.Message = request.Request.Message;
                    }

                    request.Client.Send(response.SerializeToByteArray());
                }


                //var p = met.GetParameters();
                //type.InvokeMember(request.Method, BindingFlags.Static | BindingFlags.Public | 
                //BindingFlags.InvokeMethod, null, null, request.Parameters.ToArray());
            }
        }
    }
}
