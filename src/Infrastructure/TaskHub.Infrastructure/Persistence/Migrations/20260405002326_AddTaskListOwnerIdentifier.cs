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
                nullable: true);

            migrationBuilder.Sql(
                """
                 IF EXISTS (SELECT 1 FROM task_lists)
                 BEGIN
                     THROW 51000, 'Cannot apply TaskList.OwnerIdentifier migration automatically for existing task_lists data. Backfill OwnerIdentifier values manually, then re-run the migration.', 1;
                 END
                 """);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerIdentifier",
                table: "task_lists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
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
