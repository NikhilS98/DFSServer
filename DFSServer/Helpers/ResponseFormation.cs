using DFSUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Helpers
{
    public static class ResponseFormation
    {
        public static Response GetResponse()
        {
            Response response = new Response
            {
                IsSuccess = false
            };

            return response;
        }
    }
}
