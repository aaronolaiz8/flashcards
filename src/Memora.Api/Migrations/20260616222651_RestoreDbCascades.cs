using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memora.Api.Migrations
{
    /// <inheritdoc />
    public partial class RestoreDbCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardMemoryStates_Users_UserId",
                table: "CardMemoryStates");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalDecks_Decks_DeckId",
                table: "GoalDecks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLogs_StudySessions_SessionId",
                table: "ReviewLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_Goals_GoalId",
                table: "StudySessions");

            migrationBuilder.AddForeignKey(
                name: "FK_CardMemoryStates_Users_UserId",
                table: "CardMemoryStates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalDecks_Decks_DeckId",
                table: "GoalDecks",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLogs_StudySessions_SessionId",
                table: "ReviewLogs",
                column: "SessionId",
                principalTable: "StudySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_Goals_GoalId",
                table: "StudySessions",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardMemoryStates_Users_UserId",
                table: "CardMemoryStates");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalDecks_Decks_DeckId",
                table: "GoalDecks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLogs_StudySessions_SessionId",
                table: "ReviewLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_Goals_GoalId",
                table: "StudySessions");

            migrationBuilder.AddForeignKey(
                name: "FK_CardMemoryStates_Users_UserId",
                table: "CardMemoryStates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalDecks_Decks_DeckId",
                table: "GoalDecks",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLogs_StudySessions_SessionId",
                table: "ReviewLogs",
                column: "SessionId",
                principalTable: "StudySessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_Goals_GoalId",
                table: "StudySessions",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id");
        }
    }
}
