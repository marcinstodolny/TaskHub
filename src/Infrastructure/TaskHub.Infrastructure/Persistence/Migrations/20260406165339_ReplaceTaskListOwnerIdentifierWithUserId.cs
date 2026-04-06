using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTaskListOwnerIdentifierWithUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "task_lists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE tl
                SET tl.UserId = u.Id
                FROM task_lists tl
                INNER JOIN users u ON u.Id = TRY_CONVERT(uniqueidentifier, tl.OwnerIdentifier)
                WHERE tl.UserId IS NULL;

                UPDATE tl
                SET tl.UserId = u.Id
                FROM task_lists tl
                INNER JOIN users u ON u.Username = tl.OwnerIdentifier
                WHERE tl.UserId IS NULL;

                IF EXISTS (SELECT 1 FROM task_lists WHERE UserId IS NULL)
                BEGIN
                    THROW 51000, 'Cannot automatically migrate task_lists ownership to UserId. Ensure matching users exist for every OwnerIdentifier value, then re-run the migration.', 1;
                END
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "task_lists",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_lists_UserId",
                table: "task_lists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_task_lists_users_UserId",
                table: "task_lists",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "OwnerIdentifier",
                table: "task_lists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_lists_users_UserId",
                table: "task_lists");

            migrationBuilder.DropIndex(
                name: "IX_task_lists_UserId",
                table: "task_lists");

            migrationBuilder.AddColumn<string>(
                name: "OwnerIdentifier",
                table: "task_lists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE tl
                SET tl.OwnerIdentifier = u.Username
                FROM task_lists tl
                INNER JOIN users u ON u.Id = tl.UserId
                WHERE tl.OwnerIdentifier IS NULL;
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

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "task_lists");
        }
    }
}
