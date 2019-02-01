﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public class BotMessageHandler : BotMessageHandlerBase
    {
        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = default(Activity);

            // Get the request body and deserialize to the Activity object.
            // We need to leave the stream open here so others downstream can access it.
            var originalPosition = request.Body.Position;
            request.Body.Position = 0;
            try
            {
                using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8, true, 1024, true)))
                {
                    activity = BotMessageHandlerBase.BotMessageSerializer.Deserialize<Activity>(bodyReader);
                }
            }
            finally
            {
                request.Body.Position = originalPosition;
            }

#pragma warning disable UseConfigureAwait // Use ConfigureAwait
            var invokeResponse = await adapter.ProcessActivityAsync(
                    request.Headers["Authorization"],
                    activity,
                    botCallbackHandler,
                    cancellationToken);
#pragma warning restore UseConfigureAwait // Use ConfigureAwait

            return invokeResponse;
        }
    }
}
