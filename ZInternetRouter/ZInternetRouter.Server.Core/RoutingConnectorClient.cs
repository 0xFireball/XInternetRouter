﻿using System;

namespace ZInternetRouter.Server.Core
{
    public class RoutingConnectorClient
    {
        #region Public Fields

        public RouteRequestProcessor RequestProcessor { get; set; }

        public string MemberId = Guid.NewGuid().ToString("N");

        #endregion Public Fields
    }
}