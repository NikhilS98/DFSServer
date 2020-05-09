using DFSServer.Communication;
using DFSUtility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DFSServer
{
    public static class ServerResponseList
    {
        private static ConcurrentDictionary<Guid, Response> responses = new ConcurrentDictionary<Guid, Response>();

        public static bool Add(Guid guid, Response response)
        {
            return responses.TryAdd(guid, response);
        }

        public static Response Remove(Guid guid)
        {
            Response response;
            responses.TryRemove(guid, out response);
            return response;
        }

        public static bool ContainsKey(Guid guid)
        {
            return responses.ContainsKey(guid);
        }

        public static int GetCount()
        {
            return responses.Count;
        }

        public static List<Response> GetResponses()
        {
            List<Response> responseList = new List<Response>();
            foreach (var item in responses)
            {
                responseList.Add(item.Value);
            }
            return responseList;
        }
    }
}
