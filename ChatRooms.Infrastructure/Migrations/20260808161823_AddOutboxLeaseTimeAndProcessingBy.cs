using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRooms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxLeaseTimeAndProcessingBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingBy",
                table: "OutboxMessages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingLeaseUntil",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_ProcessingLeaseUnti~",
                table: "OutboxMessages",
                columns: new[] { "IsProcessed", "IsDeadLetter", "ProcessingLeaseUntil", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_ProcessingLeaseUnti~",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "ProcessingBy",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "ProcessingLeaseUntil",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IsProcessed_IsDeadLetter_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "IsProcessed", "IsDeadLetter", "OccurredOn" });
        }
    }
}
