using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace RepTrackData.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            return propertyBuilder.HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { }),
                v => JsonSerializer.Deserialize<T>(v, new JsonSerializerOptions { }) ?? default!);
        }
    }
}
