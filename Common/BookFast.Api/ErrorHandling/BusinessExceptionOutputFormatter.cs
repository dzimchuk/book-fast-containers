﻿using BookFast.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookFast.Api.ErrorHandling
{
    internal class BusinessExceptionOutputFormatter : TextOutputFormatter
    {
        public BusinessExceptionOutputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add("application/json");
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(BusinessException).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var options = context.HttpContext.RequestServices.GetService(typeof(IOptions<JsonOptions>)) as IOptions<JsonOptions>;

            var exception = (BusinessException)context.Object;
            var payload = JsonSerializer.Serialize(new { errors = exception.Errors }, options.Value.JsonSerializerOptions);

            return context.HttpContext.Response.WriteAsync(payload);
        }
    }
}
