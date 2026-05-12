using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRooms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeadLetterToOutboxIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_IsProcessed_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "IsProcessed", "IsDeadLetter", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IsProcessed_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "IsProcessed", "OccurredOn" });
        }
    }
}
