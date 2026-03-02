using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormMaker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationFieldsToForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableNotifications",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NotificationEmail",
                table: "Forms",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableNotifications",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "NotificationEmail",
                table: "Forms");
        }
    }
}
