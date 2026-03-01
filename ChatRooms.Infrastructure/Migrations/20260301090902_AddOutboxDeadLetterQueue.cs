using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatRooms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxDeadLetterQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeadLetter",
                table: "OutboxMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeadLetter",
                table: "OutboxMessages");
        }
    }
}
