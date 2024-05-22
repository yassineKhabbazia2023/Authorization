using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Polly.Retry;

namespace Pulse.Authorization.Infrastructure.Extensions
{
    public static class EntityContextExtensions
    {
        public static void HandleEFCoreFailure<T>(this T context) where T : DbContext
        {
            context.SaveChangesFailed += (s, e) =>
            {
                context.ChangeTracker.Clear();
            };
        }
    }
}
