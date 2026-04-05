using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskListOwnerIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerIdentifier",
                table: "task_lists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerIdentifier",
                table: "task_lists");
        }
    }
}
