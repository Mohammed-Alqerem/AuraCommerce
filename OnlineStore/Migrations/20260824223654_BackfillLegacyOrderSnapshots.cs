using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Migrations
{
    /// <inheritdoc />
    public partial class BackfillLegacyOrderSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [orders]
                SET
                    [DeliveryMethod] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([orders].[DeliveryMethod])), N'') IS NULL THEN N'Not recorded'
                        ELSE [orders].[DeliveryMethod]
                    END,
                    [ShippingAddress] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([orders].[ShippingAddress])), N'') IS NULL
                            THEN N'Historical order - original delivery address unavailable'
                        ELSE [orders].[ShippingAddress]
                    END,
                    [ShippingEmail] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([orders].[ShippingEmail])), N'') IS NULL
                            THEN COALESCE(NULLIF(LTRIM(RTRIM([users].[Email])), N''), N'unknown@example.invalid')
                        ELSE [orders].[ShippingEmail]
                    END,
                    [ShippingName] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([orders].[ShippingName])), N'') IS NULL
                            THEN COALESCE(NULLIF(LTRIM(RTRIM([users].[Name])), N''), N'Historical customer')
                        ELSE [orders].[ShippingName]
                    END,
                    [ShippingPhone] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([orders].[ShippingPhone])), N'') IS NULL
                            THEN COALESCE([users].[Phone], N'')
                        ELSE [orders].[ShippingPhone]
                    END,
                    [Subtotal] = CASE WHEN [orders].[Subtotal] = 0 THEN [orders].[TotalPrice] ELSE [orders].[Subtotal] END
                FROM [Orders] AS [orders]
                LEFT JOIN [Users] AS [users] ON [users].[Id] = [orders].[UserId]
                WHERE
                    NULLIF(LTRIM(RTRIM([orders].[DeliveryMethod])), N'') IS NULL OR
                    NULLIF(LTRIM(RTRIM([orders].[ShippingAddress])), N'') IS NULL OR
                    NULLIF(LTRIM(RTRIM([orders].[ShippingEmail])), N'') IS NULL OR
                    NULLIF(LTRIM(RTRIM([orders].[ShippingName])), N'') IS NULL OR
                    [orders].[Subtotal] = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Historical snapshot values cannot be safely restored to missing values.
        }
    }
}
