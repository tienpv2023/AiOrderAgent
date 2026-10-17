#pragma warning disable OPENAI001

using AiOrderAgent.Tools;
using OpenAI.Responses;
using System.Text.Json;

namespace AiOrderAgent.Services;

public class AiAgentService
{
    private readonly ResponsesClient _client;
    private readonly GetOrderStatusTool _orderTool;

    public AiAgentService(
        ResponsesClient client,
        GetOrderStatusTool orderTool)
    {
        _client = client;
        _orderTool = orderTool;
    }

    public async Task<string> AskAsync(string userMessage)
    {
        var inputItems = new List<ResponseItem>
        {
            ResponseItem.CreateSystemMessageItem("""
                You are an order assistant.

                You can use GetOrderStatus when the user asks about an order.

                Never invent order information.
                Only answer using information returned by the tool.
                """),

            ResponseItem.CreateUserMessageItem(userMessage)
        };

        var tool = ResponseTool.CreateFunctionTool(
            functionName: "GetOrderStatus",
            functionDescription: "Get the status and information of an order",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "orderId": {
                        "type": "integer",
                        "description": "The order ID"
                    }
                },
                "required": ["orderId"],
                "additionalProperties": false
            }
            """u8.ToArray()),
            strictModeEnabled: true
        );

        while (true)
        {
            var options = new CreateResponseOptions(
                "gpt-5.2",
                inputItems)
            {
                Tools = { tool }
            };

            var response =
                await _client.CreateResponseAsync(options);

           // inputItems.AddRange(response.OutputItems);
            inputItems.AddRange(response.Value.OutputItems);

            var functionCall =
                response.Value.OutputItems
                    .OfType<FunctionCallResponseItem>()
                    .FirstOrDefault();

            if (functionCall == null)
            {
                return response.Value.GetOutputText();
            }

            if (functionCall.FunctionName == "GetOrderStatus")
            {
                var result =
                    await _orderTool.ExecuteAsync(
                        functionCall.FunctionArguments.ToString());

                inputItems.Add(
                    new FunctionCallOutputResponseItem(
                        functionCall.CallId,
                        result));
            }
        }
    }
}