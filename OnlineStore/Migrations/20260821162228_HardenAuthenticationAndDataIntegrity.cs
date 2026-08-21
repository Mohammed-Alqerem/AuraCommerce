using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Migrations
{
    /// <inheritdoc />
    public partial class HardenAuthenticationAndDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE [Users] SET [NormalizedEmail] = UPPER(LTRIM(RTRIM([Email])));");

            migrationBuilder.Sql(
                "UPDATE [OrderItems] SET [ProductName] = [Products].[Name] " +
                "FROM [OrderItems] INNER JOIN [Products] ON [OrderItems].[ProductId] = [Products].[Id];");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProductName",
                value: "Wireless Mouse");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProductName",
                value: "Mechanical Keyboard");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProductName",
                value: "Smart Watch");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProductName",
                value: "Casual Sneakers");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "ProductName",
                value: "Running Shoes");

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 6,
                column: "ProductName",
                value: "Smart Watch");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NormalizedEmail", "Password", "Role" },
                values: new object[] { "MOHAMMED@GMAIL.COM", "AQAAAAIAAYagAAAAEJ2wyfnuNORBZAX4OEmJZkWIHaa1yGwVQ+NWaLrSxeM9AZhXq2N10Si1w5TV2sjqkA==", "Admin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NormalizedEmail", "Password", "Role" },
                values: new object[] { "AHMAD@GMAIL.COM", "AQAAAAIAAYagAAAAEKhKgPLuSh6I8xZe/x+WyHyXiphNVT2/1mppogy0X2lv510pcGtFpr22bGERXnCZvA==", "Customer" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "NormalizedEmail", "Password", "Role" },
                values: new object[] { "SARA@GMAIL.COM", "AQAAAAIAAYagAAAAEI9pAV+19TzA1Blbt0DPTjNoPHjZ9Og8CSHeZqEX4iNYSyNu3Qsb/iNN63xmyhvaUw==", "Customer" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "NormalizedEmail", "Password", "Role" },
                values: new object[] { "OMAR@GMAIL.COM", "AQAAAAIAAYagAAAAENeCscWzBfbMGK0nvla29rD7/nc+az5LPNRZw9hEYdh38r5imVSV9A6Fjay4LIEFvw==", "Customer" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role",
                table: "Users",
                sql: "[Role] IN ('Admin', 'Customer')");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating",
                table: "Reviews",
                sql: "[Rating] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Price",
                table: "Products",
                sql: "[Price] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Stock",
                table: "Products",
                sql: "[Stock] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders",
                sql: "[Status] IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_TotalPrice",
                table: "Orders",
                sql: "[TotalPrice] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity",
                table: "OrderItems",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_UnitPrice",
                table: "OrderItems",
                sql: "[UnitPrice] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CartItems_Quantity",
                table: "CartItems",
                sql: "[Quantity] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Price",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Stock",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Status",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_TotalPrice",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_UnitPrice",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CartItems_Quantity",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
