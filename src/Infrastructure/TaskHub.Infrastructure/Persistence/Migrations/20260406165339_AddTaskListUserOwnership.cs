using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskListUserOwnership : Migration
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
                IF EXISTS (SELECT 1 FROM task_lists)
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM users)
                    BEGIN
                        THROW 51000, 'Cannot migrate task_lists to UserId ownership because no users exist. Seed or backfill users first, then re-run the migration.', 1;
                    END

                    IF (SELECT COUNT(*) FROM users) > 1
                    BEGIN
                        THROW 51000, 'Cannot automatically migrate task_lists to UserId ownership when multiple users exist. Manually backfill task_lists.UserId before re-running the migration.', 1;
                    END

                    UPDATE tl
                    SET tl.UserId = u.Id
                    FROM task_lists tl
                    CROSS JOIN users u
                    WHERE tl.UserId IS NULL;
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

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "task_lists");
        }
    }
}
